using EPGPAnalyze;
using System;
using System.Diagnostics;
using System.IO;

namespace EPGPCompare
{
	class Program
	{
		const bool DEBUG_LOGS = false;

		private static StreamWriter Log = null;

		static void Main(string[] args)
		{
			Console.WriteLine( "Choose Mode: " );
			Console.WriteLine( "    1. Overall" );
			Console.WriteLine( "    2. Simulate" );
			Console.WriteLine();
			Console.Write( "? " );

			using( Log = new StreamWriter( new FileStream( "log.txt", FileMode.Create ) ) ) {
				if( int.TryParse( Console.ReadLine(), out int choice ) ) {
					switch( choice ) {
						case 1:
							OverallStandings();
							break;
						case 2:
							Analyze();
							break;
					}
				}
			}

			Log = null;
		}

		static void WriteLine( string line = "" ) {
			if( Log != null ) {
				Log.WriteLine( line );
			}

			Console.WriteLine( line );
		}

		static void OverallStandings()
		{
			Standings standings = new Standings();

			standings.ProcessFile( "standings.txt" ).Wait();

			Console.Write( "Your name: " );
			string playername = Console.ReadLine().Trim();

			Standings.Entry yourEntry = standings.Entries.Find( ( entry ) => entry.Name.ToLower() == playername.ToLower() );
			if( yourEntry == null ) {
				Console.WriteLine( "Couldnt find player" );
				return;
			}

			WriteLine( $"You: { yourEntry.Name } -- { yourEntry.EP }/{ yourEntry.GP } -- { yourEntry.PR }" );
			WriteLine();

			foreach( Standings.Entry otherEntry in standings.Entries ) {
				if( otherEntry.Name == yourEntry.Name ) {
					continue;
				}

				if( otherEntry.PR == 0 ) {
					continue;
				}

				if( otherEntry.PR > yourEntry.PR ) {
					WriteLine( "You lose to '" + otherEntry.Name + "' because your PR is lower" );
					continue;
				}

				if( otherEntry.PR == yourEntry.PR ) {
					WriteLine( "You tie with '" + otherEntry.Name + "' because your PR is the same" );
					continue;
				}

				double PRDiff = yourEntry.PR - otherEntry.PR;
				double allowance = yourEntry.EP / otherEntry.PR - yourEntry.GP;

				WriteLine( $"If you spend {allowance:F2} GP you're PR will match { otherEntry.Name }'s ({ otherEntry.EP }/{ otherEntry.GP } -- { otherEntry.PR })" );
			}
		}

		static void Analyze() {
			Standings standings = new Standings();

			standings.ProcessFile( "standings.txt" ).Wait();

			Console.Write( "Your name: " );
			string playername = Console.ReadLine().Trim();

			Standings.Entry yourEntry = standings.Entries.Find( ( entry ) => entry.Name.ToLower() == playername.ToLower() );
			if( yourEntry == null ) {
				Console.WriteLine( "Couldnt find player" );
				return;
			}

			WriteLine( $"{ yourEntry.Name } -- { yourEntry.EP }/{ yourEntry.GP } -- { yourEntry.PR }" );
			WriteLine();

			Console.Write( "Points you want to spend: " );
			int points = 0;
			if( !int.TryParse( Console.ReadLine(), out points ) ) {
				Console.WriteLine( "Invalid points" );
				return;
			}

			yourEntry.GP += points;
			yourEntry.PR = double.Parse( $"{((double)yourEntry.EP / yourEntry.GP):F2}" );

			WriteLine( $"{ yourEntry.Name } -- { yourEntry.EP }/{ yourEntry.GP } -- { yourEntry.PR }" );
			WriteLine();

			Console.Write( "Competitor: " );
			string competitor = Console.ReadLine().Trim();

			if( competitor == "*" ) {
				AnalyzeAll( yourEntry, standings );
				return;
			}

			Standings.Entry otherEntry = standings.Entries.Find( ( entry ) => entry.Name.ToLower() == competitor.ToLower() );
			if( otherEntry == null ) {
				Console.WriteLine( "Couldnt find player" );
				return;
			}

			AnalyzeSingle( yourEntry, otherEntry );
		}

		private static void AnalyzeAll( Standings.Entry yourEntry, Standings standings ) {
			foreach( Standings.Entry entry in standings.Entries ) {
				Standings.Entry yourClone = new Standings.Entry();

				yourClone.Name = yourEntry.Name;
				yourClone.EP = yourEntry.EP;
				yourClone.GP = yourEntry.GP;
				yourClone.PR = yourEntry.PR;

				AnalyzeSingle( yourClone, entry );
			}
		}

		private static void AnalyzeSingle( Standings.Entry yourEntry, Standings.Entry otherEntry ) {
			WriteLine();
			WriteLine( "-----------------------------------------------" );
			
			WriteLine();
			WriteLine( $"Competitor: { otherEntry.Name } -- { otherEntry.EP }/{ otherEntry.GP } -- { otherEntry.PR }" );
			WriteLine();

			if( yourEntry.PR <= otherEntry.PR ) {
				WriteLine( "You immediately lose priority to your competitor after this award" );
				return;
			}

			int decay( int val, double percent, int baseVal = 0 ) {
				if( baseVal > 0 ) {
					return (int)Math.Max( Math.Floor( ( val - baseVal ) * ( 1.0 - percent ) + baseVal ), baseVal );
				}

				return (int)Math.Max( Math.Floor( val * ( 1.0 - percent ) ), 0 );
			}

			int iterations = 0;
			int baseGP = 150;
			int weeklyEP = 300;
			while( true ) {
				iterations++;

				yourEntry.EP += weeklyEP;
				otherEntry.EP += weeklyEP;

				yourEntry.EP = decay( yourEntry.EP, 0.1 );
				yourEntry.GP = decay( yourEntry.GP, 0.1, baseGP );
				yourEntry.PR = double.Parse( $"{((double)yourEntry.EP / yourEntry.GP):F2}" );

				otherEntry.EP = decay( otherEntry.EP, 0.1 );
				otherEntry.GP = decay( otherEntry.GP, 0.1, baseGP );
				otherEntry.PR = double.Parse( $"{((double)otherEntry.EP / otherEntry.GP):F2}" );

				if( DEBUG_LOGS ) {
					WriteLine( "Iteration: " + iterations );
					WriteLine( $"{ yourEntry.Name } -- { yourEntry.EP }/{ yourEntry.GP } -- { yourEntry.PR }" );
					WriteLine( $"{ otherEntry.Name } -- { otherEntry.EP }/{ otherEntry.GP } -- { otherEntry.PR }" );
					WriteLine();
				}

				if( otherEntry.PR >= yourEntry.PR ) {
					break;
				}
				
				if( otherEntry.PR == yourEntry.PR ) {
					break;
				}
			}

			WriteLine( $"It took { iterations } weeks for { otherEntry.Name } to catch you in PR after you spent GP." );
			WriteLine( "Assuming you both had identical raid attendance" );

			WriteLine();
			WriteLine( $"{ yourEntry.Name } -- { yourEntry.EP }/{ yourEntry.GP } -- { yourEntry.PR }" );
			WriteLine( $"{ otherEntry.Name } -- { otherEntry.EP }/{ otherEntry.GP } -- { otherEntry.PR }" );
			WriteLine();
		}
	}
}

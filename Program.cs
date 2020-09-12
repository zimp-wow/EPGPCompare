using EPGPAnalyze;
using System;
using System.Diagnostics;

namespace EPGPCompare
{
	class Program
	{
		const bool DEBUG_LOGS = false;

		static void Main(string[] args)
		{
			Console.WriteLine( "Choose Mode: " );
			Console.WriteLine( "    1. Overall" );
			Console.WriteLine( "    2. Simulate" );
			Console.WriteLine();
			Console.Write( "? " );

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

			Console.WriteLine( $"You: { yourEntry.Name } -- { yourEntry.EP }/{ yourEntry.GP } -- { yourEntry.PR }" );
			Console.WriteLine();

			foreach( Standings.Entry otherEntry in standings.Entries ) {
				if( otherEntry.Name == yourEntry.Name ) {
					continue;
				}

				if( otherEntry.PR == 0 ) {
					continue;
				}

				if( otherEntry.PR > yourEntry.PR ) {
					Console.WriteLine( "You lose to '" + otherEntry.Name + "' because your PR is lower" );
					continue;
				}

				if( otherEntry.PR == yourEntry.PR ) {
					Console.WriteLine( "You tie with '" + otherEntry.Name + "' because your PR is the same" );
					continue;
				}

				double PRDiff = yourEntry.PR - otherEntry.PR;
				double allowance = yourEntry.EP / otherEntry.PR - yourEntry.GP;

				Console.WriteLine( $"If you spend {allowance:F2} GP you're PR will match { otherEntry.Name }'s ({ otherEntry.EP }/{ otherEntry.GP } -- { otherEntry.PR })" );
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

			Console.WriteLine( $"{ yourEntry.Name } -- { yourEntry.EP }/{ yourEntry.GP } -- { yourEntry.PR }" );
			Console.WriteLine();

			Console.Write( "Points you want to spend: " );
			int points = 0;
			if( !int.TryParse( Console.ReadLine(), out points ) ) {
				Console.WriteLine( "Invalid points" );
				return;
			}

			yourEntry.GP += points;
			yourEntry.PR = double.Parse( $"{((double)yourEntry.EP / yourEntry.GP):F2}" );

			Console.WriteLine( $"{ yourEntry.Name } -- { yourEntry.EP }/{ yourEntry.GP } -- { yourEntry.PR }" );
			Console.WriteLine();

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
			Console.WriteLine();
			Console.WriteLine( "-----------------------------------------------" );
			
			Console.WriteLine();
			Console.WriteLine( $"Competitor: { otherEntry.Name } -- { otherEntry.EP }/{ otherEntry.GP } -- { otherEntry.PR }" );
			Console.WriteLine();

			if( yourEntry.PR <= otherEntry.PR ) {
				Console.WriteLine( "You immediately lose priority to your competitor after this award" );
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
					Console.WriteLine( "Iteration: " + iterations );
					Console.WriteLine( $"{ yourEntry.Name } -- { yourEntry.EP }/{ yourEntry.GP } -- { yourEntry.PR }" );
					Console.WriteLine( $"{ otherEntry.Name } -- { otherEntry.EP }/{ otherEntry.GP } -- { otherEntry.PR }" );
					Console.WriteLine();
				}

				if( otherEntry.PR >= yourEntry.PR ) {
					break;
				}
				
				if( otherEntry.PR == yourEntry.PR ) {
					break;
				}
			}

			Console.WriteLine( $"It took { iterations } weeks for { otherEntry.Name } to catch you in PR after you spent GP." );
			Console.WriteLine( "Assuming you both had identical raid attendance" );

			Console.WriteLine();
			Console.WriteLine( $"{ yourEntry.Name } -- { yourEntry.EP }/{ yourEntry.GP } -- { yourEntry.PR }" );
			Console.WriteLine( $"{ otherEntry.Name } -- { otherEntry.EP }/{ otherEntry.GP } -- { otherEntry.PR }" );
			Console.WriteLine();
		}
	}
}

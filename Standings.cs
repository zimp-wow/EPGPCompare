using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EPGPAnalyze
{
	class Standings
	{
		public List<Entry> Entries { get; set; } = new List<Entry>();

		public Standings() {
		}

		public async Task ProcessFile( string file ) {
			
			using( StreamReader sr = new StreamReader( file ) ) {
				while( !sr.EndOfStream ) {
					string line  = await sr.ReadLineAsync();

					try {
						Entry entry = new Entry( line );

						Entries.Add( entry );
					}
					catch( Exception e ) {
						Console.WriteLine( "Failed to parse line: " + line + " - " + e );
					}
				}
			}

			Entries.Sort( ( left, right ) => { return right.PR.CompareTo( left.PR ); } );
		}

		public class Entry {
			public string   Name    { get; set; }
			public string   Class   { get; set; }
			public string   Role    { get; set; }
			public int      EP      { get; set; }
			public int      GP      { get; set; }
			public double   PR      { get; set; }

			public Entry() {
			}

			public Entry( string line ) {

				string[] comps = line.Split( ',' );
				if( line.Length < 3 ) {
					throw new Exception( "Unexpected number of fields" );
				}

				Name  = string.Empty;
				// Stripping any non-ascii characters from the name, thanks Auslander
				foreach( char c in comps[0].ToCharArray() ) {
					if( c <= sbyte.MaxValue ) {
						Name += c;
					}
				}

				Class = comps[1];
				Role  = comps[2];
				EP = 0;
				GP = 0;
				PR = 0.0f;

				if( comps.Length < 4 ) {
					return;
				}

				if( string.IsNullOrWhiteSpace( comps[3] ) ) {
					comps[3] = "0";
				}

				EP = int.Parse( comps[3] );

				if( comps.Length < 5 ) {
					return;
				}

				if( string.IsNullOrWhiteSpace( comps[4] ) ) {
					comps[4] = $"{ 0 }";
				}

				GP = int.Parse( comps[4] );

				if( comps.Length < 6 ) {
					return;
				}

				if( string.IsNullOrWhiteSpace( comps[5] ) ) {
					comps[5] = "0";
				}

				PR = double.Parse( comps[5] );
			}
		}
	}
}

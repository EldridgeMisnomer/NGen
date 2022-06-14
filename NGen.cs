using System;
using System.Collections.Generic;

namespace NGen {
    public class NGen {

        /*
         *  Top-level holder for all the gens.
         *  This is what will be created from a succesfully parsed Text file
         */

        public readonly Dictionary<string, OutputGen> gens = new Dictionary<string, OutputGen>();

        public string[] mainGens = null;

        public Dictionary<char, char> remapDict = null;


        public NGen( Dictionary<string, OutputGen> namedGens ) {
            gens = namedGens;

            //Check for mainGens
            List<string> tempMain = new List<string>();
            foreach( KeyValuePair<string, OutputGen> g in gens ) {
                if( g.Value.gs.isMain ) {
                    tempMain.Add( g.Key );
                }
            }
            if( tempMain.Count > 0 ) {
                mainGens = tempMain.ToArray();
            }

        }

        public NGen() { }

        public bool HasMainGens() {
            return mainGens != null;
        }

        public string GenTxt( string name, params string[] tags ) {

            if( gens.ContainsKey( name ) ) {

                return gens[name].GetTxt( tags );

            } else {
                Console.WriteLine( $"NGen Error: This NGen did not have a Gen named {name} in it" );
                return "";
            }

        }

        public string GenTxt( string name ) {

            if( gens.ContainsKey( name ) ) {

                return gens[name].GetTxt();

            } else {
                Console.WriteLine( $"NGen Error: This NGen did not have a Gen named {name} in it" );
                return "";
            }

        }

        public string[] GenTxt( string name, int number ) {

            string[] output = new string[number];

            for( int i = 0; i < number; i++ ) {
                output[i] = GenTxt( name );
            }

            return output;

        }

        public string[][] GenAll( int number = 10, bool onlyMain = false ) {

            int count = onlyMain ? mainGens.Length : gens.Count;

            string[][] output = new string[count][];

            string[] keys = GetGenNames();

            for( int i = 0; i< output.Length; i++ ) {

                output[i] = new string[number];
                for( int j = 0; j < number; j++ ) {

                    if( onlyMain ) {

                        output[i][j] = gens[mainGens[j]].GetTxt();

                    } else {

                        output[i][j] = gens[keys[j]].GetTxt();

                    }
                }
            }

            return output;

        }


        public string[] GetGenNames( bool onlyMain = false ) {

            if( onlyMain ) {
                if( mainGens != null ) {
                    return mainGens;
                } else {
                    //TODO - maybe put an error in here?
                    return null;
                }
            } else {
                return new List<string>(gens.Keys).ToArray();
            }


        }

        public void TurnIntoJSON() {

            FileWrangler.NGenToJSON( this );

        }

    }
}

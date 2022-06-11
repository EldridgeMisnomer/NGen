using System;
using System.Collections.Generic;

namespace NGen {
    public class NGen {

        /*
         *  Top-level holder for all the gens.
         *  This is what will be created from a succesfully parsed Text file
         */

        public readonly Dictionary<string, OutputGen> gens = new Dictionary<string, OutputGen>();

        public NGen( Dictionary<string, OutputGen> namedGens ) {
            gens = namedGens;
        }

        public NGen() { }

        public string GenTxt( string name, params string[] tags ) {

            //TODO handle tags

            return GenTxt( name );
            
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


        public string[] GetGenNames() {

            return new List<string>(gens.Keys).ToArray();

        }

        public void TurnIntoJSON() {

            FileHandler.NGenToJSON( this );

        }

    }
}

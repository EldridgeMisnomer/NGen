﻿using System;
using System.Collections.Generic;

namespace NGen {
    public class NGen {

        private readonly Dictionary<string, SenGen> gens = new Dictionary<string, SenGen>();
        /*
         *  Top-level holder for all the gens.
         *  This is what will be created from a succesfully parsed Text file
         */


        public NGen( Dictionary<string, SenGen> namedGens ) {
            gens = namedGens;
        }

        public string GenTxt( string name ) {

            if( gens.ContainsKey(name) ) {

                return gens[name].GetTxt();

            } else {
                Console.WriteLine( $"Error: This NGen did not have a Gen named {name} in it" );
                return "";
            }

        }

        public string[] GetGenNames() {

            return new List<string>(gens.Keys).ToArray();

        }

    }
}

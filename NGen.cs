using System;
using System.Collections.Generic;

namespace NGen {
    public class NGen {

        /*
         *  Top-level holder for all the gens.
         *  This is what will be created from a succesfully parsed Text file
         */

        public readonly Dictionary<string, SenGen> gens = new Dictionary<string, SenGen>();

        //tags
        public bool useTags = false;
        public readonly Dictionary<string, Dictionary<string, SenGen>> taggedGens = new Dictionary<string, Dictionary<string, SenGen>>();

        public NGen( Dictionary<string, SenGen> namedGens ) {
            gens = namedGens;
        }

        public NGen() { }

        /*
         *  Four ways to request a tagged NGen
         *      1) - no tags - it'll pick a random one of the ones available
         *      2) - list of possible tags - it'll pick a random one from the tags listed
         *      3) - list of required tags - it'll pick the one that has all the tags
         *      4) - list of lists - one tag from each list is required, eg:
         *              name ( m, f ), ( posh, poor )
         *                  it'll look for the following options:
         *                      m posh
         *                      m poor
         *                      f posh
         *                      f poor
         *  
         *  At the moment the NGen will do its utmost to return some txt
         *  Even when tags are required, it'll still look for a Gen with
         *  no tags with the correct name in order to return some txt
         *  Maybe this is not always the best option, if the tags are not present
         *  maybe it should fail and return nothing
         *  
         *  I think I will add this in future, but as an option (TODO)
         */

        public string GenTxt( string name, params string[] possibleTags ) {

            if( useTags ) {

                List<SenGen> gens = FindGensWithNameAndTags( name, possibleTags );

                if( gens.Count > 0 ) {

                    //TODO - at some point we may add some options for how this is picked,
                    //for now it's just uniform random
                    int choice = Utils.Rand.RandomRangeInt( 0, gens.Count );
                    return gens[choice].GetTxt();

                } else {

                    if( taggedGens["none"].ContainsKey(name) ) {

                        return taggedGens["none"][name].GetTxt();

                    } else {

                        Console.WriteLine( $"NGen Error: This NGen did not have any Gens named {name} in it" );
                        return "";

                    }
                }

            } else {

                return GenTxt( name );

            }
        }

        /*
         *  eg: below we should end up with
         *  onea1, onea2, onea3, oneb1, oneb2, oneb3, onec1, onec2, onecc3
         *  twoa1, twoa2, twoa3, twob1, twob2, twob3, twoc1, twoc2, twocc3
         *  threea1, threea2, threea3, threeb1, threeb2, threeb3, threec1, threec2, threecc3
         *  
         */

        private void Test() {

            string[][] tags = {
                new string[] { "one", "two", "three" },
                new string[] { "a", "b", "c" },
                new string[] { "1", "2", "3" }
            };

            string n = "name";

            string txt = GenTxt( n, tags );

        }

        public string GenTxt( string name, string[][] tags ) {

            string s= "";

            int tot = tags.Length;

            for( int i = 0; i < tags.Length; i++ ) {
                tot *= tags[i].Length;
            }

            string[] combinedTags = new string[tot];

            int ind = 0;

            for( int i = 0; i < tags.Length; i++ ) {

                for( int j = 0; j < tags[i].Length; j++ ) {



                    ind++;

                }

            }





            return s;

        }

        public string GenTxtStrict( string name, params string[] requiredTags ) {

            //if there's more than one tag we have to sort them alphabetically
            if( requiredTags.Length > 0 ) {
                Array.Sort( requiredTags ); 
            }

            //collect all the tags into one combined tag
            string tag = "";
            foreach( string t in requiredTags ) {
                tag += t.Trim();
            }

            return GenTxt( name, tag );
        }


        public string GenTxt( string name ) {

            if( useTags ) {

                //we haven't been supplied a tag
                //so let's look in none first
                //if there's a gen with this name in none, return its text
                if( taggedGens["none"].ContainsKey( name ) ) {

                    return taggedGens["none"][name].GetTxt();

                //otherwise, look through all the other tags to find gens with that name
                //pick one at random, and return its text
                } else {

                    List<SenGen> gens = FindGensWithName( name );

                    if( gens.Count > 0 ) {

                        int choice = Utils.Rand.RandomRangeInt( 0, gens.Count );
                        return gens[choice].GetTxt();

                    } else {

                        Console.WriteLine( $"NGen Error: This NGen did not have any Gens named {name} in it" );
                        return "";

                    }
                }

            } else {
                if( gens.ContainsKey( name ) ) {

                    return gens[name].GetTxt();

                } else {
                    Console.WriteLine( $"NGen Error: This NGen did not have a Gen named {name} in it" );
                    return "";
                }
            }
        }


        public string[] GetGenNames() {

            return new List<string>(gens.Keys).ToArray();

        }

        public void TurnIntoJSON() {

            FileHandler.NGenToJSON( this );

        }


        private List<SenGen> FindGensWithName( string name ) {

            List<SenGen> gens = new List<SenGen>();
            List<string> keys = new List<string>( taggedGens.Keys );

            foreach( string k in keys ) {

                if( k != "none" ) {

                    if( taggedGens[k].ContainsKey( name ) ) {

                        gens.Add( taggedGens[k][name] );

                    }
                }
            }

            return gens;
        }

        private List<SenGen> FindGensWithNameAndTags( string name, string[] possibleTags ) {

            List<SenGen> gens = new List<SenGen>();
            foreach( string t in possibleTags ) {

                if( taggedGens.ContainsKey( t ) ) {

                    if( taggedGens[t].ContainsKey( name ) ) {

                        gens.Add( taggedGens[t][name] );

                    }
                }
            }

            return gens;

        }
    }
}

using System;
using System.Collections.Generic;

using PU = NGen.ParserUtils;

namespace NGen {

    public static class GenProcessors {

        private static List<ProxyGen> proxyGens = new List<ProxyGen>();

        public static List<ProxyGen> GetProxyGens() {
            return proxyGens;
        }


        public static Gen WrdProcessor( string s, GenSettings headerSettings ) {
            /*
             *  Takes any string which would normally be treated as a Wrd
             *      ie - it doesn't have any complicated construction - like lists - in it
             *  and checks to see if it has any ProxyGens in it,
             *  it then returns either a Wrd or a SenGen accordingly
             *  
             *  TODO I think this could be optimised better
             *  instead of creating a Wrd for every Wrd is should
             *  instead group words between ProxyGens and shove them all together
             *  into one Wrd
             */

            if( PU.StringContainsRef( s ) ) {
                //split the string based on spaces
                char[] separators = { ' ' };
                string[] words = s.Split( separators, StringSplitOptions.RemoveEmptyEntries );

                List<Gen> gens = new List<Gen>();

                foreach( string w in words ) {

                    if( PU.StringContainsRef( w ) ) {

                        int refIndex = w.IndexOf( PU.CharMap( CharType.reference ) );
                        string name = w.Substring( refIndex + 1, w.Length - refIndex - 1 );
                        ProxyGen pg = ProxyProcessor( name );
                        gens.Add( pg );

                    } else {
                        Wrd wrd = new Wrd( w );
                        gens.Add( wrd );
                    }
                }

                SenGen sg = new SenGen( gens.ToArray(), headerSettings );
                return sg;

            } else {
                return new Wrd( s );
            }

        }

        public static ProxyGen ProxyProcessor( string s ) {

            ProxyGen pg = new ProxyGen( s.Trim() );
            //add to the ProxyGen list for connecting up later
            proxyGens.Add( pg );
            return pg;

        }

        public static Gen[] MultiWrdProcessor( string[] s, GenSettings genSettings ) {
            /*
             *  Processes an array of potential Wrds
             *  Checking for ProxyGens
             */

            Gen[] gens = new Gen[s.Length];

            for( int i = 0; i < s.Length; i++ ) {
                gens[i] = WrdProcessor( s[i], genSettings );
            }

            return gens;
        }


        public static ListGen ListGenProcessor( string[] sArr, GenSettings headerSettings ) {
            /*
             *  receives the contents of square brackets and turns them into a ListGen
             */

            //check to see if there are nested lists
            bool containsLists = false;
            foreach( string s in sArr ) {
                containsLists = PU.StringContinsList( s );
                if( containsLists ) {
                    break;
                }
            }

            ListGen wg;

            if( containsLists ) {
                //Console.WriteLine( "WrdGenProcessor says YES, there are nested list(s)" );
                List<Gen> gens = new List<Gen>();

                foreach( string s in sArr ) {
                    if( PU.StringContinsList( s ) ) {

                        Gen g = SenGenProcessor( s, headerSettings );
                        gens.Add( g );

                    } else {
                        Gen g = WrdProcessor( s, headerSettings );
                        gens.Add( g );
                    }
                }

                wg = new ListGen( gens.ToArray(), headerSettings );

            } else {

                //if there are no nested lists, create a simple ListGen out of the strings
                Gen[] gens = MultiWrdProcessor( sArr, headerSettings );
                wg = new ListGen( gens, headerSettings );

            }

            return wg;
        }

        public static Gen SenGenProcessor( string s, GenSettings headerSettings ) {
            /*
             * This will take a string and turn it into a SenGen,
             * although, if it is a simple sentence it may return
             * either a ListGen or a Wrd instead
             */

            //Create a list to contain gens, this will eventually be turned into the SenGen
            List<Gen> gens = new List<Gen>();

            //Check to see if we have any ListGens
            if( PU.StringContinsList( s ) ) {

                string preBracketStart;
                string postBracketStart = PU.GetBracketsStart( s, out preBracketStart );

                //If there is any text in the preBrackets string
                //we can treat it as just a Wrd, because we know it can't contain a list
                if( preBracketStart.Trim().Length > 0 ) {
                    Gen g = WrdProcessor( preBracketStart.Trim(), headerSettings );
                    gens.Add( g );
                }

                string postBrackets;
                string[] bracketsContents = PU.GetBracketsContents( postBracketStart, out postBrackets );

                ListGen wg = ListGenProcessor( bracketsContents, headerSettings );
                gens.Add( wg );

                //process postBrackets text
                //we don't know if there are any lists in here, so we need to check
                //If there is a list in here, we send it back into the SenGenProcessor
                if( postBrackets.Trim().Length > 0 ) {

                    if( PU.StringContinsList( postBrackets ) ) {

                        Gen pbg = SenGenProcessor( postBrackets, headerSettings );
                        gens.Add( pbg );

                    } else {

                        //if there's no list there, treat it as a Wrd
                        //and add it to the list
                        Gen g = WrdProcessor( postBrackets.Trim(), headerSettings );
                        gens.Add( g );
                    }
                }

            } else {

                //If there are no WrdLists, then dump the whole string into a Wrd
                //DEBUG
                //Console.WriteLine( $"string had no lists: \"{s}\"" );
                Gen g = WrdProcessor( s.Trim(), headerSettings );
                gens.Add( g );

            }

            //if only one gen has been created, return it,
            //otherwise create a SenGen and return that
            if( gens.Count == 1 ) {

                return gens[0];

            } else {

                SenGen sg = new SenGen( gens.ToArray(), headerSettings );
                return sg;

            }
        }
    }
}

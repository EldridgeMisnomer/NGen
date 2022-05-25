using System;
using System.Collections.Generic;
using System.IO;

using PU = NGen.ParserUtils;

namespace NGen {

    public static class DataGetter {

        private static List<ProxyGen> proxyGens = new List<ProxyGen>();

        public static NGen ParseTxtFile( string path ) {

            //Get the file as an array of lines
            string[] lines = GetDataFromTxt( path );
            //Remove the comments
            string[] strippedLines = PU.StripComments( lines );
            //convert into pairs of strings - names and ? sentences
            //add them to a dictionary

            //TODO - deal with multiline names
            //TODO - check for duplicate names
            //TODO - escaping characters
            //TODO - repeats
            //TODO - variable %
            //TODO - header
            //TODO - set allow repeat elements from lists

            Dictionary<string, Gen> gens = new Dictionary<string, Gen>();

            foreach( string l in strippedLines ) {

                if( l.Length > 0 ) {

                    string n;
                    string c;
                    PU.StringToStringPair( l, out n, out c );

                    Gen g = SenGenProcessor( c );
                    gens.Add( n, g );
                }
            }

            //Check all ProxyGens to see if their genNames are in the dictionary
            //if they are, pass them their gens
            if( proxyGens.Count > 0 ) {
                for( int i = 0; i < proxyGens.Count; i++ ) {
                    string name = proxyGens[i].GetName();

                    if( gens.ContainsKey( name ) ) {

                        proxyGens[i].SetGen( gens[name] );

                    } else {
                        Console.WriteLine( $"Error: ProxyGen name \"{name}\" has not been created" );
                    }
                }
            }

            proxyGens = new List<ProxyGen>();


            NGen nGen = new NGen( gens );


            return nGen;

        }

        private static Gen[] MultiWrdProcessor( string[] s ) {
            /*
             *  Processes an array of potential Wrds
             *  Checking for ProxyGens
             */

            Gen[] gens = new Gen[s.Length];

            for( int i = 0; i < s.Length; i++ ) {
                gens[i] = WrdProcessor( s[i] );
            }

            return gens;
        }

        private static Gen WrdProcessor( string s ) {
            /*
             *  Takes any string which would normally be treated as a Wrd
             *      ie - it doesn't have any complicated construction - like lists - in it
             *  and checks to see if it has any ProxyGens in it,
             *  it then either returns a Wrd or a SenGen accordingly
             */

            if(PU.StringContainsRef( s ) ) {
                //split the string based on spaces
                char[] separators = { ' ' };
                string[] words = s.Split( separators, StringSplitOptions.RemoveEmptyEntries );

                List<Gen> gens = new List<Gen>();

                foreach( string w in words ) {
                    if( PU.StringContainsRef( w ) ) {

                        int refIndex = w.IndexOf( PU.CharacterMap( CharType.reference ) );
                        string name = w.Substring( refIndex + 1, w.Length - refIndex - 1 );
                        ProxyGen pg = new ProxyGen( name.Trim() );
                        //add to the ProxyGen list for connecting up later
                        proxyGens.Add( pg );
                        gens.Add( pg );

                    } else {
                        Wrd wrd = new Wrd( w );
                        gens.Add( wrd );
                    }
                }

                SenGen sg = new SenGen( gens.ToArray() );
                return sg;

            } else {
                return new Wrd( s );
            }

        }



        private static Gen SenGenProcessor( string s ) {
            /*
             * This will take a string and turn it into a SenGen,
             * although, if it is a simple sentence it may return
             * either a ListGen or a Wrd instead
             */

            //Create a list to contain gens, this will eventually be turned into the SenGen
            List<Gen> gens = new List<Gen>();

            //Check to see if we have any WrdGens
            if( PU.StringContinsList(s) ) {

                string preBracketStart;
                string postBracketStart = GetBracketsStart( s, out preBracketStart );

                //If there is any text in the preBrackets string
                //we can treat it as just a Wrd, because we know it can't contain a list
                if( preBracketStart.Trim().Length > 0 ) {
                    Gen g = WrdProcessor(preBracketStart.Trim() );
                    gens.Add( g );
                }

                string postBrackets;
                string[] bracketsContents = GetBracketsContents( postBracketStart, out postBrackets );

                ListGen wg = WrdGenProcessor( bracketsContents );
                gens.Add( wg );

                //process postBrackets text
                //we don't know if there are any lists in here, so we need to check
                //If there is a list in here, we send it back into the SenGenProcessor
                if( postBrackets.Trim().Length > 0 ) {

                    if( PU.StringContinsList( postBrackets ) ) {

                        Gen pbg = SenGenProcessor( postBrackets );
                        gens.Add( pbg );

                    } else {

                        //if there's no list there, treat it as a Wrd
                        //and add it to the list
                        Gen g = WrdProcessor( postBrackets.Trim() );
                        gens.Add( g );
                    }
                }

            } else {

                //If there are no WrdLists, then dump the whole string into a Wrd
                //DEBUG
                //Console.WriteLine( $"string had no lists: \"{s}\"" );
                Gen g = WrdProcessor( s.Trim() );
                gens.Add( g );

            }

            //if only one gen has been created, return it,
            //otherwise create a SenGen and return that

            if( gens.Count == 1 ) {

                return gens[0];

            } else {

                SenGen sg = new SenGen( gens.ToArray() );
                return sg;
            }
        }

        private static ListGen WrdGenProcessor( string[] sArr ) {
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

                        Gen g = SenGenProcessor( s );
                        gens.Add( g );

                    } else {
                        Gen g = WrdProcessor( s );
                        gens.Add( g );
                    }
                }

                wg = new ListGen( gens.ToArray() );

            } else {

                //if there are no nested lists, create a simple ListGen out of the strings
                Gen[] gens = MultiWrdProcessor( sArr );
                wg = new ListGen( gens );

            }

            return wg;
        }

        private static string GetBracketsStart( string s, out string preBrackets ) {
            /*
             * finds the starting point of the brackets and returns the input string
             * minus anything before the start, also outputs to prefix separately
             */
            preBrackets = "";
            string contents = "";
            bool started = false;

            foreach( char c in s ) {
                if( started ) {
                    contents += c;
                } else {

                    if( c == PU.CharacterMap( CharType.openList ) ) {

                        started = true;

                    } else {

                        preBrackets += c;

                    }

                }
            }

            return contents;
        }


        private static string[] GetBracketsContents( string s, out string postBrackets ) {
            /*
             *  This method is given a string which starts at the beginning of a set of brackets
             *  but it doesn't know where the brackets end
             *  so
             *  it goes through the string character by character
             *  and counts opening and closing brackets as it goes
             *  when the number of opening and closing brackets are equal
             *  it knows it has reached the end of the brackets,
             *  no matter how many nested brackets there are,
             *  and so it can separate out the contents of the brackets
             *  from whatever comes after it
             *  
             *  while the number of opening brackets is 1 greater than the number of closing brackets
             *  it knows that it is on the first level (ie no nesting has occured)
             *  and so it can split the string using commas (',')
             *  into its different elements, putting them into an array
             *  when the number is higher than 1
             *  it can sefely ignore commas
             */

            //count the number of opening and closing brackets,
            //openCount starts at 1 because the first opening bracket
            //is not passed in
            int openCount = 1;
            int closeCount = 0;

            //a list to contain the comma-separated elements
            List<string> contents = new List<string>();
            //temp string to contain the current element
            string tempContents = "";
            //string to place everything following the brackets
            postBrackets = "";
            //flag to let us know once the brackets are comlete
            bool complete = false;

            //DEBUG
            //Console.WriteLine( "getting contents of brackets:" );
            //Console.WriteLine( "\tinput string is:" );
            //Console.WriteLine( $"\t\t\"{s}\"" );

            //go through the string one character at a time
            foreach( char c in s ) {
                //Console.WriteLine( $"char is '{c}'" );
                //if the brackets have already been completed, add the character
                //to the postBrackets string
                if( complete ) {
                    postBrackets += c;
                } else {

                    //increment the counters
                    if( c == PU.CharacterMap( CharType.closeList ) ) {
                        closeCount++;
                    } else if( c == PU.CharacterMap( CharType.openList ) ) {
                        openCount++;
                    }

                    //check to see if the brackets have been closed
                    if( openCount == closeCount ) {

                        contents.Add( tempContents );
                        complete = true;

                    } else {

                        //separate by commas, unless we are above level 1 in the nesting
                        if( c == PU.CharacterMap( CharType.listSeparator ) &&
                            openCount - 1 == closeCount ) {

                                contents.Add( tempContents );
                                tempContents = "";

                        } else {

                            tempContents += c;

                        }

                    }
                }
            }

            //DEBUG
            //Console.WriteLine( "\toutput string is:" );
            //for( int i = 0; i < contents.Count; i++ ) {
            //    Console.WriteLine( $"\t\t {i}: {contents[i]}" );
            //}

            return contents.ToArray();


        }



        public static string[] GetDataFromTxt( string path ) {

            /*
             *  Retrieves a text file and returns it as a string array
             */

            if( File.Exists( path ) ) {

                return File.ReadAllLines( path );

            } else {

                Console.WriteLine( $"Get Data From Text File Failed: path ({path}) is invalid" );
                return null;

            }

        }
    }
}

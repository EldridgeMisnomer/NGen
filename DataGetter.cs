using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NGen {
    public static class DataGetter {

        public static void ParseTxtFile( string path ) {

            //Get the file as an array of lines
            string[] lines = GetDataFromTxt( path );
            //Remove the comments
            string[] strippedLines = StripComments( lines );
            //convert into pairs of strings - names and ? sentences
            //add them to a dictionary

            Dictionary<string, Gen> gens = new Dictionary<string, Gen>();

            foreach( string l in strippedLines ) {

                if( l.Length > 0 &&
                    l.Contains( "[" ) &&
                    l.Contains( "]" ) ) {

                    string n;
                    string c;
                    StringToStringPair( l, out n, out c );

                    Gen g = SenGenProcessor( c );
                    gens.Add( n, g );
                }
            }


            //Test Code
            int numTestToRunPerName = 5;
            foreach( KeyValuePair<string, Gen> g in gens ) {
                Console.WriteLine( g.Key + ":" );
                for( int i = 0; i < numTestToRunPerName; i++ ) {
                    Console.WriteLine( "\t" + g.Value.GetTxt() );
                }
            }

        }

        
        private static void StringToStringPair( string s, out string name, out string contents ) {

            int divide = s.IndexOf( '=' );
            name = s.Substring( 0, divide ).Trim();
            contents = s.Substring( divide + 1, s.Length - divide - 1);

        }

        /*
         *  I'm having a lot of trouble thinking about how to parse these strings
         *  So I'm going to try and go through it here to get it clear what I'm doing
         *  
         *  given a string like this:
         *      pre [ something1, something2, [nested1, nested2, nested3] ] [again1, again2] post
         *      
         *  OR:
         *      pre, [ something1, something2], [again1, again2]
         *  
         *  NO - actually let's enforce the rule that ALL lists must be in brackets ([])
         *  
         *  We're going to create an empty sentence
         *  
         *  We step through it character by character 
         *  adding the characters to a string
         *  until we get to an '['
         *  everything we've already got we know is a Wrd in a Sentence.
         *  so we make a word out of it and add it to an array/list for later
         *  
         *  things which come after the bracket are going to be a list, so we need to get all the
         *  contents of these brackets with the GetBracktetsContents() method
         *  
         *  GetBracketsContents() will give us back a string with the contents of the brackets,
         *  We send that to the List processor which will return a List
         *  
         *  We add that list to our array for later
         *  GetBracketsContents() also returns the text AFTER the brackets
         *  We start going through that character by character as before, now treating it as a new Wrd
         *  in a Sentence, until we get to some brackets again
         *  And repeat
         *  
         *  The List processor will separate everything by comma (,)
         *  And then it will check every element to see if it contains any brackets. 
         *  If it doesn't then it's a Wrd, and if it does then it's a list
         *  Or at least partly a list 
         *      - it'll separate anything out before and after the brackets
         *      and, if there are things before and after - create a sentence of three elements
         *          - before, list, after
         *      if there aren't then it'll just take the output of the list
         *      
         *  on second thoughts, maybe we do the checking for if there are brackets
         *  in the sentence processor BEFORE it gets to the list
         *  
         */

        private static Gen SenGenProcessor( string s ) {
            /*
             * This will take a string and turn it into a Sentence,
             * although potentially a sentence with only one element
             * 
             */

            //Create a list to contain gens, this will eventually be turned into the SenGen
            List<Gen> gens = new List<Gen>();

            //Check to see if we have any WrdGens
            if( StringContinsList(s) ) {

                string preBracketStart;
                string postBracketStart = GetBracketsStart( s, out preBracketStart );

                //If there is any text in the preBrackets string
                //we can treat it as just a Wrd, because we know it can't contain a list
                if( preBracketStart.Trim().Length > 0 ) {
                    Wrd w = new Wrd( preBracketStart.Trim() );
                    gens.Add( w );
                }

                string postBrackets;
                string[] bracketsContents = GetBracketsContents( postBracketStart, out postBrackets );

                ListGen wg = WrdGenProcessor( bracketsContents );
                gens.Add( wg );

                //process postBrackets text
                //we don't know if there are any lists in here, so we need to check
                //If there is a list in here, we send it back into the SenGenProcessor
                if( postBrackets.Trim().Length > 0 ) {

                    if( StringContinsList( postBrackets ) ) {

                        Gen pbg = SenGenProcessor( postBrackets );
                        gens.Add( pbg );

                    } else {

                        //if there's no list there, treat it as a Wrd
                        //and add it to the list
                        Wrd w = new Wrd( postBrackets.Trim() );
                        gens.Add( w );
                    }
                }

            } else {

                //If there are no WrdLists, then dump the whole string into a Wrd
                Wrd w = new Wrd( s.Trim() );
                gens.Add( w );

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
                containsLists = StringContinsList( s );
                if( containsLists ) {
                    break;
                }
            }

            ListGen wg;

            if( containsLists ) {
                //Console.WriteLine( "WrdGenProcessor says YES, there are nested list(s)" );
                List<Gen> gens = new List<Gen>();

                foreach( string s in sArr ) {
                    if( StringContinsList( s ) ) {

                        Gen g = SenGenProcessor( s );
                        gens.Add( g );

                    } else {
                        Wrd w = new Wrd( s );
                        gens.Add( w );
                    }
                }

                wg = new ListGen( gens.ToArray() );

            } else {

                //if there are no nested lists, create a simple ListGen out of the strings
                wg = new ListGen( sArr );

            }
            return wg;
        }

        private static bool StringContinsList( string s ) {
            return s.Contains( '[' );
        }

        private static string GetBracketsStart( string s, out string preBrackets ) {

            preBrackets = "";
            string contents = "";
            bool started = false;

            foreach( char c in s ) {
                if( started ) {
                    contents += c;
                } else {

                    if( c == '[' ) {

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
            Console.WriteLine( "getting contents of brackets:" );
            Console.WriteLine( "\tinput string is:" );
            Console.WriteLine( $"\t\t\"{s}\"" );

            //go through the string one character at a time
            foreach( char c in s ) {
                //Console.WriteLine( $"char is '{c}'" );
                //if the brackets have already been completed, add the character
                //to the postBrackets string
                if( complete ) {
                    postBrackets += c;
                } else {

                    //increment the counters
                    if( c == ']' ) {
                        closeCount++;
                    } else if( c == '[' ) {
                        openCount++;
                    }

                    //check to see if the brackets have been closed
                    if( openCount == closeCount ) {

                        contents.Add( tempContents );
                        complete = true;

                    } else {

                        //separate by commas, unless we are above level 1 in the nesting
                        if( c == ',' &&
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
            Console.WriteLine( "\toutput string is:" );
            for( int i = 0; i < contents.Count; i++ ) {
                Console.WriteLine( $"\t\t {i}: {contents[i]}" );
            }

            return contents.ToArray();


        }

        private static string[] StringListToArray( string s ) {

            /*
             *  Splits the elements of a comma-separated list into an array
             *  removes empty elements
             *  trims each element
             */

            char[] separators = { ',' };
            string[] wrds = s.Split( separators, StringSplitOptions.RemoveEmptyEntries );

            for( int i = 0; i < wrds.Length; i++ ) {
                wrds[i] = wrds[i].Trim();
            }

            return wrds;
        }

        private static string[] StripComments( string[] ls ) {

            /*
             *  Receives an array of strings
             *  and returns an array including only those strings
             *  which do not begin with a # symbol
             *  
             *  also removes empty lines
             */

            List<string> strippedL = new List<string>();

            foreach( string l in ls ) {

                if( l.Trim().Length > 0 ) {

                    if( l[0] != '#' ) {
                        strippedL.Add( l );
                    }
                }
            }


            return strippedL.ToArray();

        }

        public static string[] GetDataFromTxt( string path ) {

            /*
             *  Retrieves a text file and returns it as a string array
             */

            if( File.Exists( path ) ) {

                return File.ReadAllLines( path );

            } else {

                System.Console.WriteLine( $"Get Data From Text File Failed: path ({path}) is invalid" );
                return null;

            }

        }
    }
}

using System;
using System.Collections.Generic;

using PU = NGen.ParserUtils;

namespace NGen {

    public static class DataGetter {

        private static List<ProxyGen> proxyGens = new List<ProxyGen>();

        public static NGen ParseTxtFile( string path ) {

            //settings for new ListGens
            //GenSettings defaultSettings = new GenSettings();
            //GenSettings currentSettings;

            //Get the file as an array of lines
            string[] lines = PU.GetDataFromTxt( path );
            //Remove the comments
            string[] strippedLines = PU.StripComments( lines );
            //process the lines into Gens with names
            Dictionary<string, Gen> gens = HeaderProcessor( strippedLines );

            //TODO - check for duplicate names
            //TODO - repeats
            //TODO - variable %
            //TODO - set allow repeat elements from lists
            //TODO - remap special characters
            //TODO - header reset defaults
            //TODO - weighted randoms


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

        private static Dictionary<string, Gen> HeaderProcessor( string[] lines ) {
            /*
             *  receives the comment-stripped lines from the text file
             *  and divides them up into sections - headers and declarations,
             *  sending declarations along with their matching headers on to the LineProcessor
             */

            string headerString = "";
            List<string> declareLines = new List<string>();
            bool inHeader = false;
            List<GenSettings> genSettings = new List<GenSettings>();
            genSettings.Add( new GenSettings() );

            //dictionary to store gens in
            Dictionary<string, Gen> gens = new Dictionary<string, Gen>();

            //loop through all lines slotting them into the correct list
            //once we have a header and a set of declarations, process them
            for( int i = 0; i < lines.Length; i++ ) {

                //check line is long enough
                if( lines[i].Trim().Length > 0 ) {

                    //if we see the header symbol switch in or out of header mode
                    //according to current state
                    if( PU.StringContainsHeader( lines[i] ) ) {
                        if( inHeader ) {
                            inHeader = false;
                        } else {
                            inHeader = true;

                            //if this is not the first header to be started
                            //process the current headerLines and declareLines
                            //before moving on
                            if( declareLines.Count > 0 ) {

                                //TODO - check what happens if there is NO header
                                //TODO - check what happens if there is NO header for the first gens,
                                //          and then there IS a header

                                //Create a new GensSettings from the header, using the previous genSettings as a basis
                                GenSettings gs = ParseHeader( headerString, genSettings[ genSettings.Count - 1 ] );
                                //store the new GenSettings
                                genSettings.Add( gs );

                                Dictionary<string, Gen> tempGens = LineProcessor( gs, declareLines.ToArray() );

                                foreach( KeyValuePair<string, Gen> g in tempGens ) {
                                    gens.Add( g.Key, g.Value );
                                }

                                headerString = "";
                                declareLines = new List<string>();

                            }
                        }

                        //if there is more string after the header symbol
                        //add it to headerLines
                        if( lines[i].Trim().Length > lines[i].IndexOf( PU.CharMap( CharType.header ) ) + 1 ) {
                            if( headerString.Length > 0 ) {
                                headerString += " ";
                            }
                            headerString += lines[i];
                        }
                    } else {

                        //add lines to the correct list depending on mode
                        if( inHeader ) {
                            if( headerString.Length > 0 ) {
                                headerString += " ";
                            }
                            headerString += lines[i];
                        } else {
                            declareLines.Add( lines[i] );
                        }

                    }
                }
            }

            //add the final declarations to the dictionary
            if( declareLines.Count > 0 ) {

                //Create a new GensSettings from the header, using the previous genSettings as a basis
                //we don't need to store this one as it's the last
                GenSettings gs = ParseHeader( headerString, genSettings[genSettings.Count - 1] );

                Dictionary<string, Gen> tempGens = LineProcessor( gs, declareLines.ToArray() );

                foreach( KeyValuePair<string, Gen> g in tempGens ) {
                    gens.Add( g.Key, g.Value );
                }
            }

            return gens;

        }

        private static GenSettings ParseHeader( string h, GenSettings oldSettings ) {

            /*
             *  This receives a string that may have come from a header, 
             *  or, in fact, the codes following a generator name
             *  (need to come up with a word for this),
             *  or the code that comes before a list  
             */

            //Create a new GenSettings to store all the info we find.
            //Maybe, in the future we might be overwritting an already-existing one
            //think about this

            GenSettings gs;

            if( h.Contains( "reset" ) ) {
                gs = new GenSettings();
            } else {
                gs = new GenSettings (oldSettings);
            }

            //check there's anything here to parse...
            //i think this should already have been done elsewhere,
            //but I'm sure something could have slipped through,
            //better safe than sorry
            if( h.Length > 0 ) {

                //this bit is case insensitive, so
                h = h.ToLower();

                //-------------------------------------------//
                //Section One
                //Pick Type
                //-------------------------------------------//

                PickType pt = gs.pickType;

                //first check the shorthand way
                int startIndex = h.IndexOf( '%' );
                if( startIndex >= 0 && h.Length > startIndex + 1 ) {

                    //DEBUG
                    //Console.WriteLine( "Shorthand pick type detected" );

                    //index of the character indicating the picktype
                    int nextIndex = startIndex + 1;
                    //possible characters
                    char[] possibleTypes = { 'r', 's', 'c' };

                    for( int i = 0; i < possibleTypes.Length; i++ ) {

                        if( h[nextIndex] == possibleTypes[i] ) {
                            pt = (PickType)i;

                            //DEBUG
                            //Console.WriteLine( $"Pick type is: {pt}" );

                            break;
                        }

                    }
                } else {

                    //now check the longhand way
                    startIndex = h.IndexOf( "pick " );
                    if( startIndex >= 0 ) {
                        startIndex += 4;
                        int equalsIndex = h.IndexOf( '=', startIndex );
                        string pickTypeString = h.Substring( equalsIndex + 1 );

                        string[] possibleTypes = { "random", "shuffle", "cycle" };

                        //DEBUG
                        //Console.WriteLine( $"pickTypeString =\"{pickTypeString}\"" );

                        for( int i = 0; i < possibleTypes.Length; i++ ) {

                            int typeLength = possibleTypes[i].Length;

                            if( pickTypeString.Length >= typeLength &&
                                pickTypeString.Contains( possibleTypes[i] ) ) {
                                pt = (PickType)i;

                                //DEBUG
                                //Console.WriteLine( $"pick type set to:{pt}" );

                                break;
                            }
                        }
                    }
                }
                gs.pickType = pt;
            }

            return gs;
        }

        private static Dictionary<string, Gen> LineProcessor( GenSettings gs, string[] lines ) {
            /*
             *  receives the comment-stripped lines from the text file
             *  and process them into generator declarations,
             *  deals with multi-line declarations
             *  
             *  also receives the header applying to those
             */

            List<string> names = new List<string>();
            List<string> declarations = new List<string>();
            List<GenSettings> settings = new List<GenSettings>(); 

            string currentDeclaration = "";

            for( int i = 0; i < lines.Length; i++ ) {

                if( lines[i].Length > 0 ) {

                    //If a new declaration is started on this line
                    if( PU.StringContainsDeclaration( lines[i] ) ) {

                        //if there's already a declaration on the go - add it to the list
                        if( currentDeclaration.Length > 0 ) {
                            declarations.Add( currentDeclaration );
                        }

                        //split the line into the name and (potentially only the start of) the declaration
                        string name;
                        string contents;
                        PU.StringToStringPair( lines[i], out name, out contents );

                        //test the name to see if it has a header of its own
                        //if it does - parse it and store the GenSettings
                        //if it doesn't - store the header-based GenSettings
                        char[] separators = { ' ' };
                        string[] nameSplit = name.Split( separators, StringSplitOptions.RemoveEmptyEntries );
                        name = nameSplit[0];

                        //settings.Add( gs );

                        if( nameSplit.Length > 1 ) {
                            GenSettings genGS = ParseHeader( nameSplit[1], gs );
                            settings.Add( genGS );
                        } else {
                            settings.Add( gs );
                        }

                        //DEBUG
                        /*Console.WriteLine( $"LP. name: '{name}', nameSplitLength: '{nameSplit.Length}', nameSplitContents:" );
                        foreach( string ns in nameSplit ) {
                            Console.WriteLine( ns );
                        }
                        Console.WriteLine( "" );*/

                                

                        //store the name and declaration
                        names.Add( name.Trim() );
                        currentDeclaration = contents.Trim();

                    } else {
                        //if a new declaration wasn't started this line,
                        //then it's a continuation of the previous one,
                        //add it to the string.
                        currentDeclaration += ' ';
                        currentDeclaration += lines[i].Trim();
                    }
                }
            }

            //get the last declaration
            if( currentDeclaration.Length > 0 ) {
                declarations.Add( currentDeclaration );
            }

            if( names.Count != declarations.Count ) {
                Console.WriteLine( $"Line Processor Error: the number of names ({names.Count}) did not match the number of gernator declarations ({declarations.Count})" );
            }

            //create a dictionary and return it
            Dictionary<string, Gen> namedDeclarations = new Dictionary<string, Gen>();
            for( int i = 0; i < names.Count; i++ ) {
                Gen g = SenGenProcessor( declarations[i], settings[i] );
                namedDeclarations.Add( names[i], g );
            }
            return namedDeclarations;
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

        private static Gen SenGenProcessor( string s, GenSettings headerSettings ) {
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

        private static ListGen ListGenProcessor( string[] sArr, GenSettings headerSettings ) {
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
                        Gen g = WrdProcessor( s );
                        gens.Add( g );
                    }
                }

                wg = new ListGen( gens.ToArray(), headerSettings );

            } else {

                //if there are no nested lists, create a simple ListGen out of the strings
                Gen[] gens = MultiWrdProcessor( sArr );
                wg = new ListGen( gens, headerSettings );

            }

            return wg;
        }

        private static string GetBracketsStart( string s, out string preBrackets ) {
            /*
             * finds the starting point of the brackets and returns the input string
             * minus anything before the start, also outputs a prefix separately
             */

            preBrackets = "";
            string contents = "";
            bool started = false;

            foreach( char c in s ) {

                if( started ) {

                    contents += c;

                } else {

                    if( c == PU.CharMap( CharType.openList ) ) {

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
                    if( c == PU.CharMap( CharType.closeList ) ) {
                        closeCount++;
                    } else if( c == PU.CharMap( CharType.openList ) ) {
                        openCount++;
                    }

                    //check to see if the brackets have been closed
                    if( openCount == closeCount ) {

                        contents.Add( tempContents );
                        complete = true;

                    } else {

                        //separate by commas, unless we are above level 1 in the nesting
                        if( c == PU.CharMap( CharType.listSeparator ) &&
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




    }
}

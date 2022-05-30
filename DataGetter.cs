using System;
using System.Collections.Generic;

using PU = NGen.ParserUtils;
using GP = NGen.GenProcessors;

namespace NGen {

    public static class DataGetter {

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
            //TODO - weighted pickTypes
            //TODO - remap special characters
            //TODO - set min, max, mean, stdDev for repeats
            //TODO - chance to output nothing
            //TODO - Set NoRep to false
            //TODO - fix NoRep shuffle function
            //TODO - check what happens if no header, or no header at beginning but yes one later
            //TODO - better optimise WrdProcessor
            //TODO - add repeat to ProxyGens


            //Check all ProxyGens to see if their genNames are in the dictionary
            //if they are, pass them their gens

            List<ProxyGen> proxyGens = GP.GetProxyGens();

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
                                GenSettings gs = ParseMainHeader( headerString, genSettings[ genSettings.Count - 1 ] );
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
                GenSettings gs = ParseMainHeader( headerString, genSettings[genSettings.Count - 1] );

                Dictionary<string, Gen> tempGens = LineProcessor( gs, declareLines.ToArray() );

                foreach( KeyValuePair<string, Gen> g in tempGens ) {
                    gens.Add( g.Key, g.Value );
                }
            }

            return gens;

        }

        private static GenSettings ParseMainHeader( string h, GenSettings oldSettings ) {
            /*
             *  deals with the long-hand parsing of headers, only used in main headers
             *  and done with a different technique to shorthand headers
             */

            h = h.ToLower();

            GenSettings gs;

            if( h.Contains( "reset" ) ) {
                gs = new GenSettings();
            } else {
                gs = new GenSettings( oldSettings );
            }


            if( h.Length > 0 ) {

                //Pick Type

                PickType pt = gs.PickType;

                StringToEnum<PickType>( h, "pick", ref pt );
                gs.PickType = pt;

                //Repeat Type

                RepeatType rt = gs.RepType;

                StringToEnum<RepeatType>( h, "repeat", ref rt );
                gs.RepType = rt;
                gs.SetRepeatDefaults();

                if( h.Contains("norep") ) {
                    gs.NoRep = true;
                }

            }

            return gs;
        }

        private static void StringToEnum<T>( string source, string type, ref T e ) where T : System.Enum {
            /*
             *  Does a little trimming on a string, 
             *  and then looks to see if it contains an enum name
             *  sets the referenced enum to that name if it exists
             */


            int startIndex = source.IndexOf( type );
            if( startIndex >= 0 ) {
                startIndex += type.Length;
                int equalsIndex = source.IndexOf( '=', startIndex );
                string searchString = source.Substring( equalsIndex + 1 );

                int enumNameIndex = DoesStringContainEnumName<T>( searchString );

                if( enumNameIndex >= 0 ) {

                    e = (T)(object)enumNameIndex;

                }

            }

        }

        private static int DoesStringContainEnumName<T>( string s ) where T : System.Enum {
            /*
             *  Checks to see if the string contains any of the names contained in the given enum
             *  If it does, returns an int which can later be converted 
             *  back into an enum, if it doesn't, returns -1
             */


            string[] names = Enum.GetNames( typeof(T) );

            for( int i = 0; i < names.Length; i++ ) {
                if( s.Contains( names[i]) ) {

                    //DEBUG
                    Console.WriteLine( $"enum name found: '{names[i]}'" );

                    return i;
                }
            }
            return -1;

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

            GenSettings gs = oldSettings;

            //this bit is case insensitive, so
            h = h.ToLower();

            //check there's anything here to parse...
            //i think this should already have been done elsewhere,
            //but I'm sure something could have slipped through,
            //better safe than sorry
            if( h.Length > 0 ) {

                //DEBUG
                Console.WriteLine( $"h is: '{h}'" );

                //-------------------------------------------//
                //Section One
                //Pick Type
                //-------------------------------------------//

                PickType pt = gs.PickType;

                //first check the shorthand way
                int startIndex = h.IndexOf( '?' );
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
                }

                gs.PickType = pt;

                int noRepIndex = h.IndexOf( '!' );

                //TODO - there is currently no way to unset NoRep at all

                if( noRepIndex >= 0 ) {
                    gs.NoRep = true;
                }

                //-------------------------------------------//
                //Section Two
                //Repeat Type
                //-------------------------------------------//

                RepeatType rt = gs.RepType;

                //first check the shorthand way
                startIndex = h.IndexOf( '&' );
                    //TODO
                    //NOTE: there is a potential problem here, I think. 
                    //if the character is the last character in the string
                    //then it won't be detected
                if( startIndex >= 0 && h.Length > startIndex + 1 ) {

                    //index of the character indicating the picktype
                    int nextIndex = startIndex + 1;

                    char rtChar = h[nextIndex];

                    //check if the character after the repeat symbol is a number
                    if( char.IsDigit( rtChar ) ) {

                        rt = RepeatType.normal;

                    } else {

                        if( rtChar == 'u') {

                            rt = RepeatType.uniform;

                        } else  if( rtChar == 'f' ) {

                            rt = RepeatType.@fixed;

                        } else if( rtChar == 'w' ) {

                            rt = RepeatType.weighted;

                        } else {

                            rt = RepeatType.normal;

                        }

                    }

                }
                gs.RepType = rt;

            }

            gs.SetRepeatDefaults();
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

                            string headerString = "";
                            for( int j = 1; j < nameSplit.Length; j++ ) {
                                headerString += nameSplit[j] + " ";
                            }

                            GenSettings genGS = ParseHeader( headerString, gs );
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
                Gen g = GP.SenGenProcessor( declarations[i], settings[i] );
                namedDeclarations.Add( names[i], g );
            }
            return namedDeclarations;
        }

    }
}

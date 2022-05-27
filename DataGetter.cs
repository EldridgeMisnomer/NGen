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
            //TODO - repeats
            //TODO - variable %
            //TODO - set allow repeat elements from lists
            //TODO - remap special characters
            //TODO - header reset defaults
            //TODO - weighted randoms


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
                Gen g = GP.SenGenProcessor( declarations[i], settings[i] );
                namedDeclarations.Add( names[i], g );
            }
            return namedDeclarations;
        }

    }
}

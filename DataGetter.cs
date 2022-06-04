using System;
using System.Collections.Generic;
using Utils;

using PU = NGen.ParserUtils;
using GP = NGen.GenProcessors;
using HP = NGen.HeaderParsers;


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

            //TODO - remap special characters
            //TODO - fix NoRep shuffle function
            //TODO - check what happens if no header, or no header at beginning but yes one later
            //TODO - better optimise WrdProcessor
            //TODO - add repeat to ProxyGens
            //TODO - add headers to ProxyGens
            //TODO - check for matching brackets
            //TODO - ??? Glitch ???
            //TODO - ??? add 1 or two default Lists - Numbers, Letters, Alphanumeric, Uppercase Letters
            //TODO - ??? some form of controlling case

            //TODO - shoud proxies have output chance switched off when set to once? -- I think  No, but worth thinking about

            List<ProxyGen> proxyGens = GP.GetProxyGens();

            if( proxyGens.Count > 0 ) {
                for( int i = 0; i < proxyGens.Count; i++ ) {
                    string name = proxyGens[i].GetName();

                    if( gens.ContainsKey( name ) ) {

                        proxyGens[i].SetGen( gens[name] );

                    } else {
                        Console.WriteLine( $"Generator Reference Error: '{name}' has not been created" );
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

                                    if( !gens.ContainsKey( g.Key ) ) {

                                        gens.Add( g.Key, g.Value );

                                    } else {
                                        Console.WriteLine( $"Duplicate Generator Name Error: Only the first generator with the name '{g.Key}' has been added." );
                                    }
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
                    if( !gens.ContainsKey( g.Key ) ) {

                        gens.Add( g.Key, g.Value );

                    } else {
                        Console.WriteLine( $"Duplicate Generator Name Error: Only the first generator with the name '{g.Key}' has been added." );
                    }
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

                EnumHelpers.StringToEnum<PickType>( h, "pick", ref pt );
                gs.PickType = pt;

                //Repeat Type

                RepeatType rt = gs.RepType;

                EnumHelpers.StringToEnum<RepeatType>( h, "repeat", ref rt );
                gs.RepType = rt;
                gs.SetRepeatDefaults();

                if( h.Contains("norep") ) {
                    gs.AllowRepeats = true;
                }

            }

            return gs;
        }

        private static GenSettings ParseHeader( string h, GenSettings oldSettings ) {

            /*
             *  This receives a string that may have come from a header, 
             *  or, in fact, the codes following a generator name
             *  (need to come up with a word for this),
             *  or the code that comes before a list  
             */

            GenSettings gs = new GenSettings(oldSettings);

            //this bit is case insensitive, so
            h = h.ToLower();

            //check there's anything here to parse...
            //i think this should already have been done elsewhere,
            //but I'm sure something could have slipped through,
            //better safe than sorry
            if( h.Length > 0 ) {

                //DEBUG
                Console.WriteLine( $"h is: '{h}'" );

                HP.HeaderShorthandSifter( h, ref gs );

                //DEBUG
/*                string sw = "";
                foreach( double dw in gs.PickWeights ) {
                    sw += dw.ToString() + ", ";
                }
                Console.WriteLine( $"ParseHeaderEnd, weights are now: {sw}" );
                Console.WriteLine( $"ParseHeaderEnd, pickType is: {gs.PickType}" );*/


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
                Console.WriteLine( $"Line Processor Error: the number of names ({names.Count}) did not match the number of generator declarations ({declarations.Count})" );
            }

            //create a dictionary and return it
            Dictionary<string, Gen> namedDeclarations = new Dictionary<string, Gen>();
            for( int i = 0; i < names.Count; i++ ) {
                Gen g = GP.SenGenProcessor( declarations[i], settings[i] );

                if( !namedDeclarations.ContainsKey( names[i] ) ) {

                    namedDeclarations.Add( names[i], g );

                } else {
                    Console.WriteLine( $"Duplicate Generator Name Error: Only the first generator with the name '{names[i]}' has been added." );
                }
            }
            return namedDeclarations;
        }

    }
}

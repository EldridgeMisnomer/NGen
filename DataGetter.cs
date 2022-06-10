using System;
using System.Collections.Generic;
using Utils;

using PU = NGen.ParserUtils;
using GP = NGen.GenProcessors;
using HP = NGen.HeaderParsers;


namespace NGen {

    public static class DataGetter {

        //TODO - remap special characters
        //TODO - fix NoRep shuffle function
        //TODO - check what happens if no header, or no header at beginning but yes one later
        //TODO - better optimise WrdProcessor
        //TODO - ??? Glitch ???
        //TODO - ??? add 1 or two default Lists - Numbers, Letters, Alphanumeric, Uppercase Letters
        //TODO - ??? some form of controlling case
        //TODO - add the possibility to manually define the start of Sentences using |
        //TODO - fill in missing longhand



        public static Dictionary<string, SenGen> SplitHeadersAndGenerators( string[] lines ) {
            /*
             *  receives the comment-stripped lines from the text file
             *  and divides them up into sections - headers and generator declarations,
             *  sending declarations along with their matching headers on to the LineProcessor
             */

            string headerString = "";
            List<string> declareLines = new List<string>();
            List<GenSettings> genSettings = new List<GenSettings>();
            genSettings.Add( new GenSettings() );
            bool inHeader = false;

            //dictionary to store gens in
            Dictionary<string, SenGen> gens = new Dictionary<string, SenGen>();

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

                                Dictionary<string, SenGen> tempGens = LineProcessor( gs, declareLines.ToArray() );

                                foreach( KeyValuePair<string, SenGen> g in tempGens ) {

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

                Dictionary<string, SenGen> tempGens = LineProcessor( gs, declareLines.ToArray() );

                foreach( KeyValuePair<string, SenGen> g in tempGens ) {
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

            //Reset or not
            if( h.Contains( "reset" ) ) {
                gs = new GenSettings();
            } else {
                gs = new GenSettings( oldSettings );
            }


            /*
             *  I need to rethink a bit how this is done.
             *  
             *  3 types of setting
             *  
             *  EnumType = EnumType.value
             *  BoolType = On / Off
             *  Number = Number / Array of Numbers
             *  
             *  We need to split it up by =
             *      Then 0 will contain a name, 
             *      1 will contain the value, & the next name, 
             *      2 the value & the next name, 
             *      etc.
             */


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

        private static Dictionary<string, OutputGen> LineProcessor( GenSettings oldSettings, string[] lines ) {
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
            List<string[]> tags = new List<string[]>();

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
                        PU.StringToStringPair( lines[i], out string name, out string contents );

                        //DEBUG
                        //Console.WriteLine( $"Getting tags from: '{name}'" );

                        //get the tags from the name
                        string[] thesetags = PU.ExtractTagsFromName( ref name );
                        tags.Add( thesetags );

                        //extract the header from the name
                        GenSettings newGS = HP.GetSettingsFromName( ref name, oldSettings );

                        //store the settings, name and declaration
                        settings.Add( newGS );
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
            Dictionary<string, OutputGen> namedDeclarations = new Dictionary<string, OutputGen>();

            for( int i = 0; i < names.Count; i++ ) {

                SenGen g = GP.SenGenProcessor( declarations[i], settings[i], tags[i] );

                if( g != null ) {

                    if( g.tags != null && g.tags.Length > 0 ) {

                        if( namedDeclarations.ContainsKey( names[i] ) ) {
                            namedDeclarations[names[i]].AddGen( g );
                        }



                    }


                    if( !namedDeclarations.ContainsKey( names[i] ) ) {

                        namedDeclarations.Add( names[i], g );

                    } else {
                        Console.WriteLine( $"Duplicate Generator Name Error: Only the first generator with the name '{names[i]}' has been added." );
                    }

                } else {

                    Console.WriteLine( $"Generator Creation Error: NGen was not able to correctly process the generator named {names[i]}, it has not been added." );

                }
            }
            return namedDeclarations;
        }


    }
}

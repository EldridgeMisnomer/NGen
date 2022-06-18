using System;
using System.Collections.Generic;
using Utils;
using System.Linq;

using PU = NGen.ParserUtils;
using GP = NGen.GenProcessors;


namespace NGen {

    public static class DataGetter {

        //TODO - fix NoRep shuffle function
        //TODO - extend AllowDupes to weighted pick types
        //TODO - check what happens if no header, or no header at beginning but yes one later
        //TODO - better optimise WrdProcessor
        //TODO - ??? add 1 or two default Lists - Numbers, Letters, Uppercase Letters
        //TODO - ??? some form of controlling case
        //TODO - add the possibility to manually define the start of Sentences using |
        //TODO - fill in missing longhand
        //TODO - add ability to set Glitch characters

        public static Dictionary<char, char> remapDict = null;


        public static Dictionary<string, OutputGen> SplitHeadersAndGenerators( string[] lines ) {
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
            Dictionary<string, OutputGen> gens = new Dictionary<string, OutputGen>();

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

                                Dictionary<string, OutputGen> tempGens = LineProcessor( gs, declareLines.ToArray() );

                                foreach( KeyValuePair<string, OutputGen> g in tempGens ) {

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

                Dictionary<string, OutputGen> tempGens = LineProcessor( gs, declareLines.ToArray() );

                foreach( KeyValuePair<string, OutputGen> g in tempGens ) {
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

            if( h.Length > 0 ) {

                //remapping
                int remapInd = h.IndexOf( "remap" );

                //DEBUG
                Console.WriteLine( $"remapInd: '{remapInd}'." );

                if( remapInd >= 0 ) {

                    int startInd = h.IndexOf( '(', remapInd + 5 );
                    int endInd = h.IndexOf( ')', startInd + 1 );

                    //DEBUG
                    Console.WriteLine( $"remap string: '{h}'." );
                    Console.WriteLine( $"remap, startInd: '{startInd}', endInd: '{endInd}'." );

                    string remapString = h.Substring( startInd + 1, endInd - startInd - 1 );
                    Console.WriteLine( $"remap string: '{remapString}'." );

                    //split string based on '='
                    string[] remapSplit = ParserUtils.StringSplitOnUnEscapedCharacter( remapString, '=' );

                    Console.WriteLine( $" remapArray is {remapSplit.Length} elements long." );


                    remapDict = new Dictionary<char, char>();

                    for( int i = 0; i < remapSplit.Length-1; i++ ) {

                        //DEBUG
                        Console.WriteLine( $" remapLoop: {i} " );

                        string f1 = remapSplit[i].Trim();
                        string f2 = remapSplit[i+1].Trim();
                        char c1 = f1[f1.Length-1];
                        char c2 = f2[0];
                        remapDict.Add( c1, c2 );
                    }

                    ParserUtils.RemapChars( remapDict );

                    //remake h without remap, to save time on later searches
                    string preRemap = "";
                    string postRemap = "";

                    if( remapInd > 0 ) {
                        //"0123remap)012"
                        preRemap = h.Substring( 0, remapInd );
                    }
                    if( h.Length > endInd + 1 ) {
                        postRemap = h.Substring(endInd + 1 );
                    }

                    h = preRemap + ' ' + postRemap;

                }



                //Split header
                char[] separators = { '=' };
                List<string> hParts = new List<string>( h.Split( separators, StringSplitOptions.RemoveEmptyEntries ) );

                //Search strings
                List<string> searchStrings = new List<string> {
                    "pick", "repeat", "output", "separator", "once", "allow", "glitch"
                };

                //keep track of pick or repeat last picked, for weights
                bool pickLast = false;

                //work through the parts
                for( int i = 0; i < hParts.Count-1; i++ ) {

                    string p1 = hParts[i];
                    string p2 = hParts[i+1];

                    //search p1 for searchStrings
                    for( int j = 0; j < searchStrings.Count; j++ ) {

                        string ss = searchStrings[j];
                        if( p1.Contains( ss ) ) {

                            //if the string is found do something depending on which one it is
                            switch( ss ) {
                                case "pick":

                                    pickLast = true;

                                    PickType pt = gs.PickType;
                                    EnumHelpers.StringToEnum<PickType>( p2, ref pt );

                                    switch( pt ) {
                                        case PickType.shuffle:
                                            searchStrings.Add( "point" );
                                            break;
                                        case PickType.cycle:
                                            searchStrings.Add( "skip" );
                                            break;
                                        case PickType.weighted:
                                            searchStrings.Add( "weights" );
                                            break;
                                    }

                                    gs.PickType = pt;
                                    gs.SetPickDefaults();
                                    break;

                                case "repeat":

                                    pickLast = false;

                                    RepeatType rt = gs.RepType;
                                    EnumHelpers.StringToEnum<RepeatType>( p2, ref rt );

                                    switch( rt ) {
                                        case RepeatType.uniform:
                                            searchStrings.Add( "min" );
                                            searchStrings.Add( "max" );
                                            break;
                                        case RepeatType.normal:
                                            searchStrings.Add( "min" );
                                            searchStrings.Add( "max" );
                                            searchStrings.Add( "mean" );
                                            searchStrings.Add( "dev" );
                                            break;
                                        case RepeatType.@fixed:
                                            searchStrings.Add( "num" );
                                            break;
                                        case RepeatType.weighted:
                                            searchStrings.Add( "weights" );
                                            searchStrings.Add( "factor" );
                                            break;
                                    }

                                    gs.RepType = rt;
                                    gs.SetRepeatDefaults();
                                    break;

                                case "output":

                                    HeaderSetDouble( p2, out gs.OutputChance );
                                    break;

                                case "separator":

                                    //check if this is a proxy
                                    int proxyInd = p2.GetNonEscapedCharIndex( ParserUtils.CharMap( CharType.proxy ) );
                                    if( proxyInd >= 0 ) {

                                        string proxyName = p2.Substring( proxyInd + 1 ).Trim();

                                        //check if there's any spaces after the proxy name and chop accordingly
                                        int spaceInd = p2.IndexOf( ' ', proxyInd );
                                        if( spaceInd >= 0 ) {
                                            proxyName = proxyName.Substring( 0, spaceInd );
                                        }

                                        //TODO - check if using gs here is really correct???
                                        ProxyGen pg = GenProcessors.ProxyProcessor( proxyName, gs );
                                        gs.ProxySeparator = pg;
                                        gs.UseProxySeparator = true;

                                    } else {

                                        if( p2.Trim().Length == 0 ) {
                                            gs.Separator = " ";
                                        } else {
                                            gs.Separator = p2.Trim();
                                        }
                                        gs.UseProxySeparator = false;

                                    }

                                    break;

                                case "once":

                                    HeaderSwitch( p2, out gs.Once );
                                    break;

                                case "allow":

                                    HeaderSwitch( p2, out gs.AllowDupes );
                                    break;

                                case "glitch":

                                    HeaderSwitch( p2, out gs.Glitch );
                                    searchStrings.Add( "chance" );
                                    searchStrings.Add( "perma" );
                                    searchStrings.Add( "clean" );

                                    break;

                                case "point":

                                    HeaderSetDouble( p2, out gs.ShufflePoint );
                                    break;

                                case "skip":

                                    HeaderSetInt( p2, out gs.Skip );
                                    break;

                                case "weights":

                                    if( p2.Contains( '-' ) ) {

                                        string[] sNums = p2.Split( '-' );

                                        //TODO - need to work out why repeat weights are ints
                                        //      and pick weights are doubles

                                        if( pickLast ) {

                                            double[] dNums = new double[sNums.Length];
                                            for( int k = 0; k < sNums.Length; k++ ) {

                                                double n;
                                                if( ParserUtils.DirtyStringToDouble( sNums[k], out n ) ) {
                                                    dNums[k] = n;
                                                } else {
                                                    //TODO - report an error here, decide what number should be added
                                                    dNums[k] = 0;
                                                }
                                            }

                                            if( dNums.Length == 2 ) {

                                                gs.PickWeightStart = dNums[0];
                                                gs.PickWeightEnd = dNums[1];
                                                gs.PickWeightsFromEnds = true;
                                                gs.PickWeightsFromFac = false;

                                            } else {

                                                gs.PickWeights = dNums;
                                                gs.PickWeightsFromEnds = false;
                                                gs.PickWeightsFromFac = false;

                                            }

                                        } else {

                                            int[] iNums = new int[sNums.Length];
                                            for( int k = 0; k < sNums.Length; k++ ) {

                                                int ni;
                                                if( ParserUtils.DirtyStringToInt( sNums[k], out ni ) ) {
                                                    iNums[k] = ni;
                                                } else {
                                                    //TODO - report an error here, decide what number should be added
                                                    iNums[k] = 1;
                                                }

                                            }

                                        }
                                    } else {
                                        //TODO - report an error here
                                        Console.WriteLine( $"Main Header Parser Error: weights could not be determined from '{p2}'." );
                                    }

                                    break;

                                case "factor":

                                    HeaderSetDouble( p2, out gs.WeightFac );
                                    gs.PickWeightsFromFac = true;
                                    gs.PickWeightsFromEnds = false;
                                    break;

                                case "min":

                                    HeaderSetInt( p2, out gs.RepMin );
                                    break;

                                case "max":

                                    HeaderSetInt( p2, out gs.RepMax );
                                    break;

                                case "mean":

                                    HeaderSetInt( p2, out gs.RepMean );
                                    break;

                                case "dev":

                                    HeaderSetDouble( p2, out gs.RepStdDev );
                                    break;

                                case "num":

                                    HeaderSetInt( p2, out gs.RepMax );
                                    break;

                                case "chance":

                                    HeaderSetDouble( p2, out gs.GlitchChance );
                                    break;

                                case "perma":

                                    HeaderSwitch( p2, out gs.PermaGlitch );
                                    break;

                                case "clean":

                                    HeaderSwitch( p2, out gs.CleanFirst );
                                    break;

                                default:
                                    //TODO report an error here
                                    Console.WriteLine( $"Main Header Parser Error: settings pair not recognised '{p1}' = '{p2}'." );
                                    break;

                            }
                        }
                    }
                }
            }

            return gs;
        }

        private static void HeaderSwitch( string s, out bool b ) {
            if( s.ToLower().Contains( "on" ) ) {
                b = true;
            } else {
                b = false;
            }
        }

        private static void HeaderSetDouble( string s, out double d ) {
            if( !ParserUtils.DirtyStringToDouble( s, out d ) ) {
                //TODO - report an error here
            }
        }

        private static void HeaderSetInt( string s, out int i ) {
            if( !ParserUtils.DirtyStringToInt( s, out i ) ) {
                //TODO - report an error here
            }
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
                        currentDeclaration = contents.Trim();

                        //get the tags from the name
                        string[] thesetags = TagWrangler.GetTagsFromName( ref name );
                        tags.Add( thesetags );

                        //DEBUG
                        string ts = "";
                        foreach( string t in thesetags ) {
                            ts += t + ", ";
                        }
                        //Console.WriteLine( $"LineProcessor - received tags are: '({ts})'." );

                        //extract the header from the name
                        GenSettings newGS = HeaderWrangler.GetSettingsFromName( ref name, oldSettings );

                        //Check if is MainGen
                        if( name[0] == '@' ) {
                            name = name.Substring( 1 );
                            newGS.isMain = true;
                        }

                        //store the header
                        settings.Add( newGS );

                        //store the name, which has been modified by the previous two methods
                        names.Add( name.Trim() );

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

            Dictionary<string, OutputGen> genDict = CreateGenDictionary( names, declarations, settings, tags );
            
            return genDict;

        }

        private static Dictionary<string, OutputGen> CreateGenDictionary( List<string> names, List<string> declarations, List<GenSettings> settings, List<string[]> tags ) {

            Dictionary<string, SenGen> senGens = new Dictionary<string, SenGen>();
            Dictionary<string, TagGen> tagGens = new Dictionary<string, TagGen>();

            for( int i = 0; i < names.Count; i++ ) {

                //Create the SenGen
                SenGen g = GP.SenGenProcessor( declarations[i], settings[i], tags[i] );

                //If that worked, put it somewhere
                if( g != null ) {

                    //If there's already a sengen with that name
                    if( senGens.ContainsKey( names[i] ) ) {

                        //DEBUG
                        //Console.WriteLine( $"There is already a gen named '{names[i]}', Creating a TagGen and putting stuff in it" );
                        //Console.WriteLine( $"Old SenGen has first tag: ({senGens[names[i]].ownTags[0]})");
                        //Console.WriteLine( $"New SenGen has first tag: ({g.ownTags[0]})");

                        //create a TagGen, put old and new Gens in it,
                        //add it to the tagGen dictionary and remove the old Gen from the senGen Dictionary
                        TagGen tg = new TagGen();
                        tg.AddGen( senGens[names[i]] );
                        tg.AddGen( g );
                        tagGens.Add( names[i], tg );
                        senGens.Remove( names[i] );

                    } else {

                        //If there's already a TagGen with this name, add the new SenGen to it
                        if( tagGens.ContainsKey( names[i] ) ) {

                            tagGens[names[i]].AddGen( g );

                        } else {

                            //otherwise, add it to the SenGen dictionary
                            senGens.Add( names[i], g );

                        }

                    }
                } else {

                    Console.WriteLine( $"Generator Creation Error: NGen was not able to correctly process the generator named {names[i]}, it has not been added." );

                }
            }

            //Now, combine the two dictionaries and return it
            Dictionary<string, OutputGen> genDict = new Dictionary<string, OutputGen>();

            if( senGens.Count > 0 ) {

                foreach( KeyValuePair<string, SenGen> sg in senGens ) {

                    genDict.Add( sg.Key, sg.Value );

                }
            }

            if( tagGens.Count > 0 ) {

                foreach( KeyValuePair<string, TagGen> tg in tagGens ) {

                    genDict.Add( tg.Key, tg.Value );

                }
            }

            return genDict;
        }
    }
}

using System;
using System.Collections.Generic;
using Utils;
using System.Linq;

namespace NGen {

    public static class MainHeaderWrangler {

        public static Dictionary<char, char> remapDict = null;

        public static GenSettings ParseMainHeader( string h, GenSettings oldSettings ) {
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

                HeaderRemapping( ref h );

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
                for( int i = 0; i < hParts.Count - 1; i++ ) {

                    string p1 = hParts[i];
                    string p2 = hParts[i + 1];

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
                                    gs.OutputChance /= 100;
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
                                    searchStrings.Add( "temp" );
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
                                            gs.RepWeights = iNums;

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
                                    gs.RepUseMean = false;
                                    gs.RepUseDev = false;
                                    break;

                                case "max":

                                    HeaderSetInt( p2, out gs.RepMax );
                                    gs.RepUseMean = false;
                                    gs.RepUseDev = false;
                                    break;

                                case "mean":

                                    HeaderSetInt( p2, out gs.RepMean );
                                    gs.RepUseMean = true;
                                    gs.RepUseDev = false;
                                    break;

                                case "dev":

                                    HeaderSetDouble( p2, out gs.RepStdDev );
                                    gs.RepUseDev = true;
                                    break;

                                case "num":

                                    HeaderSetInt( p2, out gs.RepMax );
                                    break;
                                case "temp":
                                    HeaderSwitch( p2, out gs.TempOnce );
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

        private static void HeaderRemapping( ref string h ) {

            //remapping
            int remapInd = h.IndexOf( "remap" );
            if( remapInd >= 0 ) {

                int startInd = h.IndexOf( '(', remapInd + 5 );
                int endInd = h.IndexOf( ')', startInd + 1 );

                //DEBUG
                //Console.WriteLine( $"remap string: '{h}'." );
                //Console.WriteLine( $"remap, startInd: '{startInd}', endInd: '{endInd}'." );

                string remapString = h.Substring( startInd + 1, endInd - startInd - 1 );
                //Console.WriteLine( $"remap string: '{remapString}'." );

                //split string based on '='
                string[] remapSplit = ParserUtils.StringSplitOnUnEscapedCharacter( remapString, '=' );

                //Console.WriteLine( $" remapArray is {remapSplit.Length} elements long." );


                remapDict = new Dictionary<char, char>();

                for( int i = 0; i < remapSplit.Length - 1; i++ ) {

                    //DEBUG
                    //Console.WriteLine( $" remapLoop: {i} " );

                    string f1 = remapSplit[i].Trim();
                    string f2 = remapSplit[i + 1].Trim();
                    char c1 = f1[f1.Length - 1];
                    char c2 = f2[0];
                    remapDict.Add( c1, c2 );
                }

                ParserUtils.RemapChars( remapDict );

                //remake h without remap, to save time on later searches
                string preRemap = "";
                string postRemap = "";

                if( remapInd > 0 ) {
                    preRemap = h.Substring( 0, remapInd );
                }
                if( h.Length > endInd + 1 ) {
                    postRemap = h.Substring( endInd + 1 );
                }

                h = preRemap + ' ' + postRemap;

            }
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
    }
}

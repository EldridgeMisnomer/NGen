using System;
using System.Collections.Generic;
using System.Linq;

using PU = NGen.ParserUtils;
using HP = NGen.HeaderParsers;

namespace NGen {

    public static class HeaderParsers {

        public static GenSettings GetSettingsFromName( ref string name, GenSettings oldSettings ) {
            /*
             *  Receives a Generator name, everything before the '=', 
             *  and extracts the settings from it, if there are any
             */


            char[] separators = { ' ' };
            string[] nameSplit = name.Split( separators, StringSplitOptions.RemoveEmptyEntries );
            name = nameSplit[0];

            if( nameSplit.Length > 1 ) {

                //recombine split strings into a header string
                string headerString = "";
                for( int j = 1; j < nameSplit.Length; j++ ) {

                    headerString += nameSplit[j] + " ";

                }

                GenSettings newGS = HP.ParseHeader( headerString, oldSettings );
                return newGS;

            } else {

                return new GenSettings( oldSettings );

            }
        }

        public static GenSettings ParseHeader( string h, GenSettings oldSettings ) {

            /*
             *  This receives a string that may have come from a component header
             *  and process it into settings
             */

            GenSettings gs = new GenSettings( oldSettings );

            //this bit is case insensitive, so
            h = h.ToLower();

            if( h.Length > 0 ) {

                HP.HeaderShorthandSifter( h, ref gs );

            }

            return gs;
        }

        public static void HeaderShorthandSifter( string s, ref GenSettings gs ) {

            /*
             *  Characters listed are, in order:
             *      Repeat, Output Chance, AllowRepeats, Pick, Separator, Once
             */

            List<char> charList = new List<char> { '&', '%', '~', '?', '_', '*' };

            char lastChar = 'x';
            string lastString = "";

            for( int i = 0; i < s.Length; i++ ) {

                if( charList.Contains( s[i] ) ) {

                    if( lastChar != 'x' ) {

                        HeaderShorthandParser( lastChar, lastString, ref gs );

                    }

                    lastChar = s[i];
                    charList.Remove( s[i] );
                    lastString = "";

                } else {

                    if( lastChar != 'x' ) {

                        lastString += s[i];

                        if( i == s.Length - 1 ) {
                            HeaderShorthandParser( lastChar, lastString, ref gs );
                        }

                    }
                }
            }
        }

        private static void HeaderShorthandParser( char c, string s, ref GenSettings gs ) {

            s = s.Trim();

            //DEBUG
            //Console.WriteLine( $"HeaderParser: char: '{c}', string: '{s}'" );

            switch( c ) {

                case '&':

                    HeaderRepeatParser( s, ref gs );
                    break;

                case '%':

                    HeaderOutputChanceParser( s, ref gs );
                    break;

                case '~':

                    HeaderNoRepParser( s, ref gs );
                    break;

                case '?':
                    HeaderPickTypeParser( s, ref gs );
                    break;

                case '_':
                    HeaderSeparatorParser( s, ref gs );
                    break;

                case '*':
                    HeaderOnceParser( s, ref gs );
                    break;

            }
        }



        private static void HeaderPickTypeParser( string s, ref GenSettings gs ) {
            /*
             *  Sets a GenSetting Pick settings from a header string
             */

            if( s.Length > 0 ) {

                char pickChar = s[0];

                switch( pickChar ) {

                    case 'r':
                        gs.PickType = PickType.random;
                        break;
                    case 's':
                        gs.PickType = PickType.shuffle;
                        break;
                    case 'c':
                        gs.PickType = PickType.cycle;
                        break;
                    case 'w':
                        gs.PickType = PickType.weighted;
                        break;
                    default:
                        //TODO document
                        Console.WriteLine( $"Pick Type Error: '{s}' does not define a recognised Pick Type ( (r)andom, (s)huffle, (c)cycle, or (w)eighted )." );
                        break;
                }

                //DEBUG
                //Console.WriteLine( $"Pick Type Selected:{gs.PickType}" );

                if( gs.PickType != PickType.random ) {

                    if( s.Length > 1 ) {

                        string sNum = s.Substring( 1, s.Length - 1 ).Trim();

                        //This should split by '-' but not when the '-' is escaped '\-'
                        string[] sNumParts = ParserUtils.StringSplitOnUnEscapedCharacter( sNum, '-' );

                        if( gs.PickType == PickType.weighted ) {

                            //if there's only one number, set weights with factor
                            if( sNumParts.Length == 1 ) {

                                double n = PU.StringToDouble( sNumParts[0] );

                                if( n < 0 ) {
                                    n = 0.8;
                                }

                                gs.WeightFac = n;
                                gs.WeightsFromFac = true;
                                gs.WeightsFromEnds = false;

                            } else if( sNumParts.Length == 2 ) {

                                //if there are two numbers, set weights with ends
                                double start = PU.StringToDouble( sNumParts[0] );
                                double end = PU.StringToDouble( sNumParts[1] );

                                if( start > 0 && end > 0 ) {

                                    gs.WeightStart = start;
                                    gs.WeightEnd = end;

                                } else {

                                    gs.WeightStart = 10;
                                    gs.WeightEnd = 1;

                                }

                                gs.WeightsFromEnds = true;
                                gs.WeightsFromFac = false;

                            } else {

                                //set weights directly
                                double[] weights = new double[sNumParts.Length];
                                for( int i = 0; i < weights.Length; i++ ) {

                                    double n = PU.StringToDouble( sNumParts[i] );
                                    if( n < 0 ) n = 1;
                                    weights[i] = n;
                                }

                                gs.PickWeights = weights;
                                gs.WeightsFromEnds = false;
                                gs.WeightsFromFac = false;

                            }

                        //if type is shuffle or cycle, we only need the first number
                        } else if( gs.PickType == PickType.shuffle ) {

                            double n = PU.StringToDouble( sNumParts[0] );

                            if( n > 0 && n < 1 ) {

                                gs.ShufflePoint = n;

                            } else {

                                gs.ShufflePoint = 1;

                            }

                        } else if( gs.PickType == PickType.cycle ) {

                            int n = PU.StringToInt( sNumParts[0] );

                            if( n > 0 ) {

                                gs.Skip = n;

                            } else {

                                gs.Skip = 0;

                            }
                        }

                    } else {

                        //if there are no settings in s
                        //just set the defaults
                        gs.SetPickDefaults();

                    }
                    
                } else {

                    //if pick type is random
                    //just set the defaults
                    gs.SetPickDefaults();

                }
            } else {

                gs.PickType = PickType.random;
                gs.SetPickDefaults();

            }
        }

        private static void HeaderSeparatorParser( string s, ref GenSettings gs ) {
            //Set Separator in GenSettings from Header Text

            //Check if this is setting a default or not
            if( s.Length > 0 ) {

                //If this is a ProxyGen Separator
                if( s[0] == ParserUtils.CharMap( CharType.reference ) ) {

                    string proxyName = s.Substring( 1, s.Length - 1 );
                    ProxyGen pg = GenProcessors.ProxyProcessor( proxyName, gs );
                    gs.ProxySeparator = pg;
                    gs.UseProxySeparator = true;

                } else {

                    //otherwise, just use the string
                    gs.Separator = s.Trim();
                    gs.UseProxySeparator = false;

                }

            } else {

                //Set default separator
                gs.Separator = " ";
                gs.UseProxySeparator = false;

            }
        }

        private static void HeaderRepeatParser( string s, ref GenSettings gs ) {


            if( s.Length > 0 ) {

                char rType = s[0];

                switch( rType ) {
                    case 'f':
                        gs.SetRepType( RepeatType.@fixed );
                        break;
                    case 'u':
                        gs.SetRepType( RepeatType.uniform );
                        break;
                    case 'n':
                        gs.SetRepType( RepeatType.normal );
                        break;
                    case 'w':
                        gs.SetRepType( RepeatType.weighted );
                        break;
                    default:
                        //TODO document
                        Console.WriteLine( $"Repeat Type Error: '{s}' does not define a recognised Repeat Type ( (f)ixed, (u)niform, (n)ormal, or (w)eighted )." );
                        break;
                }

                if( s.Length > 1 ) {

                    string sNum = s.Substring( 1, s.Length - 1 ).Trim();

                    int firstDash = sNum.IndexOf( '-' );
                    if( firstDash >= 0 ) {

                        //This should split by '-' but not when the '-' is escaped '\-'
                        string[] sNumParts = ParserUtils.StringSplitOnUnEscapedCharacter( sNum, '-' );

                        //Try and convert the num string into integers
                        int[] numParts = new int[sNumParts.Length];

                        for( int i = 0; i < sNumParts.Length; i++ ) {

                            int num = PU.StringToInt( sNumParts[i] );
                            if( num == -1 ) num = 0;
                            numParts[i] = num;
                        }

                        //if the repeat type is weighted, set the weights
                        if( gs.RepType == RepeatType.weighted ) {

                            gs.RepWeights = numParts;

                        } else {

                            //two numbers means set the Minimum and the Maximum
                            if( sNumParts.Length == 2 ) {

                                gs.RepMin = numParts[0];
                                gs.RepMax = numParts[1];

                            }

                            //for uniform repeat type only
                            if( gs.RepType == RepeatType.uniform ) {

                                //three numbers means set min, mean, and max
                                if( sNumParts.Length == 3 ) {

                                    gs.RepMin = numParts[0];
                                    gs.RepMean = numParts[1];
                                    gs.RepMax = numParts[2];
                                    gs.UseMean = true;
                                    gs.UseDev = false;


                                } else {
                                    //more than three numbers mean set min, mean, stdDev, and max
                                    gs.RepMin = numParts[0];
                                    gs.RepMean = numParts[1];
                                    gs.RepStdDev = numParts[2];
                                    gs.RepMax = numParts[3];
                                    gs.UseMean = true;
                                    gs.UseDev = true;

                                }
                            }
                        }
                        
                    } else {
                        // s only has one number in it

                        int num = PU.StringToInt( sNum );
                        if( num == -1 ) num = 0;

                        gs.RepMax = num;

                    }

                } else {

                    gs.SetRepeatDefaults();

                }

            } else {
                gs.SetRepType( RepeatType.@fixed );
                gs.SetRepeatDefaults();
            }

        }


        private static void HeaderNoRepParser( string s, ref GenSettings gs ) {

            if( s.Length > 0 && s.Contains( '!' ) ) {

                gs.AllowRepeats = false;

            } else {

                gs.AllowRepeats = true;

            }
        }        
        
        private static void HeaderOnceParser( string s, ref GenSettings gs ) {

            if( s.Length > 0 && s.Contains( '!' ) ) {

                gs.Once = false;

            } else {

                gs.Once = true;

            }
        }

        private static void HeaderOutputChanceParser( string s, ref GenSettings gs ) {

            if( s.Length > 0 ) {

                double d;

                if( double.TryParse( s, out d ) ) {

                    if( d > 100 ) {
                        d = 100;
                    }

                    if( d < 0 ) {
                        d = 0;
                    }

                    gs.OutputChance = d/100;

                } else {
                    //TODO document
                    Console.WriteLine( $"Output Chance Error: '{s}' not recognised as a number between 0.0 and 1.0" );
                }
            } else {

                gs.OutputChance = 1;

            }
        }
    }
}

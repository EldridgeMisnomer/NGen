using System;
using System.Collections.Generic;
using System.Linq;

using PU = NGen.ParserUtils;

namespace NGen {

    public static class GenProcessors {

        private static List<ProxyGen> proxyGens = new List<ProxyGen>();

        public static List<ProxyGen> GetProxyGens() {
            return proxyGens;
        }


        public static Gen WrdProcessor( string s, GenSettings headerSettings ) {
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

            if( PU.StringContainsNewSentence( s ) ) {
                SenGen sg = SenGenPreProcessor( s, headerSettings, null );
                return sg;
            } else {

                if( PU.StringContainsProxy( s ) ) {
                    //split the string based on spaces
                    char[] separators = { ' ' };
                    string[] words = s.Split( separators, StringSplitOptions.RemoveEmptyEntries );

                    List<Gen> gens = new List<Gen>();

                    foreach( string w in words ) {

                        if( PU.StringContainsProxy( w ) ) {

                            ProxyGen pg = ProxyProcessor( w, headerSettings );

                            gens.Add( pg );

                        } else {
                            Wrd wrd = WrdSeparatorProcessor( w, headerSettings );
                            gens.Add( wrd );
                        }
                    }

                    SenGen sg = new SenGen( gens.ToArray(), headerSettings );
                    return sg;

                } else {
                    return WrdSeparatorProcessor( s, headerSettings );
                }
            }
        }

        public static Wrd WrdSeparatorProcessor( string w, GenSettings genSettings ) {


            w = w.Trim();
            GenSettings gs = new GenSettings( genSettings );

            if( w[0] == PU.CharMap( CharType.noSepBefore ) ) {
                //DEBUG
                //Console.WriteLine( $"Wrd Separator Processor, wrd is: {w}" );
                gs.NoSepBefore = true;
                w = w.Substring( 1 );
            }

            if( w[w.Length-1] == PU.CharMap( CharType.noSepAfter ) ) {
                //DEBUG
                //Console.WriteLine( $"Wrd Separator Processor, wrd is: {w}" );
                gs.NoSepAfter = true;
                w = w.Substring( 0, w.Length - 1 );
            }


            //DEBUG
            //Console.WriteLine( $"wrd: '{w}', gs.noSepB: {gs.NoSepBefore}, gs.noSepA: {gs.NoSepAfter}" );

            return new Wrd( w, gs );

        }

        public static ProxyGen ProxyProcessor( string s, GenSettings genSettings ) {

            int refIndex = s.IndexOf( PU.CharMap( CharType.proxy ) );

            string name = s.Substring( refIndex + 1, s.Length - refIndex - 1 );

            GenSettings gs = new GenSettings( genSettings );

            if( refIndex > 0 ) {
                string proxyHeaderString = s.Substring( 0, refIndex );
                HeaderWrangler.HeaderShorthandSifter( proxyHeaderString, ref gs );
                //DEBUG
                //Console.WriteLine( $"ProxyHeader is: '{proxyHeaderString}'" );
            }

            //DEBUG
            //Console.WriteLine( $"ProxyProcessor, output chance is: {gs.OutputChance}" );

            ProxyGen pg = new ProxyGen( name.Trim(), gs );
            //add to the ProxyGen list for connecting up later

            proxyGens.Add( pg );
            return pg;

        }

        public static Gen[] MultiWrdProcessor( string[] s, GenSettings genSettings ) {
            /*
             *  Processes an array of potential Wrds
             *  Checking for ProxyGens
             */

            Gen[] gens = new Gen[s.Length];

            for( int i = 0; i < s.Length; i++ ) {
                gens[i] = WrdProcessor( s[i], genSettings );
            }

            return gens;
        }


        public static ListGen ListGenProcessor( string[] sArr, GenSettings headerSettings ) {
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

                        Gen g = SenGenPreProcessor( s, headerSettings );
                        gens.Add( g );

                    } else {
                        Gen g = WrdProcessor( s, headerSettings );
                        gens.Add( g );
                    }
                }

                wg = new ListGen( gens.ToArray(), headerSettings );

            } else {

                //if there are no nested lists, create a simple ListGen out of the strings
                Gen[] gens = MultiWrdProcessor( sArr, headerSettings );
                wg = new ListGen( gens, headerSettings );

            }

            return wg;
        }

        public static SenGen SenGenProcessor( string s, GenSettings headerSettings, string[] tags = null ) {
            /*
             * This will take a string and turn it into a SenGen,
             */

            //first check to see if all lists are closed
            //TODO document
            int openBracketsCount = PU.GetCharacterCount( s, PU.CharMap( CharType.openList ) );
            int closeBracketsCount = PU.GetCharacterCount( s, PU.CharMap( CharType.closeList ) );

            if( openBracketsCount != closeBracketsCount ) {

                string endS = "closed with ']'";
                if( openBracketsCount < closeBracketsCount ) {
                    endS = "opened with '[";
                }

                Console.WriteLine( $"List Error: not all Lists were {endS}." );
                return null;

            }

            //Create a list to contain gens, this will eventually be turned into the SenGen
            List<Gen> gens = new List<Gen>();

            //Check to see if we have any ListGens
            if( PU.StringContinsList( s ) ) {

                //separate text before and after the start of the brackets
                //also get the header string, if there is one
                string preBracketStart;
                string bracketsHeaderString;
                string postBracketStart = PU.GetBracketsStart( s, out preBracketStart, out bracketsHeaderString );

                //DEBUG
                //Console.WriteLine( $"preBracket text is: '{preBracketStart}'" );
                //Console.WriteLine( $"bracketsHeader text is: '{bracketsHeaderString}'" );

                //If there is any text in the preBrackets string
                //we can treat it as just a Wrd, because we know it can't contain a list
                if( preBracketStart.Trim().Length > 0 ) {
                    Gen g = WrdProcessor( preBracketStart.Trim(), headerSettings );
                    gens.Add( g );
                }

                //separate text within the brackets from text after them
                string postBrackets;
                string[] bracketsContents = PU.GetBracketsContents( postBracketStart, out postBrackets );

                //if we had a header string, process it
                GenSettings newGS = new GenSettings( headerSettings );

                if( bracketsHeaderString.Length > 0 ) {

                    HeaderWrangler.HeaderShorthandSifter( bracketsHeaderString, ref newGS );
                
                }

                ListGen wg = ListGenProcessor( bracketsContents, newGS );
                gens.Add( wg );

                //process postBrackets text
                //we don't know if there are any lists in here, so we need to check
                //If there is a list in here, we send it back into the SenGenProcessor
                if( postBrackets.Trim().Length > 0 ) {

                    if( PU.StringContinsList( postBrackets ) ) {

                        Gen pbg = SenGenPreProcessor( postBrackets, headerSettings );
                        gens.Add( pbg );

                    } else {

                        //if there's no list there, treat it as a Wrd
                        //and add it to the list
                        Gen g = WrdProcessor( postBrackets.Trim(), headerSettings );
                        gens.Add( g );
                    }
                }

            } else {

                Gen g = WrdProcessor( s.Trim(), headerSettings );
                gens.Add( g );

            }

            SenGen sg = new SenGen( gens.ToArray(), headerSettings );
            
            //add any tags we've inherited
            if( tags != null && tags.Length > 0 ) {
                sg.ownTags = tags;
            }
            return sg;
        }

        public static SenGen SenGenPreProcessor( string s, GenSettings headerSettings, string[] tags = null ) {
            /*
             *  This feels pretty inelegant, but I need a way to split the Sentence into Sentences, so here goes
             */

            //We only need to do the work if there are any new Sentence characters
            //Technically we only need to do the work if they're on the bottom level
            //But I'm not sure how to test for that without doing the work
            if(  PU.StringContainsNewSentence( s ) ) {

                int openCount = 0;
                int closeCount = 0;

                List<string> headerStrings = new List<string>();
                List<string> sentenceStrings = new List<string>();
                string tempContents = "";

                //add one empty header - I'm not sure about this
                headerStrings.Add( "" ) ;

                char lastC = ' ';

                foreach( char c in s ) {

                    //increment the counters
                    if( c == PU.CharMap( CharType.closeList ) ) {
                        closeCount++;
                    } else if( c == PU.CharMap( CharType.openList ) ) {
                        openCount++;
                    }

                    //check we're on bottom level
                    if( openCount - closeCount == 0 ) {

                        //if we spot a new sentence
                        //TODO - check in other places where we do a similar check - I think we're not checking for escapes.
                        if( PU.CharIsUnescapedChar( c, lastC, PU.CharMap( CharType.newSentence ) ) ) {

                            //get the header (and trim it out of the tempContents string)
                            string headerString = PU.ExtractBracketsHeaderString( ref tempContents );

                            //add header and previous sentence to lists for later processing
                            headerStrings.Add( headerString );
                            sentenceStrings.Add( tempContents );

                            tempContents = "";

                        } else {
                            //if we're at the bottom level,
                            //add all characters that are not sentence starts
                            tempContents += c;
                        }
                    } else {
                        //if we're above the bottom level,
                        //add all characters
                        tempContents += c;
                    }
                    lastC = c;
                }

                //add the final sentence
                sentenceStrings.Add( tempContents );

                if( headerStrings.Count != sentenceStrings.Count ) {

                    //TODO error here?
                    Console.WriteLine( $"Pre SenGen Parser Error: There were {headerStrings.Count} headerStrings and {sentenceStrings.Count} sentenceStrings, there should be an equal number of each." );
                    return null;

                } else {

                    if( sentenceStrings.Count == 1 ) {

                        GenSettings gs = new GenSettings( headerSettings );

                        if( headerStrings[0].Trim().Length > 0 ) {

                            HeaderWrangler.HeaderShorthandSifter( headerStrings[0], ref gs );

                        }

                        SenGen sg = SenGenProcessor( sentenceStrings[0], gs, tags );
                        return sg;

                    } else {
                        List<SenGen> gens = new List<SenGen>();

                        for( int i = 0; i < headerStrings.Count; i++ ) {

                            if( sentenceStrings[i].Trim().Length > 0 ) {

                                GenSettings gs = new GenSettings( headerSettings );

                                if( headerStrings[i].Trim().Length > 0 ) {

                                    HeaderWrangler.HeaderShorthandSifter( headerStrings[i], ref gs );

                                }
                                gens.Add( SenGenProcessor( sentenceStrings[i], gs, tags ) );
                            }
                        }

                        if( gens.Count > 0 ) {

                            //DEBUG
                            Console.WriteLine( $"Creating sentence with {gens.Count} sentences in it." );

                            SenGen sg = new SenGen( gens.ToArray(), headerSettings );
                            return sg;

                        } else {
                            //TODO error here
                            Console.WriteLine( $"Pre SenGen Parser Error: no Sentences detected." );
                            return null;
                        }
                    }

                }

            } else {
                return SenGenProcessor( s, headerSettings, tags );
            }

        }
    }

}

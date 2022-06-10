using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NGen {

    public static class ParserUtils {

        /*
         *  NOTE:
         *  remapping special characters isn't implemented yet, but when it is we need
         *  to make sure that some of them can't be remapped.
         *  eg.
         *  ^ - header - has no need to be remapped 
         *              because it only  ever appears on a line on its own without any definition
         *  # - comment - doesn't need to be remapped
         *              because it only ever matters at the beginning of a line
         *              ((NOTE - I'm not sure that this is true - need to check
         *              maybe - this example would trigger a comment
         *              name = [
         *                      #something, something
         *                      ]
         *              if so comments do need remapping))
         *              NOTE - at the moments comments do NOT check for escaping
         *  = - declare - might not need remapping
         *              because I think only the first instance is important
         *              definitly not sure about this - probably does need it
         */

        private static Dictionary<CharType, char> chars = new Dictionary<CharType, char> {
                    { CharType.comment , '#' },
                    { CharType.declare, '=' },
                    { CharType.openList, '[' },
                    { CharType.closeList, ']' },
                    { CharType.listSeparator, ',' },
                    { CharType.reference, '$' },
                    { CharType.header, '^' },
                    { CharType.noSepBefore, '<' },
                    { CharType.noSepAfter, '>' }
                };


        public static bool StringContinsList( string s ) {
            return NonEscapedCharCheck( s, CharMap( CharType.openList ) );
        }

        public  static bool StringContainsProxy( string s ) {
            return NonEscapedCharCheck( s,  CharMap( CharType.reference ) );
        }

        public static bool StringContainsDeclaration( string s ) {
            return NonEscapedCharCheck( s, CharMap( CharType.declare ) );
        }

        internal static bool StringContainsHeader( string s ) {
            return NonEscapedCharCheck( s, CharMap( CharType.header ) );
        }

        private static bool NonEscapedCharCheck( string s, char c ) {
            /*
             *  Will check whether or not a string contains a given character
             *  Returns true if the character is present, unless it has been escaped
             */

            bool contains = s.Contains( c );
            if( contains ) {

                //collect all the indexes of the given character in the string
                int[] charIndexes = GetAllCharacterIndexes( s, c );

                //go through all of the indexes, and check if the previous character was NOT an escape character
                //if it wasn't, return true
                //if they were ALL escape characters, return false
                for( int i = 0; i < charIndexes.Length; i++ ) {
                    int prevIndex = charIndexes[i] - 1;
                    if( prevIndex < 0 || s[prevIndex] != '\\' ) {
                        return true;
                    }
                }
                return false;

            } else {
                return false;
            }
        }

        private static int[] GetAllCharacterIndexes( string s, char c ) {
            List<int> charIndexes = new List<int>();
            for( int i = s.IndexOf( c ); i > -1; i = s.IndexOf( c, i + 1 ) ) {
                charIndexes.Add( i );
            }
            return charIndexes.ToArray();
        }

        public static void StringToStringPair( string s, out string name, out string contents ) {

            int divide = s.IndexOf( CharMap( CharType.declare ) );
            name = s.Substring( 0, divide ).Trim();
            contents = s.Substring( divide + 1, s.Length - divide - 1 );

        }

        public static string[] StripComments( string[] ls ) {

            /*
             *  Receives an array of strings and returns an array including 
             *  only those strings which do not begin with a # symbol
             *  
             *  also removes empty lines
             */

            List<string> strippedL = new List<string>();

            foreach( string l in ls ) {

                if( l.Trim().Length > 0 ) {

                    if( l.Trim()[0] != CharMap( CharType.comment ) ) {
                        strippedL.Add( l );
                    }
                }
            }


            return strippedL.ToArray();

        }

        public static string[] StringSplitOnUnEscapedCharacter( string s, char c ) {

            string pattern = @"(?<!\\)" + c;
            //DEBUG
            //Console.WriteLine( $"RegEx Split. String is: '{s}', char is: '{c}', pattern is: '{pattern}'");
            return Regex.Split( s, pattern );

        }

        public static string StripEscapes( string s ) {
            /*
             * Removes all escape characters from a string, unless it is preceeded by an escape
             * 
             * Note To Self:
             *      This uses RegEx, you do not understand it, don't pretend you do
             *      You used this website to generate this:
             *      https://regex101.com/r/HQm24o/1
             */

            if( s.Contains( '\\' ) ) {

                string pattern = @"(?<!\\)\\";
                RegexOptions options = RegexOptions.Multiline;
                Regex regex = new Regex( pattern, options );
                return regex.Replace( s, "" );

            } else {
                return s;
            }
        }

        public static char CharMap( CharType type ) {

            if( chars.ContainsKey( type ) ) {
                return chars[type];
            } else {
                Console.WriteLine( $"Error: CharacterMap does not contain a character for type: '{type}' " );
                return ' ';
            }

        }


        public static int StringToInt( string s ) {

            int num;
            if( int.TryParse( s, out num ) ) {

                return num;

            } else {

                //TODO document
                Console.WriteLine( $"Setting Error: '{s}' not recognised as an integer." );
                return -1;

            }
        }

        public static double StringToDouble( string s ) {

            double num;
            if( double.TryParse( s, out num ) ) {

                return num;

            } else {

                //TODO document
                Console.WriteLine( $"Setting Error: '{s}' not recognised as a double." );
                return -1;

            }
        }

        public static int GetCharacterCount( string s, char c ) {
            /*
             *  Will count unescaped instances of a character
             */

            int cCount = 0;
            if( s.Contains( c ) ) {

                //collect all the indexes of the given character in the string
                int[] charIndexes = GetAllCharacterIndexes( s, c );

                //go through all of the indexes, and count them
                //only if the previous character was NOT an escape character
                for( int i = 0; i < charIndexes.Length; i++ ) {
                    int prevIndex = charIndexes[i] - 1;
                    if( prevIndex < 0 || s[prevIndex] != '\\' ) {
                        cCount++;
                    }
                }

                return cCount;

            } else {
                return 0;
            }
        }

        public static string GetBracketsStart( string s, out string preBrackets, out string bracketsHeaderString ) {
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

                    if( c == CharMap( CharType.openList ) ) {

                        started = true;

                    } else {

                        preBrackets += c;

                    }
                }
            }

            bracketsHeaderString = ExtractBracketsHeaderString( ref preBrackets );

            return contents;
        
        }

        private static string ExtractBracketsHeaderString( ref string preBrackets ) {
            string bracketsHeaderString = "";

            //Check prebrackets for a header
            if( preBrackets.Length > 0 ) {

                int spaceIndex = preBrackets.LastIndexOf( ' ' );

                if( spaceIndex == -1 ||
                    spaceIndex < preBrackets.Length - 1 ) {

                    bracketsHeaderString = preBrackets.Substring( spaceIndex + 1 );

                    //if there is a header, remove it from the preBrackets string
                    if( preBrackets.Length > bracketsHeaderString.Length + 1 ) {

                        preBrackets = preBrackets.Substring( 0, spaceIndex );

                    } else {

                        preBrackets = "";

                    }
                }
            }

            return bracketsHeaderString;
        }

        public static string[] ExtractTagsFromName( ref string name ) {
            
            int firstBracket = name.IndexOf( '(' );
            int lastBracket = name.IndexOf( ')' );

            //"0(2345)7"
            //(f)

            if( firstBracket >= 0 && lastBracket >= 0 ) {

                string bracketText = name.Substring( firstBracket, lastBracket - firstBracket + 1 );

                string newNameString = "";

                if( firstBracket > 0 ) {

                    newNameString += name.Substring( 0, firstBracket );

                }

                if( lastBracket < name.Length - 1 ) {

                    newNameString += ' ' + name.Substring( lastBracket + 1 );

                }

                name = newNameString.Trim();

                return GenerateTags( bracketText );

            } else {

                return new string[0];

            }
        }

        private static string[] GenerateTags( string tagText ) {
            /*
             *  Extracts tags from a string which might look like one of the following:
             *      (tag)
             *      (tag1) (tag2)
             *      (tag1, tag2, tag3)
             *      (tag1, tag2) (tag3, tag4)
             */

            List<String> bracketedText = new List<string>();

            int nextOpenBracket = 0;
            int nextCloseBracket = 0;

            while( nextOpenBracket >= 0 ) {

                nextOpenBracket = tagText.IndexOf( '(', nextCloseBracket );

                if( nextOpenBracket >= 0 ) {
                    nextCloseBracket = tagText.IndexOf( ')', nextOpenBracket );
                } else {
                    nextCloseBracket = -1;
                }

                //DEBUG
                Console.WriteLine( $"Trying to split tag, '(' index is: '{nextOpenBracket}', ')' index is: '{nextCloseBracket}'" );
                Console.WriteLine( $"Will substring with: '{nextOpenBracket + 1}', '{nextCloseBracket - nextOpenBracket - 1}'" );

                if( nextOpenBracket >= 0 && nextCloseBracket >= 0 ) {

                    string t = tagText.Substring( nextOpenBracket + 1, nextCloseBracket - nextOpenBracket - 1 );
                    bracketedText.Add( t.Trim() );
                }

            }

            List<string> tags = new List<string>();

            char[] separators = { ',' };
            if( tagText.Contains( separators[0] ) ) {

                foreach( string bt in bracketedText ) {

                    string[] strings = bt.Split( separators, StringSplitOptions.RemoveEmptyEntries );
                    foreach( string s in strings ) {

                        tags.Add( s.Trim() );

                    }
                }

            } else {
                tags = bracketedText;
            }

            //DEBUG

            string ds = "";
            foreach( string t in tags ) {
                ds += t + ' ';
            }

            Console.WriteLine( $"tags are: {ds} " );


            return tags.ToArray();




            
        }

        public static string[] GetBracketsContents( string s, out string postBrackets ) {
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

                //DEBUG
                //Console.WriteLine( $"char is '{c}'" );

                //if the brackets have already been completed, add the character
                //to the postBrackets string
                if( complete ) {

                    postBrackets += c;

                } else {

                    //increment the counters
                    if( c == CharMap( CharType.closeList ) ) {
                        closeCount++;
                    } else if( c == CharMap( CharType.openList ) ) {
                        openCount++;
                    }

                    //check to see if the brackets have been closed
                    if( openCount == closeCount ) {

                        contents.Add( tempContents );
                        complete = true;

                    } else {

                        //separate by commas, unless we are above level 1 in the nesting
                        if( c == CharMap( CharType.listSeparator ) &&
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
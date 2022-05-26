using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NGen {

    public enum CharType { comment, declare, openList, closeList, listSeparator, reference, header };

    public enum PickType { random, noRepRandom, shuffle, noRepShuffle, cycle, weighted };

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
                    { CharType.header, '^' }
                };


        public static bool StringContinsList( string s ) {
            return NonEscapedCharCheck( s, CharMap( CharType.openList ) );
        }

        public  static bool StringContainsRef( string s ) {
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

        public static string[] GetDataFromTxt( string path ) {

            /*
             *  Retrieves a text file and returns it as a string array
             */

            if( File.Exists( path ) ) {

                return File.ReadAllLines( path );

            } else {

                Console.WriteLine( $"Get Data From Text File Failed: path ({path}) is invalid" );
                return null;

            }

        }
    }
}
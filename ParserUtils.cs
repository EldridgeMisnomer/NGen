using System;
using System.Collections.Generic;
using System.Linq;

namespace NGen {

    public enum CharType { comment, openList, closeList, listSeparator, reference };

    public static class ParserUtils {


        private static Dictionary<CharType, char> characters = new Dictionary<CharType, char> {
                    { CharType.comment , '#' },
                    { CharType.openList, '[' },
                    { CharType.closeList, ']' },
                    { CharType.listSeparator, ',' },
                    { CharType.reference, '$' }
                };


        public static bool StringContinsList( string s ) {
            return s.Contains( CharacterMap( CharType.openList ) );
        }

        public  static bool StringContainsRef( string s ) {
            return s.Contains( CharacterMap( CharType.reference ) );
        }

        public static void StringToStringPair( string s, out string name, out string contents ) {

            int divide = s.IndexOf( '=' );
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

                    if( l[0] != CharacterMap( CharType.comment ) ) {
                        strippedL.Add( l );
                    }
                }
            }


            return strippedL.ToArray();

        }

        public static char CharacterMap( CharType type ) {

            if( characters.ContainsKey( type ) ) {
                return characters[type];
            } else {
                Console.WriteLine( $"Error: CharacterMap does not contain a character for type: '{type}' " );
                return ' ';
            }

        }
    }
}
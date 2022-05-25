using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NGen {

    public enum CharType { comment, declare, openList, closeList, listSeparator, reference };

    public static class ParserUtils {


        private static Dictionary<CharType, char> characters = new Dictionary<CharType, char> {
                    { CharType.comment , '#' },
                    { CharType.declare, '=' },
                    { CharType.openList, '[' },
                    { CharType.closeList, ']' },
                    { CharType.listSeparator, ',' },
                    { CharType.reference, '$' }
                };


        public static bool StringContinsList( string s ) {
            return s.Contains( CharMap( CharType.openList ) );
        }

        public  static bool StringContainsRef( string s ) {
            return s.Contains( CharMap( CharType.reference ) );
        }

        public static bool StringContainsDeclaration( string s ) {
            return s.Contains( CharMap( CharType.declare ) );
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

        public static char CharMap( CharType type ) {

            if( characters.ContainsKey( type ) ) {
                return characters[type];
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace NGen {
    public static class TagWrangler {

        public static string[] ExtractTagsFromName( ref string name ) {

            int firstBracket = name.IndexOf( '(' );
            int lastBracket = name.IndexOf( ')' );

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
                //Console.WriteLine( $"Trying to split tag, '(' index is: '{nextOpenBracket}', ')' index is: '{nextCloseBracket}'" );
                //Console.WriteLine( $"Will substring with: '{nextOpenBracket + 1}', '{nextCloseBracket - nextOpenBracket - 1}'" );

                if( nextOpenBracket >= 0 && nextCloseBracket >= 0 ) {

                    string t = tagText.Substring( nextOpenBracket + 1, nextCloseBracket - nextOpenBracket - 1 );
                    bracketedText.Add( t.Trim().ToLower() );
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


            /*          
                        //DEBUG
                        string ds = "";
                        foreach( string t in tags ) {
                            ds += t + ' ';
                        }

                        Console.WriteLine( $"tags are: {ds} " );
            */

            return tags.ToArray();
        }

        public static int ScoreTags( string[] userTags, string[] genTags ) {

            int score = 0;

            for( int i = 0; i < userTags.Length; i++ ) {

                if( genTags.Contains( userTags[i] ) ) {
                    score++;
                }

            }

            return score;

        }

    }
}
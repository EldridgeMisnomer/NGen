using System;

namespace Utils {

    public static class EnumHelpers {

        public static void StringToEnum<T>( string source, string type, ref T e ) where T : System.Enum {
            /*
             *  Does a little trimming on a string, 
             *  and then looks to see if it contains an enum name
             *  sets the referenced enum to that name if it exists
             */


            int startIndex = source.IndexOf( type );
            if( startIndex >= 0 ) {
                startIndex += type.Length;
                int equalsIndex = source.IndexOf( '=', startIndex );
                string searchString = source.Substring( equalsIndex + 1 );

                int enumNameIndex = DoesStringContainEnumName<T>( searchString );

                if( enumNameIndex >= 0 ) {

                    e = (T)(object)enumNameIndex;

                }

            }

        }

        public static int DoesStringContainEnumName<T>( string s ) where T : System.Enum {
            /*
             *  Checks to see if the string contains any of the names contained in the given enum
             *  If it does, returns an int which can later be converted 
             *  back into an enum, if it doesn't, returns -1
             */


            string[] names = Enum.GetNames( typeof( T ) );

            for( int i = 0; i < names.Length; i++ ) {
                if( s.Contains( names[i] ) ) {

                    //DEBUG
                    Console.WriteLine( $"enum name found: '{names[i]}'" );

                    return i;
                }
            }

            return -1;

        }
    }
}

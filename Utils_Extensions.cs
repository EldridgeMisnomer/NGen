using System.Linq;

namespace Utils {

    public static class Extensions {
        
        public static int[] AllIndexesOf<T>( this T[] array, T value ) {

            //Let's be honest - I do not understand this
            //I got it from here: https://stackoverflow.com/a/10443540
            return array.Select( ( b, i ) => object.Equals( b, value ) ? i : -1 ).Where( i => i != -1 ).ToArray();

        }
    }

    public static class Glitcher {

        private static char[] glitchChars = { '#', '*', '$', '@', '%', '&', '!', '¡', '~'  };

        public static string Glitch( this string s, double perCharChance = 0.1 ) {

            char[] sChars = s.ToCharArray();

            for( int i = 0; i < sChars.Length; i++ ) {

                if ( Rand.NextDouble() < perCharChance ) {
                    char newChar = glitchChars[Rand.RandomRangeInt( 0, glitchChars.Length )];
                    sChars[i] = newChar;
                }

            }

            return new string(sChars);

        }

        public static void ChangeGlitchChars( char[] chars ) {
            glitchChars = chars;
        }

        public static void AddGlitchChars( params char[] chars ) {

            char[] newGlitchChars = new char[chars.Length + glitchChars.Length];
            for( int i = 0; i < newGlitchChars.Length; i++ ) {

                if( i >= glitchChars.Length ) {
                    int accessInd = i - glitchChars.Length;
                    newGlitchChars[i] = chars[accessInd];
                } else {
                    newGlitchChars[i] = glitchChars[i];
                }
            }

            glitchChars = newGlitchChars;


        }

    }
}

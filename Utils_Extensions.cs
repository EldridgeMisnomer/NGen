using System.Linq;

namespace Utils {

    public static class Extensions {
        
        public static int[] AllIndexesOf<T>( this T[] array, T value ) {

            //Let's be honest - I do not understand this
            //I got it from here: https://stackoverflow.com/a/10443540
            return array.Select( ( b, i ) => object.Equals( b, value ) ? i : -1 ).Where( i => i != -1 ).ToArray();

        }
    }
}

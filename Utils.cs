using System;


namespace NGen {
    public static class Utils {

        private static Random rand = new Random();

        public static T NonRepeatingRandFromArray<T>( T[] array, ref int lastChoice ) {

            lastChoice = ( rand.Next( lastChoice + 1, lastChoice + array.Length - 1 ) ) % array.Length;
            return array[lastChoice];

        }

        public static T RandFromArray<T>( T[] array ) {
            int choice = rand.Next( 0, array.Length );
            return array[choice];
        }

    }

}

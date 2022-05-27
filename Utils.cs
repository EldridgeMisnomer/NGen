using System;
using System.Collections.Generic;

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


        public static void Shuffle<T>( this List<T> list ) {
            /*
             *  Fisher–Yates based shuffle for a list
             */

            int count = list.Count;
            while( count > 1 ) {
                count--;
                int pos = rand.Next( count + 1 );
                T value = list[pos];
                list[pos] = list[count];
                list[count] = value;
            }
        }

        public static void Shuffle<T>( this T[] array ) {
            /*
             *  Fisher–Yates based shuffle for an array
             */

            int count = array.Length;
            while( count > 1 ) {
                count--;
                int pos = rand.Next( count + 1 );
                T value = array[pos];
                array[pos] = array[count];
                array[count] = value;
            }

        }

        public static void NonRepeatingShuffle<T>( this T[] array ) {
            /*
             *  Fisher–Yates based shuffle for an array
             *  it stops the last element from becoming the first element
             *  thereby avoiding repeats
             *  
             *  it only does this to arrays longer than 2 elements
             *  because a 2 element list always stays the same
             *      0, 1 --> 0, 1 
             *      
             *  although it'll work on other small-element lists,
             *  it's probably not a good idea to limit the permutations like this
             *  at the very least for 3 and 4 element lists
             */

            int count = array.Length;

            if( count > 2 ) {

                while( count > 1 ) {
                    count--;
                    int pos;
                    if( count == array.Length - 1 ) {
                        pos = rand.Next( 1, count );
                    } else {
                        pos = rand.Next( count + 1 );
                    }
                    T value = array[pos];
                    array[pos] = array[count];
                    array[count] = value;
                }

            } else {

                array.Shuffle();

            }


        }

    }

}

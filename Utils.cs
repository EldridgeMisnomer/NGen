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
             */

            int count = array.Length;
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

        }

    }

}

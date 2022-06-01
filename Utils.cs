using System;
using System.Collections.Generic;

namespace Utils {

    public static class Rand {

        private static readonly Random rand = new Random();

        //normal random
        private static double spare;
        private static Boolean haveSpare = false;

        public static double DoNothing() {
            return 10;
        }

        public static double RandomNormal( double mean = 0, double deviation = 1 ) {
            /*
             *  Return a random number following a normal distribution 
             *  with a given mean and standard deviation
             *  
             *  Uses the Marsaglia polar method, 
             *  from the Wikipedia page: https://en.wikipedia.org/wiki/Marsaglia_polar_method
             */

            if( haveSpare ) {
                haveSpare = false;
                return spare * deviation + mean;
            } else {

                //initial values have no meaning, just have to initialise them or else!
                double u = 1.0;
                double v = 1.0;
                double s = 1.2;

                while( s >= 1 || s == 0) {
                    u = rand.NextDouble() * 2 - 1;
                    v = rand.NextDouble() * 2 - 1;
                    s = u * u + v * v;
                }

                s = Math.Sqrt( -2.0 * Math.Log( s ) / s );
                spare = v * s;

                haveSpare = true;

                return mean + deviation * u * s;
            }
        }

        public static double RandomNormalRangeMeanDev( double min, double max, double mean, double deviation ) {
            if( min < 0 ) min = 0;

            //make max high so first while happens
            double num = max * 2;

            //use a counter to track how often this runs
            //because a badly set min and max could make this run indefinitely
            int counter = 0;
            int loopMax = 100;

            while( num > max || num < min ) {

                counter++;
                num = RandomNormal( mean, deviation );

                //if this has run too many times
                //set the output to the minimum
                //and display an error
                if( counter >= loopMax ) {
                    num = min;
                    Console.WriteLine( $"Repeat Error: the number of repeats generated with the given mean and standard deviation fell outside the given minimum and maximum (min:{min}, max:{max}, mean:{mean}, stdDev:{deviation})" );
                }
            }

            return num;
        }

        public static double RandomNormalRange( double min = 1, double max = 10 ) {
            /*
             *  Uses the Normal function to produce a random number
             *  min is used as the mean also
             *  the max is used to calculate the standard deviation
             */

            double stdDev = (max-min) / 4;
            double num = RandomNormalRangeMeanDev( min, max, min, stdDev );
            return num;

        }

        public static int RandomNormalMeanDevInt( int min, int max, int mean, int deviation ) {

            double dNum = RandomNormalRangeMeanDev( (double)min, (double)(max + 1), (double)mean, (double)deviation );
            int num = (int)Math.Floor( dNum );
            return num;
        }

        public static int RandomNormalRangeInt( int min, int max ) {

            double dNum = RandomNormalRange( (double)min, (double)(max + 1) );
            int num = (int)Math.Floor( dNum );
            return num;

        }

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

        public static bool PercChanceTest( double percChance ) {
            double chance = percChance / 100;
            return ChanceTest( chance );
        }

        public static bool ChanceTest( double chance ) {
            return rand.NextDouble() < chance;
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
             *  
             *  TODO - I don't think this actually works - it works the first time, 
             *  but then there's nothing stopping the last number from later being
             *  shuffled into the first position
             *  something more complicated is needed, I think
             *  
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

        public static int RandomDoubleWeightedInt( double[] weights ) {

            double weightSum = 0;
            foreach( double d in weights ) {
                weightSum += d;
            }

            double random = NextDouble( weightSum );

            for( int i = 0; i < weights.Length; i++ ) {
                if( weights[i] != 0 && random < weights[i] ) {
                    return i;
                }

            random -= weights[i];

            }

            return weights.Length - 1;
        }

        public static double NextDouble( double max ) {

            return rand.NextDouble() * max;

        }

        public static double NextDouble( double min, double max ) {
            return ( rand.NextDouble() * ( max - min ) ) + min;
        }

        public static int RandomWeightedInt( int[] weights ) {
            //this function picks a random integer using a list of probablilty weights.

            //sum the weights
            int weightSum = 0;
            for( int i = 0; i < weights.Length; i++ ) {
                weightSum += weights[i];
            }

            //generate a random number based on the sum of the weights
            int random = rand.Next( weightSum );

            for( int i = 0; i < weights.Length; i++ ) {
                if( weights[i] != 0 && random < weights[i] ) {
                    return i;
                }
                random -= weights[i];
            }

            return weights.Length - 1;

        }

        public static int RandomRangeInt( int min, int max ) {

            return rand.Next( min, max );

        }

        public static double[] CalculateLinearWeightsFromMinMax( double min, double max, int numWeights, bool maxFirst = true ) {

            double[] weights = new double[numWeights];

            //if we only need 2 weights, just return the min and the max
            if( numWeights == 2 ) {

                if( maxFirst ) {
                    weights[0] = max;
                    weights[1] = min;
                } else {
                    weights[0] = min;
                    weights[1] = max;
                }

            } else {

                //if there's no difference between min and max
                //make all weights the same
                double dif = max - min;

                if( dif == 0 ) {

                    for( int i = 0; i < weights.Length; i++ ) {
                        weights[i] = min;
                    }

                } else {

                    //otherwise, calculate new weights
                    double spacing = dif / (double)( numWeights - 1 );

                    double weight;
                    if( maxFirst ) {
                        weight = max;
                    } else {
                        weight = min;
                    }

                    for( int i = 0; i < weights.Length; i++ ) {
                        weights[i] = weight;
                        if( maxFirst ) {
                            weight -= spacing;
                        } else {
                            weight += spacing;
                        }
                    }

                }
            }

            return weights;
        }

        public static double[] CalculateWeightsFromMult( double mult, int numWeights ) {

            double weight;

            if( mult > 0 ) {
                weight = 1;
            } else {
                weight = 1000;
            }

            double[] weights = new double[numWeights];

            for( int i = 0; i < weights.Length; i++ ) {

                weights[i] = weight;
                weight *= mult;

            }

            return weights;

        }
    }
}

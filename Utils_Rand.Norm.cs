using System;

namespace Utils {
    public static partial class Rand {
        public static class Norm {

            //normal random
            private static double spare;
            private static Boolean haveSpare = false;


            public static double Next( double mean = 0, double deviation = 1 ) {
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

                    while( s >= 1 || s == 0 ) {
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

            public static double Range( double min = 1, double max = 10 ) {
                /*
                 *  Uses the Normal function to produce a random number
                 *  min is used as the mean also
                 *  the max is used to calculate the standard deviation
                 */

                double stdDev = ( max - min ) / 4;
                double num = RangeMeanDev( min, max, min, stdDev );
                return num;

            }

            public static double RangeMeanDev( double min, double max, double mean, double deviation ) {
                if( min < 0 )
                    min = 0;

                //make max high so first while happens
                double num = max * 2;

                //use a counter to track how often this runs
                //because a badly set min and max could make this run indefinitely
                int counter = 0;
                int loopMax = 100;

                while( num > max || num < min ) {

                    counter++;
                    num = Next( mean, deviation );

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

            public static int RangeInt( int min, int max ) {

                double dNum = Range( (double)min, (double)( max + 1 ) );
                int num = (int)Math.Floor( dNum );
                return num;

            }

            public static int RangeMeanInt( int min, int max, int mean ) {

                double stdDev = ( max - min ) / 3.5;
                int num = RangeMeanDevInt( min, max, mean, stdDev );
                return num;

            }

            public static int RangeMeanDevInt( int min, int max, int mean, double deviation ) {

                double dNum = RangeMeanDev( (double)min, (double)( max + 1 ), (double)mean, deviation );
                int num = (int)Math.Floor( dNum );
                return num;
            }
        }
    }
}

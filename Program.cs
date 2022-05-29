using System;
using System.Collections.Generic;

namespace NGen {
    class Program {
        static void Main( string[] args ) {

            NGen nGen = DataGetter.ParseTxtFile( "data/names_01.txt" );

            string[] genNames = nGen.GetGenNames();

            //Test Code
            int numTestToRunPerName = 12;

            foreach( string s in genNames ) {
                Console.WriteLine( $"{s}:" );
                for( int i = 0; i < numTestToRunPerName; i++ ) {
                    Console.WriteLine( "\t" + nGen.GenTxt(s) );
                }
                Console.WriteLine( "" );
            }
/*
            List<int> nums = new List<int>();

            for( int i = 0; i < 200000; i++ ) {
                //Console.WriteLine( "r: " + Utils.GetNormalRand( 0, 1 ).ToString( "f2" ) );
                //Console.WriteLine( "r: " + (int)Utils.GetBoundedNormalRand() );
                //nums.Add( (int)Utils.GetNormalRand(0, 5) );
                nums.Add( (int)Utils.GetBoundedNormalRandInt(0, 14) );
                *//*
                 * Equivalent maximums
                 * st   max
                 * 1  =  3
                 * 2  =  7
                 * 3  =  11/12
                 * 4  =  15/16
                 * 5  =  20
                 * 
                 * so ~~ aprox max = stdDev * 4
                 *//*
            }

            int[] sums = { 0, 0, 0, 0, 0,
                           0, 0, 0, 0, 0, 
                           0, 0, 0, 0};

            for( int i = 0; i < nums.Count; i++ ) {
                if( nums[i] >= 0 && nums[i] < sums.Length ) {
                    sums[nums[i]] += 1;
                }
            }

            float[] percs = new float[sums.Length];

            int tot = nums.Count;
            for( int i = 0; i < sums.Length; i++ ) {
                percs[i] = ( (float)sums[i] / (float)tot * 100 );
            }

            string l1 = "";
            string l2 = "";
            string l3 = "";

            for( int i = 0; i < percs.Length; i++ ) {
                l1 += $"\t {i}";
                l2 += $"\t {sums[i]}";
                l3 += $"\t {percs[i].ToString("f2")}%";
            }

            Console.WriteLine( l1 );
            Console.WriteLine( l2 );
            Console.WriteLine( l3 );*/


            //Stop the program from exiting
            string cont = Console.ReadLine().Trim().ToLower();
            Console.WriteLine( cont );
            Environment.Exit( 0 );


        }
    }

  

}

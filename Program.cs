using System;
using CUILib;

namespace NGen {
    class Program {
        static void Main( string[] args ) {

            //UIElements.Box( "This is some text in a box.\n And here is a second line, && I love you \n And here is a really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really long line." );

            string title = "_____ _____         \n|   | |   __|___ ___ \n| | | |  |  | -_|   |\n|_|___|_____|___|_|_|";
            UIElements.SetTitle( title );
            UIElements.H1();

            PageStack ps = new PageStack();
            Paragraph pg = new Paragraph {
                text = "Suscipit architecto dolorum ullam necessitatibus iure. Sit ea minima tempore deleniti maxime sapiente consequatur maiores. Saepe aliquid dolorem eos. Fuga minima incidunt autem cum perspiciatis. Ut aut occaecati modi.\n\nSuscipit architecto dolorum ullam necessitatibus iure. Sit ea minima tempore deleniti maxime sapiente consequatur maiores. Saepe aliquid dolorem eos. Fuga minima incidunt autem cum perspiciatis. Ut aut occaecati modi."
            };

            Page p = new Page( "test one", pg );
            ps.AddPage( "one", p );
            ps.ShowPage( "one" );



            bool loadFromJSON = false;

            NGen nGen;
            if( loadFromJSON ) {

                string path = "JSON output/names_01.json";
                nGen = FileHandler.JSONToNGen( path );

            } else {

                nGen = FileHandler.TxtFileToNGen( "data/names_01.txt" );

            }

            string[] genNames = nGen.GetGenNames();

            bool testJSON = false;

            if( testJSON ) {

                nGen.TurnIntoJSON();

            } else {

                int numTestToRunPerName = 30;
                bool runAllGens = false;
                string[] selectNames = { "name" };


                string[] gensToRun;

                if( runAllGens ) {
                    gensToRun = genNames;
                } else {
                    gensToRun = selectNames;
                }


                foreach( string s in gensToRun ) {
                    //Console.WriteLine( $"{s}:" );
                    for( int i = 0; i < numTestToRunPerName; i++ ) {
                        //Console.WriteLine( "\t" + nGen.GenTxt( s ) );
                    }
                    //Console.WriteLine( "" );
                }
            }



/*            string test = "0";
            string[] testSplit = test.Split( '-' );
            Console.WriteLine( $"length of testSplit is:{testSplit.Length}" );
*/
/*            int dif = 3;

            double[] weights = { 1, 2, 3, 5, 5 };
            double lastWeight = weights[weights.Length - 1];
            double penultimateWeight = weights[weights.Length - 2];

            double min = Math.Min( lastWeight, penultimateWeight );
            double max = Math.Max( lastWeight, penultimateWeight );

            //note - missing weights include penultimate and last weights
            double[] missingWeights = Utils.CalculateLinearWeightsFromMinMax( min, max, dif + 2, penultimateWeight > lastWeight );

            double[] newWeights = new double[ weights.Length + dif ];

            for( int i = 0; i < newWeights.Length; i++ ) {

                if( i < weights.Length - 2 ) {

                    newWeights[i] = weights[i];

                } else {
                    // 3, 4, 5  -> 0, 1, 3
                    newWeights[i] = missingWeights[i - weights.Length + 2 ];

                }
                Console.WriteLine( $"{i}: {newWeights[i]}" );
            }*/




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

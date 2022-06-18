using System;
using System.Collections.Generic;
using Utils;

namespace NGen {
    class Program {
        static void Main( string[] args ) {

            //Testing Switches and Parameters
            bool loadFromJSON = false;
            bool testJSON = false;
            bool displayGenNames = true;
            bool runAllGens = false;
            bool testMainGens = false;
            bool runRepeatCountTest = false;
            bool runGlitchTest = false;


            string[] gensToRun = { "name" };
            string[] tagsToUse = { };

            int numTestToRunPerName = 30;

            if( runGlitchTest ) {

                string gt = "If I was a lovely combine harvester and you were a key, I'd wrap you up in silver foil and throw you in the barley.";

                Glitcher.AddGlitchChars( new char[] { 'a', 'b'} );

                gt = gt.Glitch();
                Console.WriteLine(gt);


            }


            NGen nGen;
            if( loadFromJSON ) {

                string path = "JSON output/names_01.json";
                nGen = FileWrangler.JSONToNGen( path );

            } else {

                string path = "data/names_01.txt";
                nGen = FileWrangler.TxtFileToNGen( path );

            }

            string[] genNames; 
            if( testMainGens ) {

                genNames = nGen.GetGenNames( true );

            } else {

                genNames = nGen.GetGenNames();

            }


            if( displayGenNames ) {
                string s = "";
                foreach( string gn in genNames ) {
                    s += $"'{gn}', ";
                }
                Console.WriteLine( $"gen names are: {s}" );
            }

            if( testJSON ) {

                nGen.TurnIntoJSON();

            } else {

                string[] gens;

                if( runAllGens ) {
                    gens = genNames;
                } else {
                    gens = gensToRun;
                }

                if( runRepeatCountTest ) {

                    //List<string>[] output = new List<string>[gens.Length];

                    int[][] repCounts = new int[gens.Length][];

                    for( int i = 0; i < gens.Length; i++ ) {

                        //output[i] = new List<string>();
                        repCounts[i] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                        for( int j = 0; j < numTestToRunPerName; j++ ) {
                            //output[i].Add(  ) );

                            string[] wrds = nGen.GenTxt( gens[i] ).Split( ' ' );
                            int numReps = wrds.Length - 1;
                            repCounts[i][numReps]++;

                        }
                    }

                    //print results
                    Console.WriteLine( $"{numTestToRunPerName} runs per gen." );
                    Console.WriteLine( "" );
                    Console.WriteLine( "" );

                    for( int i = 0; i < repCounts.Length; i++ ) {

                        Console.WriteLine( $"{gens[i]} reps:" );
                        Console.WriteLine( "" );

                        string l1 = "";
                        string l2 = "";
                        for( int j = 0; j < repCounts[i].Length; j++ ) {

                            l1 += "\t" + repCounts[i][j];
                            l2 += "\t" + j;
                        }

                        Console.WriteLine( l1 );
                        Console.WriteLine( l2 );
                        Console.WriteLine( "" );
                        Console.WriteLine( "" );

                    }


                } else {

                    foreach( string s in gens ) {

                        Console.WriteLine( $"{s}:" );

                        for( int i = 0; i < numTestToRunPerName; i++ ) {

                            Console.WriteLine( "\t" + nGen.GenTxt( s, tagsToUse ) );

                        }

                        Console.WriteLine( "" );
                    }
                }
            }



            //Stop the program from exiting
            string cont = Console.ReadLine().Trim().ToLower();
            Console.WriteLine( cont );
            Environment.Exit( 0 );


        }
    }
}

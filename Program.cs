using System;
using System.Collections.Generic;

namespace NGen {
    class Program {
        static void Main( string[] args ) {

            //Testing Switches and Parameters
            bool loadFromJSON = false;
            bool testJSON = false;
            bool displayGenNames = true;
            bool runAllGens = false;
            bool testMainGens = false;

            string[] gensToRun = { "name" };
            string[] tagsToUse = { };

            int numTestToRunPerName = 30;

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

                foreach( string s in gens ) {

                    Console.WriteLine( $"{s}:" );

                    for( int i = 0; i < numTestToRunPerName; i++ ) {

                        Console.WriteLine( "\t" + nGen.GenTxt( s, tagsToUse ) );

                    }

                    Console.WriteLine( "" );

                }
            }



            //Stop the program from exiting
            string cont = Console.ReadLine().Trim().ToLower();
            Console.WriteLine( cont );
            Environment.Exit( 0 );


        }
    }
}

using System;

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


            //Stop the program from exiting
            string cont = Console.ReadLine().Trim().ToLower();
            Console.WriteLine( cont );
            Environment.Exit( 0 );


        }
    }

  

}

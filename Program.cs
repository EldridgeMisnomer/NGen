using System;
using System.Collections.Generic;


namespace NGen {
    class Program {
        static void Main( string[] args ) {

            //string[] w1 = { "Yelo", "Pick", "Vicious", "Silk" };
            //string[] w2 = { "Wip", "Vintage", "Master", "Lep" };

            //ListGen wc1 = new ListGen( w1 );
            //ListGen wc2 = new ListGen( w2 );

            //ListGen[] wca = { wc1, wc2 };

            //SenGen s = new SenGen( wca );


            //for( int i = 0; i < 100; i++ ) {
            //    string ws = s.GetTxt();
            //    Console.WriteLine( ws );
            //}

            DataGetter.ParseTxtFile( "data/names_01.txt" );


            //Stop the program from exiting
            string cont = Console.ReadLine().Trim().ToLower();
            Console.WriteLine( cont );
            Environment.Exit( 0 );
        }
    }

  

}

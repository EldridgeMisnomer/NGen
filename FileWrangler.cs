using System;
using System.Collections.Generic;
using System.IO;
using TinyJSON;

namespace NGen {

    public static class FileWrangler {

        public static void NGenToJSON( NGen ngen ) {

            //EncodeOptions.IncludePublicProperties
            //EncodeOptions.PrettyPrint
            string nGenJSON = JSON.Dump( ngen, EncodeOptions.IncludePublicProperties );

            Console.WriteLine( $"JSON test 1: {nGenJSON}" );

            string location = "JSON output/";
            string file = "names_01.json";

            if( !Directory.Exists(location) ) {
                Directory.CreateDirectory( location );
            }

            File.WriteAllText( location + file, nGenJSON );

        }

        public static NGen JSONToNGen( string path ) {

            if( File.Exists( path ) ) {

                NGen n;

                string json = File.ReadAllText( path );

                JSON.MakeInto( JSON.Load( json ), out n );

                if( n.remapDict != null ) {
                    ParserUtils.RemapChars( n.remapDict );
                }

                return n;

            }

            return null;

        }

        public static string[] GetTxtFromFile( string path ) {

            /*
             *  Retrieves a text file and returns it as a string array
             */

            if( File.Exists( path ) ) {

                return File.ReadAllLines( path );

            } else {

                Console.WriteLine( $"Get Data From Text File Failed: path ({path}) is invalid" );
                return null;

            }

        }

        public static NGen TxtFileToNGen( string path ) {

            //Get the file as an array of lines
            string[] lines = FileWrangler.GetTxtFromFile( path );
            //Remove the comments
            string[] strippedLines = ParserUtils.StripCommentsAndEmpties( lines );
            //process the lines into Gens with names
            Dictionary<string, OutputGen> gens = DataGetter.SplitHeadersAndGenerators( strippedLines );

            //get a list of all the proxygens for checking
            List<ProxyGen> proxyGens = GenProcessors.GetProxyGens();

            //check proxyGens
            if( proxyGens.Count > 0 ) {
                for( int i = 0; i < proxyGens.Count; i++ ) {
                    string name = proxyGens[i].GetName();

                    if( gens.ContainsKey( name ) ) {

                        proxyGens[i].SetGen( gens[name] );

                    } else {
                        Console.WriteLine( $"Proxy Error: '{name}' Proxy has not been created because the Generator did not exist" );
                    }
                }
            }

            //Create the nGen and return it
            NGen nGen = new NGen( gens );

            //if there's a remap dictionary, add it to the gen
            if( DataGetter.remapDict != null ) {
                nGen.remapDict = DataGetter.remapDict;
                DataGetter.remapDict = null;
            }

            return nGen;

        }


    }
}

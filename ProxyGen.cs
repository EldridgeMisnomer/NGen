using System.Collections.Generic;
using Utils;

namespace NGen {
    public class ProxyGen : Gen {
        /*
         *  This is a Gen which stands as a proxy for another named Gen
         * 
         *  It has one setting it uses, so far:
         *      Once - where it only retrieves text from a Gen the first time
         *          and after that it alawys returns the same text
         */

        public SenGen gen;
        public string genName;

        public GenOutput[] onceText = null;

        public ProxyGen( string name, GenSettings genSettings ) {
            genName = name;

            gs = genSettings;

            //DEBUG
            //Console.WriteLine( $"Create ProxyGen, outputChance = {gs.OutputChance}" );
        }

        public ProxyGen() { }

        protected override GenOutput[] PickTxt() {
            if( gen == null ) {

                return null;

            } else {

                if( gs.Once ) {

                    if( onceText == null ) {

                        onceText = gen.GetOutput();

                    }
                    return onceText;

                } else {

                    return gen.GetOutput();
                }
            }
        }

        public override GenOutput[] GetOutput() {

            if( gs.OutputChance == 1 || Rand.ChanceTest( gs.OutputChance ) ) {

                //Get Number of repeats, GetTxt that number of times and make GenOutputs from it, return the GenOutputs
                int repeats = GetRepeatNum();
                List<GenOutput> gens = new List<GenOutput>();
                for( int r = 0; r < repeats + 1; r++ ) {
                    GenOutput[] newGOs = PickTxt();

                    if( newGOs != null && newGOs.Length > 0 ) {

                        foreach( GenOutput go in newGOs ) {
                            gens.Add( go );
                        }
                    }
                }

                return gens.ToArray();

            } else {

                return null;

            }
        }

        public void SetGen( SenGen g ) {
            gen = g;
        }

        public string GetName() {
            return genName;
        }

        public string GetProxyTxt() {
            return gen.GetTxt();
        }

    }
}

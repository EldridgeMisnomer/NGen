using System.Collections.Generic;
using Utils;

namespace NGen {
    public class ProxyGen : Gen {
        /*
         *  This is a Gen which stands as a proxy for another named Gen
         * 
         *  It can repeat and have an output chance - just like a list
         *  It has one unique setting:
         *      Once - where it only retrieves text from a Gen the first time
         *          and after that it alawys returns the same text
         *      TODO - look into how to make this a per-run setting.
         */

        public OutputGen gen;
        public string genName;

        public GenOutput[] onceText = null;

        public ProxyGen( string name, GenSettings genSettings ) {
            genName = name;
            gs = genSettings;
        }

        public ProxyGen() { }

        protected override GenOutput[] PickTxt( params string[] tags ) {
            if( gen == null ) {

                return null;

            } else {

                if( gs.Once ) {

                    if( onceText == null ) {

                        onceText = gen.GetOutput(  tags  );

                    }
                    return onceText;

                } else {

                    return gen.GetOutput( tags  );
                }
            }
        }

        public override GenOutput[] GetOutput( params string[] tags ) {

            if( gs.OutputChance == 1 || Rand.ChanceTest( gs.OutputChance ) ) {

                //Get Number of repeats, GetTxt that number of times and make GenOutputs from it, return the GenOutputs
                int repeats = GetRepeatNum();
                List<GenOutput> gens = new List<GenOutput>();
                for( int r = 0; r < repeats + 1; r++ ) {
                    GenOutput[] newGOs = PickTxt( tags  );

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

        public void SetGen( OutputGen g ) {
            gen = g;
        }

        public string GetName() {
            return genName;
        }

        public string GetProxyTxt() {
            //This is just used in the context of separators
            return gen.GetTxt();
        }

    }
}

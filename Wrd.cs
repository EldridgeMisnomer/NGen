using System;
using Utils;

using PU = NGen.ParserUtils;

namespace NGen {
    public class Wrd : Gen {
        /*
         *  A container for just one word, a string which will always be returned
         *  To be used in complicated WrdGens with nesting
         */

        public string wrd;

        public Wrd( string str, GenSettings genSettings ) {

            gs = new GenSettings(genSettings);
            //stop wrds getting repeats and separators
            //gs.Reset();

            wrd = PU.StripEscapes(str.Trim());
        }

        public Wrd() { }

        protected override GenOutput[] PickTxt( params string[] tags ) {
            GenOutput[] newGOs = new GenOutput[] { new GenOutput( Glitch(), gs ) };
            return newGOs;
        }

        public override GenOutput[] GetOutput( params string[] tags ) {
            return PickTxt( tags );
        }

        private string Glitch() {

            if( gs.Glitch ) {
               
                string s = wrd;
                s = s.Glitch( gs.GlitchChance/100 );

                if( gs.PermaGlitch ) {

                    if( gs.CleanFirst ) {

                        string clean = wrd;
                        wrd = s;
                        return clean;

                    } else {

                        wrd = s;
                        return wrd;

                    }
                } else {
                    return s;
                }
            } else {
                return wrd;
            }
        }

    }
}

using System.Collections.Generic;

namespace NGen {
    public class SenGen : Gen {

        /*
         * A container for word choices
         * Will output a sentence constructed from one word from each of its Gens
         */

        public Gen[] wrds;

        public SenGen( Gen[] gens, GenSettings genSettings ) {
            gs = genSettings;
            wrds = gens;
        }
        public SenGen
() { }

        public string GetTxt( out bool sepBefore, out bool sepAfter ) {

            GenOutput[] outputs = PickTxt();
            string outputString = "";

            sepBefore = true;
            sepAfter = true;

            if( outputs != null ) {

                for( int i = 0; i < outputs.Length; i++ ) {
                    outputString += outputs[i].Txt;
                    //DEBUG
                    //Console.WriteLine( $"sengen txt: output {i} is: '{outputs[i].Txt}', sepBefore: {outputs[i].SepBefore}, speAfter: {outputs[i].SepAfter}" );
                    if( i < outputs.Length - 1 ) {

                        //DEBUG

                        if( outputs[i].SepAfter && outputs[i + 1].SepBefore ) {
                            outputString += GetSeparator();
                        }

                    }
                }

                sepBefore = outputs[0].SepBefore;
                sepAfter = outputs[outputs.Length - 1].SepAfter;
            }

            return outputString.Trim();

        }

        public string GetTxt( ) {
            return GetTxt( out _, out _ );
        }

        private string GetSeparator() {

            if( gs.UseSeparator ) {

                if( gs.UseProxySeparator ) {

                    return gs.ProxySeparator.GetProxyTxt();

                } else {

                    return gs.Separator;

                }
            }

            return "";

        }

        public override GenOutput[] GetOutput() {

            bool sepBefore;
            bool sepAfter;
            string os = GetTxt( out sepBefore, out sepAfter );
            GenOutput go = new GenOutput( os, gs );
            go.SepBefore = sepBefore;
            go.SepAfter = sepAfter; 

            return new GenOutput[] { go };

        }

        protected override GenOutput[] PickTxt() {

            List<GenOutput> gens = new List<GenOutput>();

            for( int i = 0; i < wrds.Length; i++ ) {

                GenOutput[] newGOs = wrds[i].GetOutput();

                if( newGOs != null ) {

                    foreach( GenOutput go in newGOs ) {
                        gens.Add( go );

                    }
                }
            }

            //DEBUG
            //Console.WriteLine( $"SenGen PickTxt, number of wrds: {wrds.Length}, number of gens: {gens.Count}." );

            if( gens.Count > 0 ) {

                return gens.ToArray();

            } else {

                return null;

            }

        }
    }
}

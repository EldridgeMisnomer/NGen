using System;
using System.Collections.Generic;

namespace NGen {

    public class SenGen : OutputGen {

        /*
         * A container for word choices
         * Will output a sentence constructed from one word from each of its Gens
         */

        public Gen[] wrds;

        public SenGen( Gen[] gens, GenSettings genSettings ) {
            gs = genSettings;
            wrds = gens;
        }

        public string[] tags = null;

        public SenGen() { }

        public override string GetTxt( out bool sepBefore, out bool sepAfter ) {

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

            if( gens.Count > 0 ) {

                return gens.ToArray();

            } else {

                return null;

            }

        }

        public override void AddGen( OutputGen og ) {

            //TODO document
            Console.WriteLine( $"SenGen Error: Tried to add a SenGen here, should only be added to TagGens" );

        }

        public override bool IsTagGen() {
            return false;
        }
    }
}

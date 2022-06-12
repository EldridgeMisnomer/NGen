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

        public string[] ownTags = null;

        public SenGen() { }

        public override string GetTxt( out bool sepBefore, out bool sepAfter, params string[] tags ) {
            /*
            //DEBUG
            string s1 = "nada";
            string s2 = "nada";
            if( ownTags != null ) s1 = ownTags[0];
            if( tags.Length > 0 ) s2 = tags[0];
            Console.WriteLine( $"SenGen: ownTag[0] is: '{s1}', tags[0] is: '{s2}'" );
            */
            string[] pickTags = tags;
            if( ownTags != null && tags.Length == 0 ) {
                pickTags = ownTags;
            }

            GenOutput[] outputs = PickTxt(pickTags);
            //choose which tags to use
            //user tags (incoming tags) always trump own tags
            //TODO - think about this more - do we want to combine the too???

            string outputString = "";

            sepBefore = true;
            sepAfter = true;

            if( outputs != null ) {

                for( int i = 0; i < outputs.Length; i++ ) {

                    outputString += outputs[i].Txt;

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

        protected override GenOutput[] PickTxt( params string[] tags  ) {

            List<GenOutput> gens = new List<GenOutput>();

            for( int i = 0; i < wrds.Length; i++ ) {

                GenOutput[] newGOs = wrds[i].GetOutput( tags );

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
    }
}

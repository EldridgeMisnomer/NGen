using System;
using System.Collections.Generic;

namespace NGen {
    public class TagGen : OutputGen {

        public List<OutputGen> gens = new List<OutputGen>();

        public TagGen() {
            gs = new GenSettings();
        }

        public override void AddGen( OutputGen og ) {
            gens.Add( og );
        }

        public override string GetTxt( out bool sepBefore, out bool sepAfter ) {

            int choice = Utils.Rand.RandomRangeInt( 0, gens.Count );

            //DEBUG
            //Console.WriteLine( $"TagGen GetTxt. num gens: '{gens.Count}', choice: '{choice}'" );
            return gens[choice].GetTxt( out sepBefore, out sepAfter );

        }

        protected override GenOutput[] PickTxt() {
            
            int choice = Utils.Rand.RandomRangeInt( 0, gens.Count );

            //DEBUG
            //Console.WriteLine( $"TagGen GetOut. num gens: '{gens.Count}', choice: '{choice}'" );

            return gens[choice].GetOutput();

        }

        public override bool IsTagGen() {
            return true;
        }
    }
}

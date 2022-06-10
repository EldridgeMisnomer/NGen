using System.Collections.Generic;

namespace NGen {
    public class TagGen : OutputGen {

        List<SenGen> gens = new List<SenGen>();

        public override void AddGen( SenGen sg ) {
            gens.Add( sg );
        }

        public override GenOutput[] GetOutput() {
            throw new System.NotImplementedException();
        }

        public override string GetTxt( out bool sepBefore, out bool sepAfter ) {
            int choice = Utils.Rand.RandomRangeInt( 0, gens.Count );
            return gens[choice].GetTxt();
        }

        protected override GenOutput[] PickTxt() {
            throw new System.NotImplementedException();
        }

    }
}

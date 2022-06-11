namespace NGen {
    public struct GenOutput {

        public string Txt;
        public bool SepBefore;
        public bool SepAfter;

        public GenOutput( string t, GenSettings gs ) {
            Txt = t;
            SepBefore = !gs.NoSepBefore;
            SepAfter = !gs.NoSepAfter;
        }
    }
}

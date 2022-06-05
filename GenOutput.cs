namespace NGen {
    public struct GenOutput {

        public string Txt;
        public bool SepBefore;
        public bool SepAfter;

        public GenOutput( string t ) {
            Txt = t;
            SepBefore = true;
            SepAfter = true;
        }
    
    }
}

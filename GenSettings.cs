namespace NGen {

    public class GenSettings {
        public PickType pickType{ get; set; }

        public GenSettings() {
            reset();
        }

        public GenSettings( GenSettings gs ) {
            pickType = gs.pickType;
        }

        public void reset() {
            pickType = PickType.random;
        }
    }

}
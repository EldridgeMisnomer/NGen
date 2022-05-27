namespace NGen {

    public class GenSettings {
        public PickType pickType{ get; set; }

        public GenSettings() {
            reset();
        }

        public void reset() {
            pickType = PickType.random;
        }
    }

}
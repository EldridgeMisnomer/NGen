namespace NGen {

    public class GenSettings {

        //pick behaviour (only for lists)
        public PickType PickType{ get; set; }

        //repeat behaviour
        public RepeatType RepType { get; set; }

        //spearator
        public char separator = ' ';

        public int RepMax { get; set; }
        public int RepMin { get; set; }
        public int RepStdDev { get; set; }
        public int RepMean { get; set; }
        public bool UseMeanDev { get; set; }
        public int[] RepWeights { get; set; }

        public GenSettings() {
            Reset();
        }

        public GenSettings( GenSettings gs ) {

            //pick behaviour
            PickType = gs.PickType;

            //repeat behaviour
            RepType = gs.RepType;
            RepMax = gs.RepMax;
            RepMin = gs.RepMin;
            RepStdDev = gs.RepStdDev;
            RepMean = gs.RepMean;
            UseMeanDev = gs.UseMeanDev;
        }

        public void SetRepType( RepeatType rt ) {
            RepType = rt;
        }

        public void Reset() {

            //pick behaviour
            PickType = PickType.random;

            //repeat behaviour
            RepType = RepeatType.constant;
            RepMax = 0;
            RepMin = 0;
            RepStdDev = 0;
            RepMean = 0;
            UseMeanDev = false;
            int[] defaultRepWeights = { 3, 4, 2, 1 };
            RepWeights = defaultRepWeights;

            
        }

        public void SetDefaults() {
            switch( RepType ) {
                case RepeatType.constant:
                    RepMax = 0;
                    break;
                case RepeatType.uniform:
                    RepMax = 3;
                    RepMin = 0;
                    break;
                case RepeatType.normal:
                    RepMax = 4;
                    RepMin = 0;
                    UseMeanDev = false;
                    break;
                case RepeatType.weighted:
                    int[] defaultRepWeights = { 3, 4, 2, 1 };
                    RepWeights = defaultRepWeights;
                    break;
            }
        }
    }

}
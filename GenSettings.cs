namespace NGen {

    public class GenSettings {

        //pick behaviour (only for lists)
        public PickType PickType = PickType.random;

        public bool AllowRepeats = true;
        public double ShufflePoint = 1;
        public int Skip = 0;

        //pick behaviour - weights
        public double WeightFac = 0.8;
        public double WeightStart = 1;
        public double WeightEnd = 10;
        public bool WeightsFromFac = false;
        public bool WeightsFromEnds = true;
        public double[] PickWeights = new double[0];

        //repeat behaviour
        public RepeatType RepType = RepeatType.@fixed;

        public int RepMax = 0;
        public int RepMin = 0;
        public int RepMean = 0;
        public double RepStdDev = 0;
        public bool UseMean = false;
        public bool UseDev = false;
        public int[] RepWeights = { 3, 4, 2, 1 };

        //output chance
        public double OutputChance = 1;

        //spearator
        public bool UseSeparator = true;
        public string Separator = " ";
        public ProxyGen ProxySeparator = null;
        public bool UseProxySeparator = false;
        public bool NoSepBefore = false;
        public bool NoSepAfter = false;

        //once behaviour (only for proxies)
        public bool Once = false;

        public GenSettings() { }

        public GenSettings( GenSettings gs ) {

            //pick behaviour
            PickType = gs.PickType;
            AllowRepeats = gs.AllowRepeats;
            ShufflePoint = gs.ShufflePoint;
            Skip = gs.Skip;

            //pick behaviour - weights
            WeightFac = gs.WeightFac;
            WeightStart = gs.WeightStart;
            WeightEnd = gs.WeightEnd;
            WeightsFromFac = gs.WeightsFromFac;
            WeightsFromEnds = gs.WeightsFromEnds;
            PickWeights = gs.PickWeights;

            //repeat behaviour
            RepType = gs.RepType;
            RepMax = gs.RepMax;
            RepMin = gs.RepMin;
            RepStdDev = gs.RepStdDev;
            RepMean = gs.RepMean;
            UseMean = gs.UseMean;
            UseDev = gs.UseDev;
            RepWeights = gs.RepWeights;

            //output chance
            OutputChance = gs.OutputChance;

            //separator
            UseSeparator = gs.UseSeparator;
            Separator = gs.Separator;
            ProxySeparator = gs.ProxySeparator;
            UseProxySeparator = gs.UseProxySeparator;
            NoSepBefore = gs.NoSepBefore;
            NoSepAfter = gs.NoSepAfter;

            //once
            Once = gs.Once; 

        }

        public void SetRepType( RepeatType rt ) {
            RepType = rt;
        }

        public void SetRepeatDefaults() {
            switch( RepType ) {
                case RepeatType.@fixed:
                    RepMax = 0;
                    break;
                case RepeatType.uniform:
                    RepMax = 3;
                    RepMin = 0;
                    break;
                case RepeatType.normal:
                    RepMax = 4;
                    RepMin = 0;
                    UseMean = false;
                    UseDev = false;
                    break;
                case RepeatType.weighted:
                    int[] defaultRepWeights = { 3, 4, 2, 1 };
                    RepWeights = defaultRepWeights;
                    break;
            }
        }

        public void SetPickDefaults() {
            switch( PickType ) {

                case PickType.shuffle:
                    ShufflePoint = 1;
                    break;
                case PickType.cycle:
                    Skip = 0;
                    break;
                case PickType.weighted:
                    WeightFac = 0.8;
                    WeightsFromEnds = false;
                    WeightsFromFac = true;
                    double[] defaultPickWeights = new double [0];
                    PickWeights = defaultPickWeights;
                    break;

            }
        }
    }

}
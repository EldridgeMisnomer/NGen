namespace NGen {

    public class GenSettings {

        //pick behaviour (only for lists)
        public PickType PickType { get; set; }

        public bool AllowRepeats { get; set; }
        public double ShufflePoint { get; set; }
        public int Skip { get; set; }

        //pick behaviour - weights
        public double WeightFac { get; set; }
        public double WeightStart { get; set; }
        public double WeightEnd { get; set; }
        public bool WeightsFromFac { get; set; }
        public bool WeightsFromEnds { get; set; }
        public double[] PickWeights { get; set; }

        //repeat behaviour
        public RepeatType RepType { get; set; }

        public int RepMax { get; set; }
        public int RepMin { get; set; }
        public double RepStdDev { get; set; }
        public int RepMean { get; set; }
        public bool UseMean { get; set; }
        public bool UseDev { get; set; }
        public int[] RepWeights { get; set; }

        //output chance
        public double OutputChance { get; set; }

        //spearator
        public bool UseSeparator { get; set; }
        public string Separator { get; set; }
        public ProxyGen ProxySeparator { get; set; }
        public bool UseProxySeparator { get; set; }
        public bool NoSepBefore { get; set; }
        public bool NoSepAfter { get; set; }

        //once behaviour (only for proxies)
        public bool Once { get; set; }

        public GenSettings() {
            Reset();
        }

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

        public void Reset() {

            //pick behaviour
            PickType = PickType.random;
            AllowRepeats = true;
            ShufflePoint = 1;
            Skip = 0;

            //pick weights behaviour
            WeightFac = 0.8;
            WeightsFromFac = true;
            WeightsFromEnds = false;
            PickWeights = null;

            //repeat behaviour
            RepType = RepeatType.@fixed;
            RepMax = 0;
            RepMin = 0;
            RepStdDev = 0;
            RepMean = 0;
            UseMean = false;
            UseDev = false;
            int[] defaultRepWeights = { 3, 4, 2, 1 };
            RepWeights = defaultRepWeights;

            //output chance
            OutputChance = 1;

            //separator
            UseSeparator = true;
            Separator = " ";
            ProxySeparator = null;
            UseProxySeparator = false;
            double[] defaultPickWeights = new double [0];
            PickWeights = defaultPickWeights;
            NoSepBefore = false;
            NoSepAfter = false;

            //once
            Once = false;

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
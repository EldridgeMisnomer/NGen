using System;
using Utils;

using PU = NGen.ParserUtils;

namespace NGen {

    public abstract class Gen {

        public string GetTxt() {
            string s = GetOutput();
            return s;
        }

        protected abstract string PickTxt();

        //settings
        protected GenSettings gs;

        protected string AddSeparator( string s ) {

            if( gs.UseProxySeparator ) {

                s += gs.ProxySeparator.GetTxt();

            } else {

                s += gs.Separator;

            }

            return s;

        }

        protected int DoRepeats() {
            int repeats = 0;

            //Choose the number of repeats based on the repeat type
            switch( gs.RepType ) {

                case RepeatType.@fixed:
                    repeats = gs.RepMax;
                    break;

                case RepeatType.uniform:
                    repeats = Rand.RandomRangeInt( gs.RepMin, gs.RepMax + 1 );
                    break;

                case RepeatType.normal:

                    if( gs.UseMean ) {

                        if( gs.UseDev ) {

                            repeats = Rand.RandomNormalRangeMeanDevInt( gs.RepMin, gs.RepMax, gs.RepMean, gs.RepStdDev );
                            break;

                        } else {

                            repeats = Rand.RandomNormalRangeMeanInt( gs.RepMin, gs.RepMax, gs.RepMean );
                            break;

                        }


                    } else {

                        repeats = Rand.RandomNormalRangeInt( gs.RepMin, gs.RepMax );
                        break;

                    }
                case RepeatType.weighted:
                    repeats = Rand.RandomWeightedInt( gs.RepWeights );
                    break;
            }

            return repeats;
        }

        protected string GetOutput() {

            string s = "";

            if( gs.OutputChance == 1 || Rand.ChanceTest( gs.OutputChance ) ) {

                int repeats = DoRepeats();

                //get text repeatedly
                for( int i = 0; i < repeats + 1; i++ ) {
                    s += PickTxt();
                    if( i != gs.RepMax ) {
                        s = AddSeparator( s );
                    }
                }
            }

            return s;
        }


        public bool GetNoSepBefore() {
            return gs.NoSepBefore;
        }
    }

    public class Wrd : Gen {
        /*
         *  A container for just one word, a string which will always be returned
         *  To be used in complicated WrdGens with nesting
         */

        private readonly string wrd;

        public Wrd( string str, GenSettings genSettings ) {

            gs = new GenSettings(genSettings);
            //stop wrds getting repeats
            gs.Reset();
            wrd = PU.StripEscapes(str.Trim());
        }

        protected override string PickTxt() {
            return wrd;
        }
    }

    public class ProxyGen : Gen {
        /*
         *  This is a Gen which stands as a proxy for another named Gen
         * 
         *  It has one setting it uses, so far:
         *      Once - where it only retrieves text from a Gen the first time
         *          and after that it alawys returns the same text
         */

        private Gen gen;
        private readonly string genName;

        private string onceText = null;

        public ProxyGen( string name, GenSettings genSettings ) {
            genName = name;

            gs = genSettings;
        }

        protected override string PickTxt() {
            if( gen == null ) {

                return $"**{genName}**";

            } else {

                if( gs.Once ) {

                    if( onceText == null ) {

                        onceText = gen.GetTxt();

                    }
                    return onceText;

                } else {

                    return gen.GetTxt();
                }
            }
        }

        public void SetGen( Gen g ) {
            gen = g;
        }

        public string GetName() {
            return genName;
        }


    }

    public class ListGen : Gen {

        /*
         *  A container for an array of words.
         *  It will return a random word from the array each time
         */

        //words
        private readonly Gen[] wrds;

        //state
        private int lastWrd = 0;
        private int nextWrd = -1;

        public ListGen( string[] words, GenSettings settings ) {

            gs = settings;

            //convert strings into Wrds and put them into their array
            wrds = new Gen[words.Length];
            for( int i = 0; i < wrds.Length; i++ ) {
                wrds[i] = new Wrd( PU.StripEscapes( words[i].Trim() ), settings );
            }

            Setup();
        }

        public ListGen( Gen[] words, GenSettings settings ) {

            gs = settings;

            wrds = words;

            Setup();
        }

        private void Setup() {

            if( gs.PickType == PickType.cycle ||
                gs.PickType == PickType.shuffle ) {

                nextWrd = 0;

            }

            if( gs.PickType == PickType.shuffle ) {

                wrds.Shuffle();

            }

            if( gs.PickType == PickType.weighted ) {

                //calculate weigths based on the given factor
                if( gs.WeightsFromFac ) {

                    int num = wrds.Length;
                    gs.PickWeights = Rand.CalculateWeightsFromMult( gs.WeightFac, num );

                    //calculate weigths based on first and last given weights
                } else if( gs.WeightsFromEnds ) {

                    int num = wrds.Length;
                    double min = Math.Min( gs.WeightStart, gs.WeightEnd );
                    double max = Math.Max( gs.WeightStart, gs.WeightEnd );

                    gs.PickWeights = Rand.CalculateLinearWeightsFromMinMax( min, max, num, gs.WeightStart > gs.WeightEnd );

                } else {
                    //if weights have already been set,
                    //but there are a different number of weights than there are Gens in the list
                    //we'll have to calculate some to fill in the gap
                    //or remove some


                    int dif = wrds.Length - gs.PickWeights.Length;

                    //if there are too few weights
                    if( dif > 0 ) {

                        //check we have enough weights to calculate from
                        if( gs.PickWeights.Length > 1 ) {

                            //calculate the missing weights
                            double[] weights = gs.PickWeights;
                            double lastWeight = weights[weights.Length - 1];
                            double penultimateWeight = weights[weights.Length - 2];

                            double min = Math.Min( lastWeight, penultimateWeight );
                            double max = Math.Max( lastWeight, penultimateWeight );

                            //note - missing weights include penultimate and last weights
                            double[] missingWeights = Rand.CalculateLinearWeightsFromMinMax( min, max, dif + 2, penultimateWeight > lastWeight );

                            double[] newWeights = new double[wrds.Length];

                            //put old weights and new weights together in one array
                            for( int i = 0; i < newWeights.Length; i++ ) {

                                    if( i < weights.Length - 2 ) {

                                    newWeights[i] = weights[i];

                                } else {

                                    newWeights[i] = missingWeights[i - weights.Length + 2];

                                }
                            }

                            gs.PickWeights = newWeights;

                        } else {
                            //if there's only one weight given
                            //(which I don't think should ever happen - but you never know)
                            //switch to default Factor calculation

                            gs.WeightsFromFac = true;
                            gs.WeightFac = 0.8;
                            Setup();
                        }


                    } else if ( dif < 0 ) {
                        //there are more weights than Gens, so remove some weights, starting at the back

                        double[] weights = gs.PickWeights;
                        Array.Resize( ref weights, wrds.Length );
                        gs.PickWeights = weights;


                    }
                }
            }
        }

        protected override string PickTxt() {

            if( wrds.Length == 1 ) {
                return wrds[0].GetTxt();
            } else {

                switch(gs.PickType) {

                    case PickType.random:

                        if( gs.AllowRepeats ) {

                            return Rand.RandFromArray( wrds ).GetTxt();

                        } else {

                            return Rand.NonRepeatingRandFromArray( wrds, ref lastWrd ).GetTxt();

                        }

                    case PickType.shuffle:
                        string output = wrds[nextWrd].GetTxt();

                        nextWrd++;
                        if( nextWrd >= ( wrds.Length * gs.ShufflePoint ) ) {

                            if( gs.AllowRepeats ) {

                                wrds.Shuffle();

                            } else {

                                wrds.NonRepeatingShuffle();

                            }
                            nextWrd = 0;
                        }

                        return output;
                    
                    case PickType.cycle:

                        output = wrds[nextWrd].GetTxt();

                        nextWrd = ( nextWrd + 1 + gs.Skip ) % wrds.Length;
                        return output;

                    case PickType.weighted:

                        return wrds[Rand.RandomDoubleWeightedInt( gs.PickWeights )].GetTxt();

                    default:

                        return Rand.RandFromArray( wrds ).GetTxt();

                }
            }
        }
    }

    public class SenGen : Gen {

        /*
         * A container for word choices
         * Will output a sentence constructed from one word from each of its Gens
         */

        private readonly Gen[] wrds;

        public SenGen( Gen[] gens, GenSettings genSettings ) {
            gs = genSettings;
            wrds = gens;
        }

        protected override string PickTxt() {

            string s = "";

            for( int i = 0; i < wrds.Length; i++ ) {

                s += wrds[i].GetTxt();

                if( i != wrds.Length - 1 ) {

                    if( !wrds[i + 1].GetNoSepBefore() ) {

                        s = AddSeparator( s );
                    }

                }
            }

            return s;

        }
    }
}

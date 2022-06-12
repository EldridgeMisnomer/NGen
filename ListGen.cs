using System;
using System.Collections.Generic;
using Utils;

using PU = NGen.ParserUtils;

namespace NGen {
    public class ListGen : Gen {

        /*
         *  A container for an array of words.
         *  It will return a random word from the array each time
         */

        //words
        public Gen[] wrds;

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

        public ListGen() { }

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

        protected override GenOutput[] PickTxt( params string[] tags ) {

            if( wrds.Length == 1 ) {
                return wrds[0].GetOutput( tags );
            } else {

                switch(gs.PickType) {

                    case PickType.random:

                        if( gs.AllowRepeats ) {

                            return Rand.RandFromArray( wrds ).GetOutput( tags );

                        } else {

                            return Rand.NonRepeatingRandFromArray( wrds, ref lastWrd ).GetOutput( tags );

                        }

                    case PickType.shuffle:
                        GenOutput[] output = wrds[nextWrd].GetOutput( tags );

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

                        output = wrds[nextWrd].GetOutput( tags );

                        nextWrd = ( nextWrd + 1 + gs.Skip ) % wrds.Length;
                        return output;

                    case PickType.weighted:

                        return wrds[Rand.RandomDoubleWeightedInt( gs.PickWeights )].GetOutput( tags );

                    default:

                        return Rand.RandFromArray( wrds ).GetOutput( tags );

                }
            }
        }

        public override GenOutput[] GetOutput( params string[] tags ) {

            if( gs.OutputChance == 1 || Rand.ChanceTest( gs.OutputChance ) ) {

                //Get Number of repeats, GetTxt that number of times and make GenOutputs from it, return the GenOutputs
                int repeats = GetRepeatNum();
                List<GenOutput> gens = new List<GenOutput>();
                for( int r = 0; r < repeats + 1; r++ ) {
                    GenOutput[] newGOs = PickTxt( tags );
                    foreach( GenOutput go in newGOs ) {
                        gens.Add( go );
                    }
                }
                return gens.ToArray();
            } else {
                return null;
            }
        }

    }
}

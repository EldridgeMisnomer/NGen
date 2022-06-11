using System.Collections.Generic;
using Utils;

namespace NGen {

    public abstract class Gen { 

        public abstract GenOutput[] GetOutput();

        protected abstract GenOutput[] PickTxt();

        //settings
        public GenSettings gs;  

        protected int GetRepeatNum() {
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


        public bool GetNoSepBefore() {
            return gs.NoSepBefore;
        }

        public bool GetNoSepAfter() {
            return gs.NoSepAfter;
        }
    }

    public abstract class OutputGen : Gen {

        public abstract string GetTxt(  out bool sepBefore, out bool sepAfter, params string[] tags );

        public string GetTxt( params string[] tags ) {
            return GetTxt( out _, out _, tags );
        }

        public string GetTxt() {
            return GetTxt( out _, out _ );
        }

        public override GenOutput[] GetOutput() {

            string os = GetTxt( out bool sepBefore, out bool sepAfter );
            GenOutput go = new GenOutput( os, gs ) {
                SepBefore = sepBefore,
                SepAfter = sepAfter
            };

            return new GenOutput[] { go };

        }

    }
}




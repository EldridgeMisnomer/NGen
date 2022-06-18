using System.Collections.Generic;
using Utils;

namespace NGen {

    public abstract class Gen { 

        public abstract GenOutput[] GetOutput( params string[] tags );

        protected abstract GenOutput[] PickTxt( params string[] tags  );

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

                    if( gs.RepUseMean ) {

                        if( gs.RepUseDev ) {

                            repeats = Rand.Norm.RangeMeanDevInt( gs.RepMin, gs.RepMax, gs.RepMean, gs.RepStdDev );
                            break;

                        } else {

                            repeats = Rand.Norm.RangeMeanInt( gs.RepMin, gs.RepMax, gs.RepMean );
                            break;

                        }


                    } else {

                        repeats = Rand.Norm.RangeInt( gs.RepMin, gs.RepMax );
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

        public override GenOutput[] GetOutput( params string[] tags ) {

            string os = GetTxt( out bool sepBefore, out bool sepAfter, tags );
            GenOutput go = new GenOutput( os, gs ) {
                SepBefore = sepBefore,
                SepAfter = sepAfter
            };

            return new GenOutput[] { go };

        }

    }
}




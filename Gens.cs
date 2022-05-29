using PU = NGen.ParserUtils;

namespace NGen {

    public abstract class Gen {

        public abstract string GetTxt();

    }

    public class Wrd : Gen {
        /*
         *  A container for just one word, a string which will always be returned
         *  To be used in complicated WrdGens with nesting
         */

        private readonly string wrd;

        public Wrd( string str ) {
            wrd = PU.StripEscapes(str);
        }

        public override string GetTxt() {
            return wrd;
        }
    }

    public class ProxyGen : Gen {
        /*
         * This is a Gen which stands as a proxy for another named Gen
         * We don't yet have a way to access these
         */

        private Gen gen;
        private readonly string genName;

        public ProxyGen( string name ) {
            genName = name;
        }

        public override string GetTxt() {

            if( gen == null ) {
                return $"**{genName}**";
            } else {
                return gen.GetTxt();
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
         *  It will never return the same words twice in a row 
         *      (unless there is only one word)
         */

        //words
        private readonly Gen[] wrds;

        //settings
        private readonly GenSettings gs;

        //state
        private int lastWrd = 0;
        private int nextWrd = -1;

        public ListGen( string[] words, GenSettings settings ) {

            //convert strings into Wrds and but them into their array
            wrds = new Gen[words.Length];
            for( int i = 0; i < wrds.Length; i++ ) {
                wrds[i] = new Wrd( PU.StripEscapes( words[i].Trim() ) );
            }

            gs = settings;
            Setup();
        }

        public ListGen( Gen[] words, GenSettings settings ) {

            wrds = words;

            gs = settings;
            Setup();
        }

        private void Setup() {
            if( gs.PickType == PickType.cycle ||
                gs.PickType == PickType.shuffle ){//||
                //pickType == PickType.noRepShuffle ) {

                nextWrd = 0;
            }

            if( gs.PickType == PickType.shuffle ) {
                wrds.Shuffle();
            }

/*            if( pickType == PickType.noRepShuffle ) {
                wrds.NonRepeatingShuffle();
            }*/
        }

        public override string GetTxt() {

            string s = "";
            int repeats = 0;

            //Choose the number of repeats based on the repeat type
            switch( gs.RepType ) {

                case RepeatType.constant:
                    repeats = gs.RepMax;
                    break;
    
                case RepeatType.uniform:
                    repeats = Utils.RandomRangeInt( gs.RepMin, gs.RepMax + 1 );
                    break;

                case RepeatType.normal:

                    if( gs.UseMeanDev ) {

                        repeats = Utils.RandomNormalMeanDevInt( gs.RepMin, gs.RepMax, gs.RepMean, gs.RepStdDev );
                        break;

                    } else {

                        repeats = Utils.RandomNormalRangeInt( gs.RepMin, gs.RepMax );
                        break;

                    }
                case RepeatType.weighted:
                    repeats = Utils.RandomWeightedInt( gs.RepWeights );
                    break;

            }

            //get text repeatedly
            for( int i = 0; i < repeats + 1; i++ ) {
                s += PickTxt();
                if( i != gs.RepMax ) {
                    s += gs.separator;
                }
            }

            return s;
        }

        public string PickTxt() {

            if( wrds.Length == 1 ) {
                return wrds[0].GetTxt();
            } else {

                switch(gs.PickType) {

                    case PickType.random:
                        return Utils.RandFromArray( wrds ).GetTxt();
                    //case PickType.noRepRandom:
                    //    return Utils.NonRepeatingRandFromArray( wrds, ref lastWrd ).GetTxt();
                    case PickType.shuffle:
                        string output = wrds[nextWrd].GetTxt();

                        nextWrd++;
                        if( nextWrd >= wrds.Length ) {
                            wrds.Shuffle();
                            nextWrd = 0;
                        }

                        return output;
                    //case PickType.noRepShuffle:
                    //    output = wrds[nextWrd].GetTxt();

                    //    nextWrd++;
                    //    if( nextWrd >= wrds.Length ) {
                    //        wrds.NonRepeatingShuffle();
                    //       nextWrd = 0;
                    //    }

                    //    return output;
                    case PickType.cycle:
                        output = wrds[nextWrd].GetTxt();

                        nextWrd = ( nextWrd + 1 ) % wrds.Length;
                        return output;
                    default:
                        return Utils.RandFromArray( wrds ).GetTxt();


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
        private string separator = " ";

        public SenGen( Gen[] gens) {
            wrds = gens;
        }

        public override string GetTxt() {

            string s = "";

            for( int i = 0; i < wrds.Length; i++ ) {
                s += wrds[i].GetTxt();
                if( i != wrds.Length - 1 ) {
                    s += separator;
                }
            }

            return s;

        }
    }
}

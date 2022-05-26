﻿using PU = NGen.ParserUtils;

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
        private readonly PickType pickType;

        //state
        private int lastWrd = 0;
        private int nextWrd = -1;

        public ListGen( string[] words, PickType type = PickType.random ) {

            //convert strings into Wrds and but them into their array
            wrds = new Gen[words.Length];
            for( int i = 0; i < wrds.Length; i++ ) {
                wrds[i] = new Wrd( PU.StripEscapes( words[i].Trim() ) );
            }

            pickType = type;
            Setup();
        }

        public ListGen( Gen[] words, PickType type = PickType.random ) {

            wrds = words;

            pickType = type;
            Setup();
        }

        private void Setup() {
            if( pickType == PickType.cycle ||
                pickType == PickType.shuffle ||
                pickType == PickType.noRepShuffle ) {

                nextWrd = 0;
            }

            if( pickType == PickType.shuffle ) {
                wrds.Shuffle();
            }

            if( pickType == PickType.noRepShuffle ) {
                wrds.NonRepeatingShuffle();
            }
        }

        public override string GetTxt() {

            if( wrds.Length == 1 ) {
                return wrds[0].GetTxt();
            } else {

                switch(pickType) {

                    case PickType.random:
                        return Utils.RandFromArray( wrds ).GetTxt();
                    case PickType.noRepRandom:
                        return Utils.NonRepeatingRandFromArray( wrds, ref lastWrd ).GetTxt();
                    case PickType.shuffle:
                        string output = wrds[nextWrd].GetTxt();

                        nextWrd++;
                        if( nextWrd >= wrds.Length ) {
                            wrds.Shuffle();
                            nextWrd = 0;
                        }

                        return output;
                    case PickType.noRepShuffle:
                        output = wrds[nextWrd].GetTxt();

                        nextWrd++;
                        if( nextWrd >= wrds.Length ) {
                            wrds.NonRepeatingShuffle();
                            nextWrd = 0;
                        }

                        return output;
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

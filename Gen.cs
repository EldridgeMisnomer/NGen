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
            wrd = str;
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
                //get the gen, store it, then return it
                return "";
            } else {
                return gen.GetTxt();
            }

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
        private readonly bool allowReps;

        //state
        private int lastWrd = 0;

        public ListGen( string[] words, bool allowRepeats = false ) {

            //convert strings into Wrds and but them into their array
            wrds = new Gen[words.Length];
            for( int i = 0; i < wrds.Length; i++ ) {
                wrds[i] = new Wrd( words[i].Trim() );
            }

            allowReps = allowRepeats;
        }

        public ListGen( Gen[] words, bool allowRepeats = false ) {

            wrds = words;

            allowReps = allowRepeats;
        }

        public override string GetTxt() {

            if( wrds.Length == 1 ) {
                return wrds[0].GetTxt();
            } else {

                if( allowReps ) {
                    return Utils.RandFromArray( wrds ).GetTxt();
                } else {
                    return Utils.NonRepeatingRandFromArray( wrds, ref lastWrd ).GetTxt();
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

        public SenGen() {
            //DEBUG - This is a placeholder, remove later
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

using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace NGen {
    public class TagGen : OutputGen {

        public List<SenGen> gens = new List<SenGen>();

        public TagGen() {
            gs = new GenSettings();
        }

        public void AddGen( SenGen sg ) {
            gens.Add( sg );
        }

        public override string GetTxt( out bool sepBefore, out bool sepAfter , params string[] tags ) {

            //if there are tags, find a gen to match to them
            if( tags != null && tags.Length > 0 ) {

                //trim and lowercaseify the tags just in case
                for( int i = 0; i < tags.Length; i++ ) {
                    tags[i] = tags[i].Trim().ToLower();
                }

                //get scores representing how many of the user tags gens fulfill
                int[] scores = new int[gens.Count];
                for( int i = 0; i < gens.Count; i++ ) {

                    scores[i] = TagWrangler.ScoreTags( tags, gens[i].ownTags );

                }

                int maxScore = scores.Max();

                //if there are no matching tags, just return a random one
                if( maxScore == 0 ) {

                    return GetRandomTxt( out sepBefore, out sepAfter, tags );

                } else {

                    //find all the gens with the highest score
                    int[] genIndexes = scores.AllIndexesOf( maxScore );

                    //pick one at random to output
                    int choice = Rand.RandomRangeInt( 0, genIndexes.Length );
                    return gens[choice].GetTxt( out sepBefore, out sepAfter, tags );

                }

            } else {

                return GetRandomTxt( out sepBefore, out sepAfter, tags );

            }
        }

        private string GetRandomTxt( out bool sepBefore, out bool sepAfter, params string[] tags ) {
            //If there are no tags given, then pick a random Gen to return
            int choice = Rand.RandomRangeInt( 0, gens.Count );

            //DEBUG
            //Console.WriteLine( $"TagGen GetTxt. num gens: '{gens.Count}', choice: '{choice}'" );
            return gens[choice].GetTxt( out sepBefore, out sepAfter, tags );
        }

        protected override GenOutput[] PickTxt( params string[] tags  ) {
            
            int choice = Rand.RandomRangeInt( 0, gens.Count );
            return gens[choice].GetOutput( tags );

        }
    }
}

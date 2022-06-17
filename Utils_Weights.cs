
namespace Utils {

    public static class Weights {

        public static double[] CalculateLinearWeightsFromMinMax( double min, double max, int numWeights, bool maxFirst = true ) {

            double[] weights = new double[numWeights];

            //if we only need 2 weights, just return the min and the max
            if( numWeights == 2 ) {

                if( maxFirst ) {
                    weights[0] = max;
                    weights[1] = min;
                } else {
                    weights[0] = min;
                    weights[1] = max;
                }

            } else {

                //if there's no difference between min and max
                //make all weights the same
                double dif = max - min;

                if( dif == 0 ) {

                    for( int i = 0; i < weights.Length; i++ ) {
                        weights[i] = min;
                    }

                } else {

                    //otherwise, calculate new weights
                    double spacing = dif / (double)( numWeights - 1 );

                    double weight;
                    if( maxFirst ) {
                        weight = max;
                    } else {
                        weight = min;
                    }

                    for( int i = 0; i < weights.Length; i++ ) {
                        weights[i] = weight;
                        if( maxFirst ) {
                            weight -= spacing;
                        } else {
                            weight += spacing;
                        }
                    }

                }
            }

            return weights;
        }

        public static double[] CalculateWeightsFromMult( double mult, int numWeights ) {

            double weight;

            if( mult > 0 ) {
                weight = 1;
            } else {
                weight = 1000;
            }

            double[] weights = new double[numWeights];

            for( int i = 0; i < weights.Length; i++ ) {

                weights[i] = weight;
                weight *= mult;

            }

            return weights;

        }
    }
}

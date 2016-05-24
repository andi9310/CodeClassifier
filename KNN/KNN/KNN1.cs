using System.ComponentModel.Composition;
using CodeClassifier.Classifiers;

namespace KNN.KNN
{
    [Export(typeof(IClassifier))]
    internal class Knn1 : KNearestNeighboursClassifier
    {
        public Knn1() : base(1)
        {
        }
    }
}
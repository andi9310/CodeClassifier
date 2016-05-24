using System.ComponentModel.Composition;
using CodeClassifier.Classifiers;

namespace KNN.KNN
{
    [Export(typeof(IClassifier))]
    internal class Knn4 : KNearestNeighboursClassifier
    {
        public Knn4() : base(4)
        {
        }
    }
}
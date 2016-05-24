using System.ComponentModel.Composition;
using CodeClassifier.Classifiers;

namespace KNN.KNN
{
    [Export(typeof(IClassifier))]
    internal class Knn8 : KNearestNeighboursClassifier
    {
        public Knn8() : base(8)
        {
        }
    }
}
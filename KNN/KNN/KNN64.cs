using System.ComponentModel.Composition;
using CodeClassifier.Classifiers;

namespace KNN.KNN
{
    [Export(typeof(IClassifier))]
    internal class Knn64 : KNearestNeighboursClassifier
    {
        public Knn64() : base(64)
        { }
    }
}

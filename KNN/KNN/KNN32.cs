using System.ComponentModel.Composition;
using CodeClassifier.Classifiers;

namespace KNN.KNN
{
    [Export(typeof(IClassifier))]
    internal class Knn32 : KNearestNeighboursClassifier
    {
        public Knn32() : base(32)
        { }
    }
}

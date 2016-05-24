using System.ComponentModel.Composition;
using CodeClassifier.Classifiers;

namespace KNN.KNN
{
    [Export(typeof(IClassifier))]
    internal class Knn16 : KNearestNeighboursClassifier
    {
        public Knn16() : base(16)
        { }
    }
}

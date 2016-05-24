using System.ComponentModel.Composition;
using CodeClassifier.Classifiers;

namespace KNN.KNN
{
    [Export(typeof(IClassifier))]
    internal class Knn2 : KNearestNeighboursClassifier
    {
        public Knn2() : base(2)
        { }
    }
}

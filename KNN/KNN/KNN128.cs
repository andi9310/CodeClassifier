using System.ComponentModel.Composition;
using CodeClassifier.Classifiers;

namespace KNN.KNN
{
    [Export(typeof(IClassifier))]
    internal class Knn128 : KNearestNeighboursClassifier
    {
        public Knn128() : base(128)
        { }
    }
}

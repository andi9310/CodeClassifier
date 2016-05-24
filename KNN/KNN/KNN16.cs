using System.ComponentModel.Composition;

namespace CodeClassifier.Classifiers.KNN
{
    [Export(typeof(IClassifier))]
    class KNN16 : KNearestNeighboursClassifier
    {
        public KNN16() : base(16)
        { }
    }
}

using System.ComponentModel.Composition;

namespace CodeClassifier.Classifiers.KNN
{
    [Export(typeof(IClassifier))]
    class KNN64 : KNearestNeighboursClassifier
    {
        public KNN64() : base(64)
        { }
    }
}

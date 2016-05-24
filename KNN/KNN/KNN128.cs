using System.ComponentModel.Composition;

namespace CodeClassifier.Classifiers.KNN
{
    [Export(typeof(IClassifier))]
    class KNN128 : KNearestNeighboursClassifier
    {
        public KNN128() : base(128)
        { }
    }
}

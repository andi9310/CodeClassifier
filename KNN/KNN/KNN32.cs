using System.ComponentModel.Composition;

namespace CodeClassifier.Classifiers.KNN
{
    [Export(typeof(IClassifier))]
    class KNN32 : KNearestNeighboursClassifier
    {
        public KNN32() : base(32)
        { }
    }
}

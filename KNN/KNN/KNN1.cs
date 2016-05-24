using System.ComponentModel.Composition;

namespace CodeClassifier.Classifiers.KNN
{
    [Export(typeof(IClassifier))]
    class KNN1 : KNearestNeighboursClassifier
    {
        public KNN1() : base(1)
        { }
    }
}

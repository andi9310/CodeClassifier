using System.ComponentModel.Composition;

namespace CodeClassifier.Classifiers.KNN
{
    [Export(typeof(IClassifier))]
    class KNN2 : KNearestNeighboursClassifier
    {
        public KNN2() : base(2)
        { }
    }
}

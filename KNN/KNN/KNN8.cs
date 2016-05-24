using System.ComponentModel.Composition;

namespace CodeClassifier.Classifiers.KNN
{
    [Export(typeof(IClassifier))]
    class KNN8 : KNearestNeighboursClassifier
    {
        public KNN8(): base(8)
        { }
    }
}

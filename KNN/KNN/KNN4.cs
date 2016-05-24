using System.ComponentModel.Composition;

namespace CodeClassifier.Classifiers.KNN
{
    [Export(typeof(IClassifier))]
    class KNN4 : KNearestNeighboursClassifier
    {
        public KNN4() : base(4)
        { }
    }
}

#region

using System.Collections.Generic;

#endregion

namespace CodeClassifier.Classifiers
{
    public interface IClassifier
    {
        void Teach(KeyValuePair<string, Dictionary<string, KeyValuePair<int, int>>> learningData);
        string Classify(Dictionary<string, KeyValuePair<int, int>> classificationData);
    }
}
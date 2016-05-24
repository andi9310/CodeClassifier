#region

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CodeClassifier.Classifiers;

#endregion

namespace Bayes.Bayess
{
    [Export(typeof(IClassifier))]
    public class BayesianClassifier : IClassifier
    {
        private const double Neutral = 0.5;

        private readonly List<KeyValuePair<string, Dictionary<string, KeyValuePair<int, int>>>> _learningSet =
            new List<KeyValuePair<string, Dictionary<string, KeyValuePair<int, int>>>>();

        private Bayess _classifier;
        private bool _isInitialized;

        public void Teach(KeyValuePair<string, Dictionary<string, KeyValuePair<int, int>>> learningData)
        {
            _learningSet.Add(learningData);
            _isInitialized = false;
        }

        public string Classify(Dictionary<string, KeyValuePair<int, int>> classificationData)
        {
            if (!_isInitialized)
            {
                var dataBase = new DataBase("programmerID", _learningSet.First().Value.Keys);

                foreach (var keyValuePair in _learningSet)
                {
                    dataBase.Add(keyValuePair.Key,
                        keyValuePair.Value.Select(
                            valuePair =>
                                valuePair.Value.Key == 0 && valuePair.Value.Value == 0
                                    ? Neutral
                                    : (double)valuePair.Value.Key / (valuePair.Value.Key + valuePair.Value.Value)));
                }
                _classifier = new Bayess(dataBase);
                _isInitialized = true;
            }
            return
                _classifier.Classify(
                    classificationData.Select(
                        valuePair =>
                            valuePair.Value.Key == 0 && valuePair.Value.Value == 0
                                ? Neutral
                                : (double)valuePair.Value.Key / (valuePair.Value.Key + valuePair.Value.Value)).ToArray());
        }

        public override string ToString()
        {
            return "Bayes";
        }
    }
}
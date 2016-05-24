#region

using System;
using System.Collections.Generic;
using System.Linq;
using CodeClassifier.Classifiers;

#endregion

namespace KNN.KNN
{
    public class KNearestNeighboursClassifier : IClassifier
    {
        private const double Neutral = 0.5;

        private readonly List<KeyValuePair<string, List<double>>> _teachingList =
            new List<KeyValuePair<string, List<double>>>();

        public KNearestNeighboursClassifier(int k)
        {
            K = k;
        }

        public int K { get; set; }

        public void Teach(KeyValuePair<string, Dictionary<string, KeyValuePair<int, int>>> learningData)
        {
            var list =
                learningData.Value.Select(
                    val =>
                        Double.IsNaN((double) val.Value.Key/(val.Value.Key + val.Value.Value))
                            ? Neutral
                            : (double) val.Value.Key/(val.Value.Key + val.Value.Value)).ToList();
            _teachingList.Add(new KeyValuePair<string, List<double>>(learningData.Key, list));
        }

        public string Classify(Dictionary<string, KeyValuePair<int, int>> classificationData)
        {
          //  var list = classificationData.ToList();

            var kNearest = (from keyValuePair in _teachingList.AsParallel()
                let dist =
                    classificationData.Select(
                        (t, i) =>
                            Math.Pow(
                                Double.IsNaN((double) (t.Value.Key)/(t.Value.Value + t.Value.Key))
                                    ? Neutral
                                    : (double) (t.Value.Key)/(t.Value.Value + t.Value.Key) - keyValuePair.Value[i], 2))
                        .Sum()
                select new KeyValuePair<string, double>(keyValuePair.Key, dist)).OrderBy(p => p.Value)
                .Take(K)
                .Select(p => p.Key);


            var dict = new Dictionary<string, int>();
            foreach (var s in kNearest)
            {
                try
                {
                    dict[s]++;
                }
                catch (KeyNotFoundException)
                {
                    dict.Add(s, 1);
                }
            }
            var dictNormal = dict.ToDictionary(i => i.Key,
                i => (double) i.Value/_teachingList.Count(p => p.Key == i.Key));

            return dictNormal.OrderByDescending(p => p.Value).First().Key;
        }

        public override string ToString()
        {
            return "KNN " + K;
        }
    }
}
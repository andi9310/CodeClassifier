#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeClassifier;

#endregion

namespace Bayes.Bayess
{
    public class Bayess
    {
        private const double Tolerance = 1E-12;

        public Bayess(DataBase db)
        {
            TrainClassifier(db.Data);
        }

        public DataSet DataSet { get; set; } = new DataSet();

        private void TrainClassifier(DataTable table)
        {
            DataSet.Tables.Add(table);

            //table
            var gaussianDistribution = DataSet.Tables.Add("Gaussian");
            gaussianDistribution.Columns.Add(table.Columns[0].ColumnName);

            //columns
            for (var i = 1; i < table.Columns.Count; i++)
            {
                gaussianDistribution.Columns.Add(table.Columns[i].ColumnName + "Mean");
                gaussianDistribution.Columns.Add(table.Columns[i].ColumnName + "Variance");
            }

            //calc data
            var results = (from myRow in table.AsEnumerable()
                group myRow by myRow.Field<string>(table.Columns[0].ColumnName)
                into g
                select new {Name = g.Key, Count = g.Count()}).ToList();

            foreach (var t in results)
            {
                var row = gaussianDistribution.Rows.Add();
                row[0] = t.Name;

                var a = 1;
                for (var i = 1; i < table.Columns.Count; i++)
                {
                    row[a] =
                        SelectRows(table, i, $"{table.Columns[0].ColumnName} = '{t.Name}'").Mean();
                    row[++a] =
                        SelectRows(table, i, $"{table.Columns[0].ColumnName} = '{t.Name}'")
                            .Variance();
                    a++;
                }
            }
        }

        public string Classify(double[] obj)
        {
            var score = new Dictionary<string, double>();

            var results = (from myRow in DataSet.Tables[0].AsEnumerable()
                group myRow by myRow.Field<string>(DataSet.Tables[0].Columns[0].ColumnName)
                into g
                select new {Name = g.Key, Count = g.Count()}).ToList();

            for (var i = 0; i < results.Count; i++)
            {
                var subScoreList = new List<double>();
                int a = 1, b = 1;
                for (var k = 1; k < DataSet.Tables["Gaussian"].Columns.Count; k = k + 2)
                {
                    var mean = Convert.ToDouble(DataSet.Tables["Gaussian"].Rows[i][a]);
                    var variance = Convert.ToDouble(DataSet.Tables["Gaussian"].Rows[i][++a]);
                    var result = Helper.NormalDist(obj[b - 1], mean, Helper.SquareRoot(variance));
                    subScoreList.Add(result);
                    a++;
                    b++;
                }

                double finalScore = 0;
                foreach (var t in subScoreList)
                {
                    if (Math.Abs(finalScore) < Tolerance)
                    {
                        finalScore = t;
                        continue;
                    }

                    finalScore = finalScore*t;
                }

                score.Add(results[i].Name, finalScore*0.5);
            }

            var maxOne = score.Max(c => c.Value);
            var name = (from c in score
                where Math.Abs(c.Value - maxOne) < Tolerance
                select c.Key).First();

            return name;
        }

        #region Helper Function

        private static IEnumerable<double> SelectRows(DataTable table, int column, string filter)
        {
            var rows = table.Select(filter);

            return rows.Select(t => (double) t[column]).ToList();
        }

        public void Clear()
        {
            DataSet = new DataSet();
        }

        #endregion
    }
}
#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

#endregion

namespace Bayes.Bayess
{
    [Serializable]
    public class DataBase
    {
        public DataBase(string className, IEnumerable<string> prop)
        {
            Data = new DataTable();
            Data.Columns.Add(className);
            foreach (var s in prop)
            {
                Data.Columns.Add(s, typeof(double));
            }
        }

        public DataTable Data { get; }

        public void Add(string programmerId, IEnumerable<double> parameters)
        {
            var ar = parameters.ToArray();
            var row = Data.NewRow();
            row[Data.Columns[0].ColumnName] = programmerId;
            for (var i = 1; i < Data.Columns.Count; i++)
            {
                row[Data.Columns[i].ColumnName] = ar[i - 1];
            }
            Data.Rows.Add(row);
        }
    }
}
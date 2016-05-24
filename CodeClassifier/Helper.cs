#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace CodeClassifier
{
    public static class Helper
    {
        public static double Variance(this IEnumerable<double> source)
        {
            var doubles = source as IList<double> ?? source.ToList();
            var avg = doubles.Average();
            var d = doubles.Aggregate(0.0, (total, next) => total + Math.Pow(next - avg, 2));
            return d/(doubles.Count - 1);
        }

        public static double Mean(this IEnumerable<double> source)
        {
            var doubles = source as IList<double> ?? source.ToList();
            if (!doubles.Any())
                return 0.0;

            double length = doubles.Count;
            var sum = doubles.Sum();
            return sum/length;
        }

        public static double NormalDist(double x, double mean, double standardDev)
        {
            var fact = standardDev*Math.Sqrt(2.0*Math.PI);
            var expo = (x - mean)*(x - mean)/(2.0*standardDev*standardDev);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return standardDev == 0.0 ? 1.0 : Math.Exp(-expo)/fact;
        }

        public static double Normdist(double x, double mean, double standardDev, bool cumulative)
        {
            const double parts = 50000.0; //large enough to make the trapzoids small enough

            const double lowBound = 0.0;
            if (!cumulative) return NormalDist(x, mean, standardDev);
            var width = (x - lowBound)/(parts - 1.0);
            var integral = 0.0;
            for (var i = 1; i < parts - 1; i++)
            {
                integral += 0.5*width*(NormalDist(lowBound + width*i, mean, standardDev) +
                                       NormalDist(lowBound + width*(i + 1), mean, standardDev));
            }
            return integral;
        }

        public static double SquareRoot(double source)
        {
            return Math.Sqrt(source);
        }
    }
}
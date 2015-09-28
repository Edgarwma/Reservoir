using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReservoirComputing
{
    public static class Extensions
    {
        public static double Compare(this double[] a, double[] b)
        {
            double Error = 0;
            if (a.Length == b.Length)
            {
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i] != Math.Round(b[i]))
                    {
                        Error = 1;
                        break;
                    }
                }
            }
            else
                throw new ArgumentOutOfRangeException("O tamanho dos vetores deve ser o mesmo");

            return Error;
        }
        public static double ConvertToDouble(this double[] val)
        {
            int index = 0;
            for (index = 0; index < val.Length; index++)
                if (val[index] == 1)
                    break;

            return (index + 1);
        }
        public static double[] ConvertToArray(this double val, int max)
        {
            double[] conversion = new double[max];
            conversion[(int)(val - 1)] = 1;

            return conversion;
        }
        public static string ToSeconds(this TimeSpan span)
        {
            string formatted = span.TotalSeconds + " seg";
            return formatted;
        }

        public static string ToReadableString(this TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}{4}",
                span.Days > 0 ? string.Format("{0:0} ds, ", span.Days) : string.Empty,
                span.Hours > 0 ? string.Format(" {0:0} h, ", span.Hours) : string.Empty,
                span.Minutes > 0 ? string.Format(" {0:0} min, ", span.Minutes) : string.Empty,
                span.Seconds > 0 ? string.Format(" {0:0} seg", span.Seconds) : string.Empty,
                span.Milliseconds > 0 ? string.Format(" {0:0} mil", span.Milliseconds) : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            return formatted;
        }

        public static void Shuffle<T>(this IList<T> list, Random rand)
        {
            Random rng = rand;
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static T[][] InitializeMatrix<T>(this T[][] matrix, int nLines, int nColumns)
        {
            matrix = new T[nLines][];
            for (int i = 0; i < nLines; i++)
            {
                matrix[i] = new T[nColumns];
            }
            return matrix;
        }

        public static double InnerProduct(this double[] a, double[] b)
        {
            double innerProduct = 0;
            if (a.Length == b.Length)
            {
                for (int i = 0; i < a.Length; i++)
                    innerProduct += a[i] * b[i];
            }
            else
                throw new ArgumentOutOfRangeException("O tamanho dos vetores deve ser o mesmo");

            return innerProduct;
        }
    }
}

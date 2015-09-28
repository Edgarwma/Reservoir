using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataManagement
{
    public static class Extensions
    {
        internal static void Shuffle<T>(this IList<T> list, Random rand)
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
        public static double[] ConvertToArray(this double val, int max)
        {
            double[] conversion = new double[max];
            conversion[(int)(val - 1)] = 1;

            return conversion;
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonnySearch.Particle;

namespace HarmonnySearch
{
    public static class Extensions
    {
        public static void SortDescending<T>(this List<HSParticle<T>> list)
        {
            list.Sort(delegate(HSParticle<T> p1, HSParticle<T> p2)
            {
                return p1.Fitness.CompareTo(p2.Fitness);
            });
            list.Reverse();
        }

        public static void SortAscending<T>(this List<HSParticle<T>> list)
        {
            list.Sort(delegate(HSParticle<T> p1, HSParticle<T> p2)
            {
                return p1.Fitness.CompareTo(p2.Fitness);
            });
        }
    }
}

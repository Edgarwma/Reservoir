using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HarmonnySearch.Configuration
{
    public abstract class HSConfiguration<T>
    {
        public HSConfiguration(T[] values, int seed)
        {
            Values = values;
            Seed = seed;
        }

        public int Seed { get; private set; }
        public T[] Values { get; private set; }
    }
}
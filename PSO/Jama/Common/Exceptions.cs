using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jama.Common
{
    public class IllegalArgumentException : Exception
    {
        public IllegalArgumentException(string message)
            : base(message)
        { }
    }

    public class ArrayIndexOutOfBoundsException : Exception
    {
        public ArrayIndexOutOfBoundsException(string message)
            : base(message)
        { }
    }

    public class RuntimeException : Exception
    {
        public RuntimeException(string message)
            : base(message)
        { }
    }
}

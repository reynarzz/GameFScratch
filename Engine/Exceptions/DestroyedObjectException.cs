using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Exceptions
{
    internal class DestroyedObjectException : Exception
    {
        public DestroyedObjectException() { }

        public DestroyedObjectException(string message)
            : base(message)
        {

        }

        public DestroyedObjectException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

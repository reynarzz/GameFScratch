using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Utils
{
    internal abstract class ObjectPool<T>
    {
        protected List<T> Elements { get; set; }

        public ObjectPool()
        {
            Elements = new List<T>();
        }

        public abstract T Get();
    }
}

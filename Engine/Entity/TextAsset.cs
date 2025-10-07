using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class TextAsset : EObject
    {
        public string Text { get; }

        public TextAsset(string text, string path, Guid guid) : base(path, guid)
        {
            Text = text;
        }
    }
}

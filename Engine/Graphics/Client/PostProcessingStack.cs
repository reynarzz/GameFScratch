using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    public class PostProcessingStack
    {
        private static List<PostProcessingPass> _passes = new();
        public static IReadOnlyCollection<PostProcessingPass> Passes => _passes;

        public static void Push(PostProcessingPass pass)
        {
            _passes.Add(pass);
        }

        public static void Pop(PostProcessingPass pass)
        {
            _passes.Remove(pass);
        }
    }
}

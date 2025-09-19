using System.Reflection;
using Engine;

namespace Sandbox
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var assembly = Assembly.LoadFrom($"{nameof(Game)}.dll");
            var allClasses = assembly.GetTypes()
                                     .Where(t => t.IsClass)
                                     .ToList();

            foreach (var t in allClasses)
            {
                Console.WriteLine(t.Name);
            }

            new Engine.Engine().Init();
        }
    }
}

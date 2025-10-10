using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Layers
{
    internal class MainThreadDispatcher : LayerBase
    {
        private static readonly Queue<Func<Task>> _queue = new();

        public override void Close()
        {

        }

        public override void Initialize()
        {

        }

        internal override void UpdateLayer()
        {
            while (_queue.TryDequeue(out var action))
            {
                action();
            }
        }

        public static Task EnqueueAsync(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            _queue.Enqueue(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
                return Task.CompletedTask;
            });
            return tcs.Task;
        }
    }
}

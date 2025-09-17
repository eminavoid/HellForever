using System.Collections;
using UnityEngine;

namespace Unity.FPS.Ours
{
    [DisallowMultipleComponent]
    public class EntityQueueRunner : MonoBehaviour
    {
        readonly GenericLinkedQueue<IQueueTask> _queue = new GenericLinkedQueue<IQueueTask>();
        bool _running;

        public void Enqueue(IQueueTask task)
        {
            _queue.Enqueue(task);
            if (!_running) StartCoroutine(RunLoop());
        }

        IEnumerator RunLoop()
        {
            _running = true;
            while (_queue.TryDequeue(out var task))
                yield return task.Run(gameObject);
            _running = false;
        }

        public void ClearPending() => _queue.Clear();
    }
}

using System.Collections;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Ours
{
    [DisallowMultipleComponent]
    public class PowerupStackRunner : MonoBehaviour
    {
        private readonly GenericLinkedStack<IQueueTask> _stack = new GenericLinkedStack<IQueueTask>();

        private IQueueTask _current;
        private Coroutine _loop;
        private bool _running;

        public void Push(IQueueTask task)
        {
            // apilar la actual si estaba corriendo
            if (_running && _current != null)
            {
                if (_current is IPausableTask p) p.Pause();
                if (_loop != null) StopCoroutine(_loop);
                _stack.Push(_current);
                _current = null;
                _running = false;
            }

            _stack.Push(task);

            if (!_running)
                _loop = StartCoroutine(RunLoop());
        }

        private IEnumerator RunLoop()
        {
            _running = true;

            while (_stack.TryPop(out var next))
            {
                _current = next;
                yield return next.Run(gameObject);
                _current = null;
            }

            _running = false;
            _loop = null;
        }

        public void ClearAll()
        {
            _stack.Clear();
            if (_current is IPausableTask p) p.Pause();
            if (_loop != null) StopCoroutine(_loop);
            _current = null;
            _loop = null;
            _running = false;
        }
    }
}

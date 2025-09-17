using System.Collections;
using Unity.FPS.Game;
using Unity.FPS.Ours;
using UnityEngine;

namespace Unity.FPS.Ours
{
    [DisallowMultipleComponent]
    public class PowerupStackRunner : MonoBehaviour
    {
        readonly GenericLinkedStack<IQueueTask> _stack = new GenericLinkedStack<IQueueTask>();

        IQueueTask _currentTask;
        Coroutine _currentRoutine;
        bool _running;

        public void Push(IQueueTask task)
        {
            // Pause if running
            if (_running && _currentTask != null)
            {
                if (_currentTask is IPausableTask pausable)
                    pausable.Pause();

                if (_currentRoutine != null)
                {
                    StopCoroutine(_currentRoutine);
                    _currentRoutine = null;
                }

                _stack.Push(_currentTask);
                _currentTask = null;
                _running = false;
            }

            // New task 
            _stack.Push(task);

            // (Re)start loop
            if (!_running)
                _currentRoutine = StartCoroutine(RunLoop());
        }

        IEnumerator RunLoop()
        {
            _running = true;

            while (_stack.TryPop(out var next))
            {
                _currentTask = next;
                yield return next.Run(gameObject);
                _currentTask = null;
            }

            _running = false;
            _currentRoutine = null;
        }

        public void ClearPending()
        {
            _stack.Clear();
            if (_currentTask is IPausableTask pausable)
                pausable.Pause();
            if (_currentRoutine != null) StopCoroutine(_currentRoutine);
            _currentTask = null;
            _currentRoutine = null;
            _running = false;
        }
    }

}

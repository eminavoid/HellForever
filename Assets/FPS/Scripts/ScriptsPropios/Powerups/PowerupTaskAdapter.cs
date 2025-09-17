using System.Collections;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Ours
{
    public class PowerupTaskAdapter : IQueueTask, IPausableTask
    {
        readonly PowerupBase _powerup;

        float _remaining;     // time left
        bool _applied;       // is effect currently applied?
        bool _paused;        // set by Pause()

        public PowerupTaskAdapter(PowerupBase powerup)
        {
            _powerup = Object.Instantiate(powerup);
            _remaining = _powerup.Duration;
        }

        public IEnumerator Run(GameObject target)
        {
            _paused = false;

            if (!_applied)
            {
                _powerup.Init(target);
                _powerup.Apply();
                _applied = true;
            }

            while (_remaining > 0f && !_paused)
            {
                _remaining -= Time.deltaTime;
                yield return null;
            }

            if (_paused) yield break;

            if (_applied)
            {
                _powerup.Revert();
                _applied = false;
            }
        }

        public void Pause()
        {
            if (_applied)
            {
                // stop affecting gameplay while paused
                _powerup.Revert();
                _applied = false;
            }
            _paused = true;
        }
    }

}

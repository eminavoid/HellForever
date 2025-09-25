using System.Collections;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Ours
{
    /// Adaptador para que un PowerupBase se ejecute como tarea con duración
    public class PowerupTaskAdapter : IQueueTask, IPausableTask
    {
        private readonly PowerupBase _powerup;
        private readonly PlayerGameplayModifiers _target;

        private float _remaining;
        private bool _applied;
        private bool _paused;

        public PowerupTaskAdapter(PowerupBase powerup, PlayerGameplayModifiers target)
        {
            // Instanciamos el SO para que sea propio de este jugador/toma
            _powerup = Object.Instantiate(powerup);
            _target = target;
            _remaining = _powerup.Duration;
        }

        public IEnumerator Run(GameObject owner)
        {
            _paused = false;

            if (!_applied)
            {
                _powerup.Apply(_target);
                _applied = true;
                Debug.Log($"[PowerupTaskAdapter] Apply => {_powerup.name} a {owner.name}");
            }

            while (_remaining > 0f && !_paused)
            {
                _remaining -= Time.deltaTime;
                yield return null;
            }

            if (_paused) yield break;

            if (_applied)
            {
                _powerup.Remove(_target);
                _applied = false;
                Debug.Log($"[PowerupTaskAdapter] Remove => {_powerup.name} de {owner.name}");
            }
        }

        public void Pause()
        {
            if (_applied)
            {
                _powerup.Remove(_target);
                _applied = false;
            }
            _paused = true;
        }
    }
}

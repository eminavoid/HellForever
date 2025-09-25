using UnityEngine;

namespace Unity.FPS.Ours
{
    public abstract class PowerupBase : ScriptableObject
    {
        [Tooltip("Duración en segundos del powerup")]
        public float Duration = 8f;

        /// Aplica el efecto sobre el jugador (target = sus modificadores)
        public abstract void Apply(PlayerGameplayModifiers target);

        /// Revierte el mismo efecto aplicado
        public abstract void Remove(PlayerGameplayModifiers target);
    }
}

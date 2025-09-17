using UnityEngine;

namespace Unity.FPS.Ours
{
    [CreateAssetMenu(menuName = "Powerups/Damage Reduction")]
    public class DamageReductionPowerup : PowerupBase
    {
        [Range(0.1f, 1f)] public float DamageTakenMultiplierValue = 0.5f; // 50% damage

        float _prev;
        public override void Apply()
        {
            _prev = GameplayModifiers.I != null ? GameplayModifiers.I.DamageTakenMultiplier : 1f;
            if (GameplayModifiers.I) GameplayModifiers.I.DamageTakenMultiplier = DamageTakenMultiplierValue;
        }

        public override void Revert()
        {
            if (GameplayModifiers.I) GameplayModifiers.I.DamageTakenMultiplier = _prev;
        }
    }
}

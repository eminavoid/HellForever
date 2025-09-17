using UnityEngine;

namespace Unity.FPS.Ours
{
    [CreateAssetMenu(menuName = "Powerups/Attack Speed")]
    public class AttackSpeedPowerup : PowerupBase
    {
        [Range(1f, 5f)] public float AttackSpeedMultiplierValue = 1.5f;

        float _prev;
        public override void Apply()
        {
            _prev = GameplayModifiers.I != null ? GameplayModifiers.I.AttackSpeedMultiplier : 1f;
            if (GameplayModifiers.I) GameplayModifiers.I.AttackSpeedMultiplier = AttackSpeedMultiplierValue;
        }

        public override void Revert()
        {
            if (GameplayModifiers.I) GameplayModifiers.I.AttackSpeedMultiplier = _prev;
        }
    }
}

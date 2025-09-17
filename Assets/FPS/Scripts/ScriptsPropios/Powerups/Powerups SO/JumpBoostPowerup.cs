using UnityEngine;

namespace Unity.FPS.Ours
{
    [CreateAssetMenu(menuName = "Powerups/Jump Boost")]
    public class JumpBoostPowerup : PowerupBase
    {
        [Range(1f, 3f)] public float JumpHeightMultiplierValue = 1.5f;

        float _prev;
        public override void Apply()
        {
            _prev = GameplayModifiers.I != null ? GameplayModifiers.I.JumpHeightMultiplier : 1f;
            if (GameplayModifiers.I) GameplayModifiers.I.JumpHeightMultiplier = JumpHeightMultiplierValue;
        }

        public override void Revert()
        {
            if (GameplayModifiers.I) GameplayModifiers.I.JumpHeightMultiplier = _prev;
        }
    }
}

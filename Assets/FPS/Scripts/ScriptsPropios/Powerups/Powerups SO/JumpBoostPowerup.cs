using UnityEngine;

namespace Unity.FPS.Ours
{
    [CreateAssetMenu(menuName = "Powerups/Jump Boost Powerup")]
    public class JumpBoostPowerup : PowerupBase
    {
        public float JumpMultiplier = 1.5f;

        public override void Apply(PlayerGameplayModifiers target)
        {
            if (target == null) return;
            target.JumpHeightMultiplier *= JumpMultiplier;
        }

        public override void Remove(PlayerGameplayModifiers target)
        {
            if (target == null) return;
            target.JumpHeightMultiplier /= JumpMultiplier;
        }
    }
}

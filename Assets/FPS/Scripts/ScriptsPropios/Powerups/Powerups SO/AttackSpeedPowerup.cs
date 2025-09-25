using UnityEngine;

namespace Unity.FPS.Ours
{
    [CreateAssetMenu(menuName = "Powerups/Attack Speed Powerup")]
    public class AttackSpeedPowerup : PowerupBase
    {
        public float SpeedMultiplier = 1.5f;

        public override void Apply(PlayerGameplayModifiers target)
        {
            if (target == null) return;
            target.AttackSpeedMultiplier *= SpeedMultiplier;
        }

        public override void Remove(PlayerGameplayModifiers target)
        {
            if (target == null) return;
            target.AttackSpeedMultiplier /= SpeedMultiplier;
        }
    }
}

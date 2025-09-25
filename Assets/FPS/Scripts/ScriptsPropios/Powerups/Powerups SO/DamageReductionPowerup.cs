using UnityEngine;

namespace Unity.FPS.Ours
{
    [CreateAssetMenu(menuName = "Powerups/Damage Reduction Powerup")]
    public class DamageReductionPowerup : PowerupBase
    {
        [Range(0.1f, 1f)] public float ReductionMultiplier = 0.5f;

        public override void Apply(PlayerGameplayModifiers target)
        {
            if (target == null) return;
            target.DamageTakenMultiplier *= ReductionMultiplier;
        }

        public override void Remove(PlayerGameplayModifiers target)
        {
            if (target == null) return;
            target.DamageTakenMultiplier /= ReductionMultiplier;
        }
    }
}

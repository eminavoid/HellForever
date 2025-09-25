using UnityEngine;

namespace Unity.FPS.Ours
{
    [CreateAssetMenu(menuName = "Powerups/Weapon Damage Powerup")]
    public class WeaponDamagePowerup : PowerupBase
    {
        public float DamageMultiplier = 1.5f;

        public override void Apply(PlayerGameplayModifiers target)
        {
            if (target == null) return;
            target.WeaponDamageMultiplier *= DamageMultiplier;
        }

        public override void Remove(PlayerGameplayModifiers target)
        {
            if (target == null) return;
            target.WeaponDamageMultiplier /= DamageMultiplier;
        }
    }
}

using UnityEngine;

namespace Unity.FPS.Ours
{
    [CreateAssetMenu(menuName = "Powerups/Weapon Damage")]
    public class WeaponDamagePowerup : PowerupBase
    {
        [Range(1f, 5f)] public float WeaponDamageMultiplierValue = 2f;

        float _prev;
        public override void Apply()
        {
            _prev = GameplayModifiers.I != null ? GameplayModifiers.I.WeaponDamageMultiplier : 1f;
            if (GameplayModifiers.I) GameplayModifiers.I.WeaponDamageMultiplier = WeaponDamageMultiplierValue;
        }

        public override void Revert()
        {
            if (GameplayModifiers.I) GameplayModifiers.I.WeaponDamageMultiplier = _prev;
        }
    }
}

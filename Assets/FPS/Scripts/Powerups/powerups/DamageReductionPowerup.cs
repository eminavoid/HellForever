using UnityEngine;

[CreateAssetMenu(menuName = "Powerups/Damage Reduction")]
public class DamageReductionPowerup : PowerupBase
{
    [Range(0.1f, 1f)] public float DamageTakenMultiplierValue = 0.5f; // 50% daño

    float _prev;
    public override void Apply()
    {
        _prev = GameplayModifiers.I.DamageTakenMultiplier;
        GameplayModifiers.I.DamageTakenMultiplier = DamageTakenMultiplierValue;
    }

    public override void Revert()
    {
        GameplayModifiers.I.DamageTakenMultiplier = _prev;
    }
}
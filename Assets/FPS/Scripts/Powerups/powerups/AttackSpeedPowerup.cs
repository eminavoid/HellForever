using UnityEngine;

[CreateAssetMenu(menuName = "Powerups/Attack Speed")]
public class AttackSpeedPowerup : PowerupBase
{
    [Range(1f, 5f)] public float AttackSpeedMultiplierValue = 1.5f;

    float _prev;
    public override void Apply()
    {
        _prev = GameplayModifiers.I.AttackSpeedMultiplier;
        GameplayModifiers.I.AttackSpeedMultiplier = AttackSpeedMultiplierValue;
    }

    public override void Revert()
    {
        GameplayModifiers.I.AttackSpeedMultiplier = _prev;
    }
}
using UnityEngine;

namespace Unity.FPS.Ours
{
    [CreateAssetMenu(menuName = "Powerups/Attack Speed")]
    public class AttackSpeedPowerup : PowerupBase
    {
        [Range(1f, 5f)] public float Multiplier = 1.5f; // one knob for both fire & reload

        float _prevAtk, _prevReload;

        public override void Apply()
        {
            _prevAtk = GameplayModifiers.I ? GameplayModifiers.I.AttackSpeedMultiplier : 1f;
            _prevReload = GameplayModifiers.I ? GameplayModifiers.I.ReloadSpeedMultiplier : 1f;

            if (GameplayModifiers.I)
            {
                GameplayModifiers.I.AttackSpeedMultiplier = Multiplier;
                GameplayModifiers.I.ReloadSpeedMultiplier = Multiplier;
            }

            Debug.Log($"[PU AttackSpeed] APPLY atk:{_prevAtk}→{Multiplier} reload:{_prevReload}→{Multiplier}");
        }

        public override void Revert()
        {
            if (GameplayModifiers.I)
            {
                GameplayModifiers.I.AttackSpeedMultiplier = _prevAtk;
                GameplayModifiers.I.ReloadSpeedMultiplier = _prevReload;
            }

            Debug.Log($"[PU AttackSpeed] REVERT atk→{_prevAtk} reload→{_prevReload}");
        }
    }

}

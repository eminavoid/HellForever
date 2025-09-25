using UnityEngine;

namespace Unity.FPS.Ours
{
    [DisallowMultipleComponent]
    public class PlayerGameplayModifiers : MonoBehaviour
    {
        [Header("Per-Player Multipliers (1 = no change)")]

        [Range(0.1f, 5f)][SerializeField] private float damageTakenMultiplier = 1f;
        [Range(0.1f, 5f)][SerializeField] private float attackSpeedMultiplier = 1f;
        [Range(0.1f, 5f)][SerializeField] private float jumpHeightMultiplier = 1f;
        [Range(0.1f, 5f)][SerializeField] private float weaponDamageMultiplier = 1f;
        [Range(0.1f, 5f)][SerializeField] private float reloadSpeedMultiplier = 1f;

        public float DamageTakenMultiplier
        {
            get => damageTakenMultiplier;
            set
            {
                damageTakenMultiplier = value;
                Debug.Log($"[PlayerGameplayModifiers] DamageTakenMultiplier = {value}");
            }
        }

        public float AttackSpeedMultiplier
        {
            get => attackSpeedMultiplier;
            set
            {
                attackSpeedMultiplier = value;
                Debug.Log($"[PlayerGameplayModifiers] AttackSpeedMultiplier = {value}");
            }
        }

        public float JumpHeightMultiplier
        {
            get => jumpHeightMultiplier;
            set
            {
                jumpHeightMultiplier = value;
                Debug.Log($"[PlayerGameplayModifiers] JumpHeightMultiplier = {value}");
            }
        }

        public float WeaponDamageMultiplier
        {
            get => weaponDamageMultiplier;
            set
            {
                weaponDamageMultiplier = value;
                Debug.Log($"[PlayerGameplayModifiers] WeaponDamageMultiplier = {value}");
            }
        }

        public float ReloadSpeedMultiplier
        {
            get => reloadSpeedMultiplier;
            set
            {
                reloadSpeedMultiplier = value;
                Debug.Log($"[PlayerGameplayModifiers] ReloadSpeedMultiplier = {value}");
            }
        }
    }
}

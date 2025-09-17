using UnityEngine;

namespace Unity.FPS.Ours
{
    public class GameplayModifiers : MonoBehaviour
    {
        public static GameplayModifiers I { get; private set; }

        [Header("Global Multipliers (1 = no change)")]
        [Range(0.1f, 5f)] public float DamageTakenMultiplier = 1f;   // < 1 = reduce damage received
        [Range(0.1f, 5f)] public float AttackSpeedMultiplier = 1f;   // > 1 = faster fire rate
        [Range(0.1f, 5f)] public float JumpHeightMultiplier = 1f;    // > 1 = higher jump
        [Range(0.1f, 5f)] public float WeaponDamageMultiplier = 1f;  // > 1 = more weapon damage
        [Range(0.1f, 5f)] public float ReloadSpeedMultiplier = 1f;   // >1 = faster reload

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
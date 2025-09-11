using System.Collections;
using UnityEngine;

public abstract class GameplayModifiers : MonoBehaviour
{
    public static GameplayModifiers I { get; private set; }

    [Header("Global Multipliers (1 = sin cambio)")]
    [Range(0.1f, 5f)] public float DamageTakenMultiplier = 1f;     // <1 = menos daño recibido
    [Range(0.1f, 5f)] public float AttackSpeedMultiplier = 1f;     // >1 = dispara más rápido
    [Range(0.1f, 5f)] public float JumpHeightMultiplier = 1f;      // >1 = salta más alto
    [Range(0.1f, 5f)] public float WeaponDamageMultiplier = 1f;    // >1 = hace más daño

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }
}
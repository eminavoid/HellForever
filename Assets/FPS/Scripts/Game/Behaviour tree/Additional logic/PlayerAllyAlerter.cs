using UnityEngine;
using Unity.FPS.Game;        

public class PlayerAllyAlerter : MonoBehaviour
{
    [Header("Referencia al Aliado")]
    [Tooltip("Arrastra aquí el GameObject de tu Aliado")]
    public AllyBehaviorTree allyBehaviorTree;

    private Health playerHealth;

    void Awake()
    {
        playerHealth = GetComponent<Health>();

        if (playerHealth == null)
        {
            return;
        }

        playerHealth.OnDamaged += HandlePlayerDamage;
    }

    private void HandlePlayerDamage(float damageAmount, GameObject damageSource)
    {
        if (allyBehaviorTree != null && damageSource != null)
        {
            allyBehaviorTree.HasTakenDamage = true;

            allyBehaviorTree.LastAttacker = damageSource.transform;
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamaged -= HandlePlayerDamage;
        }
    }
}
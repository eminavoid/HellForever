using UnityEngine;
using Unity.FPS.Game;

public class AllyContactDamage : MonoBehaviour
{
    public float DamageAmount = 2500000f;
    public GameObject DamageSource;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Health enemyHealth = other.GetComponentInParent<Health>();

            if (enemyHealth != null)
            {
                if (DamageSource == null)
                    DamageSource = this.gameObject;

                enemyHealth.TakeDamage(DamageAmount, DamageSource);

                Debug.Log($"DAÑO APLICADO ({DamageAmount}) al padre de la HitBox: {enemyHealth.gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"Colisión con enemigo, pero no se encontró el script Health en el objeto o sus padres.");
            }
        }
    }
}
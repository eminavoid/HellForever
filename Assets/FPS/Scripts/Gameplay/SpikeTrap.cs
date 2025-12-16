using System.Collections;
using Unity.FPS.Game;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] private float damageAmount = 25f;
    [SerializeField] private float damageInterval = 1f;
    private bool canDamage = true;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger detected with " + other.gameObject.name);
        Damageable damageable = other.gameObject.GetComponent<Damageable>();
        if (damageable != null && canDamage)
        {
            damageable.InflictDamage(damageAmount, false, gameObject);
            canDamage = false;
            StartCoroutine(ResetTrap());
        }
    }

    IEnumerator ResetTrap()
    {
        yield return new WaitForSeconds(damageInterval);
        canDamage = true;
    }
}

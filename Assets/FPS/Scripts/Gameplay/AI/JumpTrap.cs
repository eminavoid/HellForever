using System.Collections;
using Unity.FPS.Gameplay;
using UnityEngine;

public class JumpTrap : MonoBehaviour
{
    [SerializeField] private float impulseAmount = 500f;
    [SerializeField] private float trapInterval = 1f;
    bool canDamage = true;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger detected with " + other.gameObject.name);
        // Use TryGetComponent to avoid allocation and check for null
        var playerController = other.GetComponent<MonoBehaviour>();
        if (playerController != null && other.CompareTag("Player"))
        {

                playerController.BroadcastMessage("AddImpulse",  Vector3.up * impulseAmount );
                canDamage = false;
                StartCoroutine(ResetTrap());
        }
    }

    IEnumerator ResetTrap()
    {
        yield return new WaitForSeconds(trapInterval);
        canDamage = true;
    }
}

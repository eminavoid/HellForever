using UnityEngine;
using Photon.Pun;
using System.Collections;
using Unity.FPS.Game; // Health
// Nota: DeathUIController está en Gameplay si seguiste la Opción 2

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(PlayerRespawn))]
    public class PlayerDeathHandler : MonoBehaviourPun
    {
        [Tooltip("Nombre del objeto DeathUI en la jerarquía (panel que contiene DeathUIController).")]
        public string deathUIName = "DeathUI";

        [Tooltip("Tiempo en segundos antes de reaparecer (contado en la UI).")]
        public float respawnDelay = 5f;

        Health health;
        CharacterController characterController;
        PlayerInputHandler inputHandler;
        PlayerWeaponsManager weaponsManager;
        PlayerRespawn playerRespawn;
        DeathUIController deathUIController;

        void Start()
        {
            health = GetComponent<Health>();
            characterController = GetComponent<CharacterController>();
            inputHandler = GetComponent<PlayerInputHandler>();
            weaponsManager = GetComponent<PlayerWeaponsManager>();
            playerRespawn = GetComponent<PlayerRespawn>();

            if (health != null)
                health.OnDie += Die;


            var uiObj = GameObject.Find(deathUIName);
            if (uiObj != null)
            {
                deathUIController = uiObj.GetComponent<DeathUIController>();
                uiObj.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"[PlayerDeathHandler] No se encontró objeto DeathUI con nombre '{deathUIName}' en la escena.");
            }
        }

        void OnDestroy()
        {
            if (health != null)
                health.OnDie -= Die;
        }

 
        private void Die()
        {
            if (!photonView.IsMine) return; 

            Debug.Log("[PlayerDeathHandler] Die() invoked - teleportando al spawn y bloqueando controles.");

          
            if (playerRespawn != null)
            {
            
                bool ccWasEnabled = (characterController != null) ? characterController.enabled : false;
                if (characterController != null && ccWasEnabled)
                    characterController.enabled = false;

                transform.position = playerRespawn.GetSpawnPosition();
                transform.rotation = playerRespawn.GetSpawnRotation();

             
            }
            else
            {
                Debug.LogWarning("[PlayerDeathHandler] PlayerRespawn no encontrado; no se teletransportó.");
            }

          
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

         
            if (inputHandler != null) inputHandler.enabled = false;
            if (weaponsManager != null) weaponsManager.enabled = false;
            if (characterController != null) characterController.enabled = false;

          
            if (deathUIController != null)
            {
                deathUIController.StartCountdown(respawnDelay);
            }
            else
            {
                Debug.LogWarning("[PlayerDeathHandler] deathUIController null - no se mostrará contador.");
            }

           
            StartCoroutine(RespawnCountdown());
        }

        IEnumerator RespawnCountdown()
        {
            yield return new WaitForSeconds(respawnDelay);

            if (deathUIController != null)
                deathUIController.Hide();

           
            if (health != null)
            {
                health.CurrentHealth = health.MaxHealth;
                health.Revive();
                health.OnHealed?.Invoke(health.MaxHealth);
            }

          
            if (playerRespawn != null && photonView.IsMine)
            {
                if (characterController != null)
                    characterController.enabled = false;

                transform.position = playerRespawn.GetSpawnPosition();
                transform.rotation = playerRespawn.GetSpawnRotation();
            }

           
            if (inputHandler != null) inputHandler.enabled = true;
            if (weaponsManager != null) weaponsManager.enabled = true;
            if (characterController != null) characterController.enabled = true;

            Debug.Log("[PlayerDeathHandler] Respawn completo: vida restaurada, estado reseteado y controles reactivados.");
        }
    }
}

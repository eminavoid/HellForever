using Photon.Pun;
using UnityEngine;

namespace Unity.FPS.Ours
{
    [RequireComponent(typeof(Collider))]
    public class PowerupPickup : MonoBehaviourPun
    {
        [Tooltip("Ruta en Resources al asset del powerup, p.ej. 'Powerups/DamageBoost'")]
        public string PowerupResourcePath;

        void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
            var rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; rb.useGravity = false;
        }

        void OnTriggerEnter(Collider other) => TryGive(other);
        private void OnTriggerStay(Collider other)
        {
            if (!photonView.IsMine) return;

            var playerPv = other.GetComponent<PhotonView>();
            if (playerPv != null && playerPv.IsMine && Input.GetKeyDown(KeyCode.E)) // o tu input de recoger
            {
                // Enviar al MasterClient para validar el pickup
                photonView.RPC(nameof(RPC_RequestPickup), RpcTarget.MasterClient, playerPv.ViewID, gameObject.name);
            }
        }


        void TryGive(Collider other)
        {
            var pv = other.GetComponentInParent<PhotonView>();
            if (!pv) return;

            // Solo el cliente dueño pide el pickup al Master (evita dobles)
            if (pv.IsMine && !string.IsNullOrEmpty(PowerupResourcePath))
            {
                photonView.RPC(nameof(RPC_RequestPickup), RpcTarget.MasterClient, pv.ViewID, PowerupResourcePath);
            }
        }

        [PunRPC]
        private void RPC_RequestPickup(int playerViewId, string powerupPath, PhotonMessageInfo info)
        {
            PhotonView playerPv = PhotonView.Find(playerViewId);
            if (playerPv != null)
            {
                // Aquí se manda un RPC directo SOLO al jugador dueño
                playerPv.RPC("RPC_ApplyPowerup", playerPv.Owner, powerupPath);
            }

            // Destruir el pickup en red (solo el master lo hace)
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.Destroy(gameObject);
        }

    }
}

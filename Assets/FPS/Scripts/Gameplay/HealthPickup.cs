using UnityEngine;
using Photon.Pun;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    public class HealthPickup : Pickup
    {
        [Header("Parameters")]
        [Tooltip("Amount of health to heal on pickup")]
        public float HealAmount = 25f;

        protected override void OnPicked(PlayerCharacterController player)
        {
            if (player == null) return;

            if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
                return;

            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth == null) return;

            if (playerHealth.CanPickup())
            {
                playerHealth.Heal(HealAmount);

                PlayPickupFeedback();

                if (PhotonNetwork.IsConnected)
                {
                    PhotonView pv = GetComponent<PhotonView>();
                    if (pv != null)
                        PhotonNetwork.Destroy(gameObject);
                    else
                        Destroy(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}

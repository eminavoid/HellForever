using UnityEngine;
using Photon.Pun;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;


namespace Unity.FPS.UI
{
    public class LocalHUDInitializer : MonoBehaviour
    {
        private bool isLinked = false;

        void Update()
        {
            if (isLinked) return;

            foreach (var health in FindObjectsByType<Unity.FPS.Game.Health>(FindObjectsSortMode.None))
            {
                // 👇 Chequeo extra: solo si tiene tag "Player"
                if (health.CompareTag("Player") &&
                    health.TryGetComponent<PhotonView>(out var pv) && pv.IsMine)
                {
                    Debug.Log($"[LocalHUDInitializer] Player local detectado con tag Player: {health.gameObject.name}");

                    // HUD vida
                    var healthBar = FindFirstObjectByType<PlayerHealthBar>();
                    if (healthBar != null)
                    {
                        healthBar.SetPlayer(health);
                        Debug.Log("[LocalHUDInitializer] HUD de vida vinculado al Player local.");
                    }

                    // HUD armas
                    var weapons = health.GetComponent<PlayerWeaponsManager>();
                    var weaponHud = FindFirstObjectByType<WeaponHUDManager>();
                    if (weaponHud != null)
                    {
                        weaponHud.SetPlayerWeaponsManager(weapons);
                        Debug.Log("[LocalHUDInitializer] HUD de armas vinculado al Player local.");
                    }

                    isLinked = true;
                    break;
                }
            }
        }
    }
}

using UnityEngine;
using Photon.Pun;

namespace Unity.FPS.Ours
{
    public class PlayerPowerupReceiver : MonoBehaviourPun
    {
        private PlayerGameplayModifiers _mods;
        private PowerupStackRunner _runner;

        void Awake()
        {
            _mods = GetComponent<PlayerGameplayModifiers>();
            _runner = GetComponent<PowerupStackRunner>();
        }

        [PunRPC]
        private void RPC_ApplyPowerup(string powerupName)
        {
            Debug.Log($"[PlayerPowerupReceiver] Jugador {photonView.ViewID} recibe powerup: {powerupName}");

            // Cargar desde Resources/Powerups/<nombre exacto>
            var prefab = Resources.Load<PowerupBase>("Powerups/" + powerupName);
            if (prefab == null)
            {
                Debug.LogError($"[PlayerPowerupReceiver] No se encontró Resources/Powerups/{powerupName}");
                return;
            }

            if (_mods == null)
            {
                Debug.LogError("[PlayerPowerupReceiver] Falta PlayerGameplayModifiers en el Player");
                return;
            }

            var adapter = new PowerupTaskAdapter(prefab, _mods);

            if (_runner != null)
            {
                _runner.Push(adapter);
                Debug.Log($"[PlayerPowerupReceiver] Encolado {powerupName} en StackRunner");
            }
            else
            {
                // fallback por si no hubiera runner
                var instance = ScriptableObject.Instantiate(prefab);
                instance.Apply(_mods);
                Debug.LogWarning("[PlayerPowerupReceiver] No hay PowerupStackRunner, aplicado sin duración.");
            }
        }
    }
}

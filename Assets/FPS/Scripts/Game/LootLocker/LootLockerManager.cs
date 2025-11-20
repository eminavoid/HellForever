using UnityEngine;
using LootLocker.Requests;
using System.Collections;

namespace Unity.FPS.Game
{
    public class LootLockerManager : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        IEnumerator Start()
        {
            // Esperamos un frame para asegurar que el SDK cargó su config
            yield return null;

            LootLockerSDKManager.StartGuestSession((response) =>
            {
                if (response.success)
                {
                    Debug.Log("✅ LootLocker: Sesión iniciada.");
                    PlayerPrefs.SetString("PlayerID", response.player_id.ToString());
                }
                else
                {
                    Debug.LogError("❌ LootLocker Error: " + response.errorData.message);
                }
            });
        }
    }
}
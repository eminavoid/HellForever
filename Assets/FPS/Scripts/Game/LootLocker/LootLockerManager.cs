using UnityEngine;
using LootLocker.Requests;
using System.Collections;
using Photon.Pun; // Necesario para el nombre

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
            yield return null;

            LootLockerSDKManager.StartGuestSession((response) =>
            {
                if (response.success)
                {
                    Debug.Log("✅ LootLocker: Sesión iniciada.");
                    PlayerPrefs.SetString("PlayerID", response.player_id.ToString());

                    
                    string nickname = PhotonNetwork.NickName;
                    if (string.IsNullOrEmpty(nickname)) nickname = "Player " + Random.Range(1000, 9999);

                    LootLockerSDKManager.SetPlayerName(nickname, (nameResponse) =>
                    {
                        if (nameResponse.success) Debug.Log("Nombre actualizado en LootLocker: " + nickname);
                    });
                   
                }
                else
                {
                    Debug.LogError("❌ LootLocker Error: " + response.errorData.message);
                }
            });
        }
    }
}
using UnityEngine;
using LootLocker.Requests;

public class LootLockerManager : MonoBehaviour
{
    // Hacemos que este objeto persista entre escenas (Lobby -> MainScene)
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Iniciar la sesión de invitado al comenzar el juego
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("LootLocker: Sesión de invitado iniciada con éxito.");
            }
            else
            {
                // Versión Corregida: Se usa .errorData.message
                Debug.LogError("LootLocker: Error al iniciar sesión de invitado: " + response.errorData.message);
            }
        });
    }
}
using UnityEngine;
using Unity.FPS.Game;

public class DebugGameEnder : MonoBehaviour
{
    // Hacemos que este objeto no se destruya al cambiar de escena
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // Al presionar la tecla 'G'
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.LogWarning("🔴 [DEBUG] Tecla 'G' presionada. Forzando envío de puntajes...");

            if (ScoreManager.Instance != null)
            {
                // 1. Enviamos los puntajes a LootLocker
                ScoreManager.Instance.SubmitGameResult();

                Debug.Log("✅ [DEBUG] SubmitGameResult() llamado. Revisa la consola para ver la respuesta de LootLocker.");
            }
            else
            {
                Debug.LogError("❌ [DEBUG] Error: No se encontró el ScoreManager en la escena.");
            }
        }
    }
}
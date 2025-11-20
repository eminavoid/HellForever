using UnityEngine;
using Unity.FPS.Game;

public class DebugGameEnder : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.LogWarning("🔴 [DEBUG] Tecla 'G' presionada. Buscando ScoreManager...");

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.SubmitGameResult();
                Debug.Log("✅ [DEBUG] ScoreManager encontrado. Enviando datos...");
            }
            else
            {
                // --- ESTE ES EL MENSAJE QUE TE VA A SALIR AHORA ---
                Debug.LogError("❌ [DEBUG] ERROR CRÍTICO: ScoreManager.Instance es NULL. \n" +
                               "Causas probables:\n" +
                               "1. No iniciaste desde la escena del Lobby.\n" +
                               "2. El objeto '_Managers' no tiene el script ScoreManager.\n" +
                               "3. Hay dos ScoreManagers en la escena y uno se destruyó.");
            }
        }
    }
}
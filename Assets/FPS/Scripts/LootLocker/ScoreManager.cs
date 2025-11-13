using UnityEngine;
using LootLocker.Requests;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    // Tus keys (asegúrate que coincidan con tu dashboard)
    private const string LEADERBOARD_KEY_SCORE = "total_score";
    private const string LEADERBOARD_KEY_ROUNDS = "highestround";

    private int currentScore = 0;
    private int currentRound = 0;

    void Awake()
    {
        // Configuración del Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // Asegurarse de que no se destruya al cargar la escena de juego
            DontDestroyOnLoad(gameObject);
        }
    }

    // --- Métodos Públicos para tu juego ---

    public void ResetScores()
    {
        currentScore = 0;
        currentRound = 0;
        Debug.Log("ScoreManager: Puntuación y Rondas reseteadas.");
    }

    public void AddScore(int points)
    {
        currentScore += points;
        Debug.Log($"ScoreManager: Puntos añadidos. Nuevo Total: {currentScore}"); // Log de prueba
    }

    public void IncrementRound()
    {
        currentRound++;
        Debug.Log($"ScoreManager: Ronda incrementada. Nueva Ronda: {currentRound}"); // Log de prueba
    }

    // --- Método de Envío a LootLocker ---

    public void SubmitAllScores(string playerName)
    {
        StartCoroutine(SubmitScoresRoutine(playerName));
    }

    private IEnumerator SubmitScoresRoutine(string playerName)
    {
        bool scoreSubmitted = false;
        bool roundSubmitted = false;

        Debug.Log($"Enviando puntajes para {playerName}: Score={currentScore}, Round={currentRound}");

        // 1. Enviar Puntuación Total
        LootLockerSDKManager.SubmitScore(
            playerName,
            currentScore,
            LEADERBOARD_KEY_SCORE,
            (response) => {
                if (response.success)
                {
                    Debug.Log("LootLocker: Puntuación total enviada con éxito.");
                }
                else
                {
                    // Versión Corregida: Se usa .errorData.message
                    Debug.LogError("LootLocker: Error al enviar puntuación total: " + response.errorData.message);
                }
                scoreSubmitted = true;
            });

        yield return new WaitUntil(() => scoreSubmitted);

        // 2. Enviar Ronda Más Alta
        LootLockerSDKManager.SubmitScore(
            playerName,
            currentRound,
            LEADERBOARD_KEY_ROUNDS,
            (response) => {
                if (response.success)
                {
                    Debug.Log("LootLocker: Ronda más alta enviada con éxito.");
                }
                else
                {
                    // Versión Corregida: Se usa .errorData.message
                    Debug.LogError("LootLocker: Error al enviar ronda: " + response.errorData.message);
                }
                roundSubmitted = true;
            });

        yield return new WaitUntil(() => roundSubmitted);

        Debug.Log("Ambos puntajes enviados. Reseteando marcadores.");
        ResetScores();
    }
}
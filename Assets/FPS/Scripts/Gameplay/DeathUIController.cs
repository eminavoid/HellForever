using UnityEngine;
using TMPro;
namespace Unity.FPS.Gameplay 
{
    public class DeathUIController : MonoBehaviour
    {
        [Tooltip("Texto del contador de respawn")]
        public TextMeshProUGUI respawnText;

        /// <summary>
        /// Inicia el contador en la UI.
        /// </summary>
        public void StartCountdown(float duration)
        {
            if (respawnText == null)
            {
                Debug.LogWarning("[DeathUIController] No hay TextMeshPro asignado para el contador.");
                return;
            }

            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(CountdownRoutine(duration));
        }

        private System.Collections.IEnumerator CountdownRoutine(float duration)
        {
            float timer = duration;

            while (timer > 0)
            {
                respawnText.text = $"Respawn en {Mathf.Ceil(timer)}...";
                yield return new WaitForSeconds(1f);
                timer--;
            }

            respawnText.text = "Respawn!";
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}

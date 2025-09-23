using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class PlayerHealthBar : MonoBehaviour
    {
        [Tooltip("Image component displaying current health")]
        public Image HealthFillImage;

        Health m_PlayerHealth;

        void Start()
        {
            
            PlayerCharacterController playerCharacterController = FindObjectOfType<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, PlayerHealthBar>(
                playerCharacterController, this);

            if (playerCharacterController != null)
            {
                m_PlayerHealth = playerCharacterController.GetComponent<Health>();
                DebugUtility.HandleErrorIfNullGetComponent<Health, PlayerHealthBar>(
                    m_PlayerHealth, this, playerCharacterController != null ? playerCharacterController.gameObject : null);
            }
        }

        void Update()
        {
            if (m_PlayerHealth == null) return;

            // update health bar value
            HealthFillImage.fillAmount = m_PlayerHealth.CurrentHealth / m_PlayerHealth.MaxHealth;
        }
    }
}

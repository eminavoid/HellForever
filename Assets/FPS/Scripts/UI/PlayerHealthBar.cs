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

        private Health m_PlayerHealth;

        public void SetPlayer(Health playerHealth)
        {
            if (m_PlayerHealth != null)
            {
                m_PlayerHealth.OnDamaged -= UpdateUI;
                m_PlayerHealth.OnHealed -= UpdateUI;
            }

            m_PlayerHealth = playerHealth;

            if (m_PlayerHealth != null)
            {
                m_PlayerHealth.OnDamaged += UpdateUI;  // (float, GameObject)
                m_PlayerHealth.OnHealed += UpdateUI;  // (float)
                RefreshBar();
            }
        }

        void OnDestroy()
        {
            if (m_PlayerHealth != null)
            {
                m_PlayerHealth.OnDamaged -= UpdateUI;
                m_PlayerHealth.OnHealed -= UpdateUI;
            }
        }

        private void UpdateUI(float _)   // OnHealed
        {
            RefreshBar();
        }

        private void UpdateUI(float _, GameObject __)  // OnDamaged
        {
            RefreshBar();
        }

        private void RefreshBar()
        {
            if (m_PlayerHealth == null) return;
            HealthFillImage.fillAmount = m_PlayerHealth.CurrentHealth / m_PlayerHealth.MaxHealth;
        }
    }
}

using Unity.FPS.Ours;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

namespace Unity.FPS.Game
{
    public class Health : MonoBehaviourPun, IPunObservable
    {
        [Tooltip("Maximum amount of health")]
        public float MaxHealth = 10f;

        [Tooltip("Health ratio at which the critical health vignette starts appearing")]
        public float CriticalHealthRatio = 0.3f;

        public UnityAction<float, GameObject> OnDamaged;
        public UnityAction<float> OnHealed;
        public UnityAction OnDie;

        public float CurrentHealth { get; set; }
        public bool Invincible { get; set; }
        public bool CanPickup() => CurrentHealth < MaxHealth;

        public float GetRatio() => CurrentHealth / MaxHealth;
        public bool IsCritical() => GetRatio() <= CriticalHealthRatio;

        bool m_IsDead;

        void Start()
        {
            CurrentHealth = MaxHealth;
        }

        public void Heal(float healAmount)
        {
            if (photonView != null && !photonView.IsMine)
                return;

            float healthBefore = CurrentHealth;
            CurrentHealth += healAmount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

            float trueHealAmount = CurrentHealth - healthBefore;
            if (trueHealAmount > 0f)
            {
                OnHealed?.Invoke(trueHealAmount);
            }
        }

        public void TakeDamage(float damage, GameObject damageSource)
        {
            if (photonView != null && !photonView.IsMine)
                return;

            if (Invincible)
                return;

            float healthBefore = CurrentHealth;

            bool isPlayer = CompareTag("Player");

            float baseAmount = damage;
            float mul = 1f;
            if (isPlayer && GameplayModifiers.I != null)
                mul = GameplayModifiers.I.DamageTakenMultiplier;

            float final = baseAmount * mul;

            CurrentHealth = Mathf.Clamp(CurrentHealth - final, 0f, MaxHealth);

            Debug.Log($"[Health] {name} took damage from {(damageSource ? damageSource.name : "unknown")}" +
                      $" | Base={baseAmount} Final={final} (Mul={mul}) | HP {healthBefore} -> {CurrentHealth}");

            float trueDamageAmount = healthBefore - CurrentHealth;
            if (trueDamageAmount > 0f)
                OnDamaged?.Invoke(trueDamageAmount, damageSource);

            HandleDeath();
        }

        public void Kill()
        {
            CurrentHealth = 0f;
            OnDamaged?.Invoke(MaxHealth, null);
            HandleDeath();
        }

        void HandleDeath()
        {
            if (m_IsDead)
                return;

            if (CurrentHealth <= 0f)
            {
                m_IsDead = true;
                OnDie?.Invoke();
            }
        }

        // 🔹 Sincronización de vida
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
                stream.SendNext(CurrentHealth);
            else
                CurrentHealth = (float)stream.ReceiveNext();
        }
    }
}

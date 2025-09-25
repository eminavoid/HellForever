using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using Unity.FPS.Ours;

namespace Unity.FPS.Game
{
    public class Health : MonoBehaviourPun, IPunObservable
    {
        [Header("Health")]
        [Tooltip("Salud máxima")]
        public float MaxHealth = 100f;

        [Tooltip("Salud actual (sincronizada)")]
        public float CurrentHealth = 100f;

        [Tooltip("Si es true, no recibe daño")]
        public bool Invincible = false;

        [Header("Crítico")]
        [Tooltip("Umbral (0-1) para considerar vida crítica")]
        [Range(0f, 1f)] public float CriticalHealthRatio = 0.3f;

        // --- Eventos públicos (usados por tu UI y otros sistemas) ---
        public UnityAction OnDie;
        public UnityAction<float, GameObject> OnDamaged; // (daño final aplicado, fuente)
        public UnityAction<float> OnHealed;

        // Flag interno para no disparar OnDie más de una vez
        bool m_IsDead;

        void Awake()
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);
            m_IsDead = CurrentHealth <= 0f;
        }

        public void TakeDamage(float damage, GameObject damageSource)
        {
            if (Invincible || m_IsDead) return;

            if (PhotonNetwork.IsConnected)
            {
                int srcId = GetSourceViewId(damageSource);
                // Enviamos el daño base y el viewID del atacante (si existe)
                photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.All, damage, srcId);
            }
            else
            {
                ApplyDamageWithTargetMultipliers(damage, damageSource);
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || m_IsDead) return;

            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC(nameof(RPC_Heal), RpcTarget.All, amount);
            }
            else
            {
                ApplyHeal(amount);
            }
        }

        public void Kill()
        {
            if (m_IsDead) return;

            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC(nameof(RPC_Kill), RpcTarget.All);
            }
            else
            {
                CommitDeath();
            }
        }

        public bool IsCritical()
        {
            if (MaxHealth <= 0f) return false;
            return (CurrentHealth / MaxHealth) <= CriticalHealthRatio;
        }

        [PunRPC]
        void RPC_TakeDamage(float baseDamage, int sourceViewId)
        {
            GameObject src = sourceViewId != 0 ? PhotonView.Find(sourceViewId)?.gameObject : null;
            ApplyDamageWithTargetMultipliers(baseDamage, src);
        }

        [PunRPC]
        void RPC_Heal(float amount)
        {
            ApplyHeal(amount);
        }

        [PunRPC]
        void RPC_Kill()
        {
            CommitDeath();
        }

        void ApplyDamageWithTargetMultipliers(float baseDamage, GameObject damageSource)
        {
            if (Invincible || m_IsDead) return;

            float takenMul = GetDamageTakenMultiplier(damageSource);
            float finalDamage = Mathf.Max(0f, baseDamage * takenMul);

            float before = CurrentHealth;
            CurrentHealth = Mathf.Clamp(CurrentHealth - finalDamage, 0f, MaxHealth);

            if (CurrentHealth < before)
            {
                OnDamaged?.Invoke(finalDamage, damageSource);
            }

            if (CurrentHealth <= 0f && !m_IsDead)
            {
                CommitDeath();
            }
        }

        void ApplyHeal(float amount)
        {
            if (m_IsDead) return;

            float before = CurrentHealth;
            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0f, MaxHealth);
            float applied = CurrentHealth - before;

            if (applied > 0f)
                OnHealed?.Invoke(applied);
        }

        void CommitDeath()
        {
            if (m_IsDead) return;
            m_IsDead = true;
            CurrentHealth = 0f;
            OnDie?.Invoke();
            // Nota: la destrucción/FX la maneja quien escuche OnDie (p.ej. EnemyController -> PhotonNetwork.Destroy)
        }

        
        float GetDamageTakenMultiplier(GameObject damageSource)
        {
            float mul = 1f;

            bool isPlayer = CompareTag("Player");

            if (isPlayer && GameplayModifiers.I != null)
            {
                // Mantiene tus powerups de "menos daño recibido"
                mul *= GameplayModifiers.I.DamageTakenMultiplier;
            }


            return mul;
        }

        int GetSourceViewId(GameObject go)
        {
            if (!go) return 0;
            var pv = go.GetComponentInParent<PhotonView>();
            return pv ? pv.ViewID : 0;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(CurrentHealth);
                stream.SendNext(m_IsDead);
            }
            else
            {
                CurrentHealth = (float)stream.ReceiveNext();
                m_IsDead = (bool)stream.ReceiveNext();
            }
        }
        public bool CanPickup()
        {
            return !m_IsDead && CurrentHealth < MaxHealth;
        }
    }
}

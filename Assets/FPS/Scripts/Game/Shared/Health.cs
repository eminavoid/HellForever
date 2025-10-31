// Assets/FPS/Scripts/Game/Health.cs
using Photon.Pun;
using Unity.FPS.Ours;
using UnityEngine;
using UnityEngine.Events;

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

        public UnityAction OnDie;
        public UnityAction<float, GameObject> OnDamaged;
        public UnityAction<float> OnHealed;

        bool m_IsDead;

        void Awake()
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);
            m_IsDead = CurrentHealth <= 0f;
        }
        public void TakeDamage(float baseDamage, GameObject damageSource)
        {
            if (Invincible || m_IsDead) return;

            if (PhotonNetwork.IsConnected)
            {
                int srcId = GetSourceViewId(damageSource);
                bool isPlayerTarget = CompareTag("Player");

                if (isPlayerTarget)
                {
                    // Player: todos aplican el mismo cálculo local (mantiene powerups defensivos)
                    photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.All, baseDamage, srcId);
                }
                else
                {
                    // Enemigo: solicitar al MASTER que aplique y difunda
                    if (PhotonNetwork.IsMasterClient)
                    {
                        photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.All, baseDamage, srcId);
                    }
                    else
                    {
                        photonView.RPC(nameof(RPC_RequestDamage), RpcTarget.MasterClient, baseDamage, srcId);
                    }
                }
            }
            else
            {
                ApplyDamageWithTargetMultipliers(baseDamage, damageSource);
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
        void RPC_RequestDamage(float baseDamage, int sourceViewId)
        {
            // Master reenvía a TODOS: así cada cliente aplica el mismo cálculo y dispara sus eventos
            photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.All, baseDamage, sourceViewId);
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
        }
        float GetDamageTakenMultiplier(GameObject damageSource)
        {
            float mul = 1f;
            var mods = GetComponent<Unity.FPS.Ours.PlayerGameplayModifiers>();
            if (mods != null) mul *= mods.DamageTakenMultiplier; 
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
        public void RespawnFull()
        {
            if (PhotonNetwork.IsConnected)
                photonView.RPC(nameof(RPC_RespawnFull), RpcTarget.All);
            else
                DoRespawnFull();
        }

        [PunRPC]
        void RPC_RespawnFull()
        {
            DoRespawnFull();
        }

        void DoRespawnFull()
        {
            m_IsDead = false;
            float prev = CurrentHealth;
            CurrentHealth = MaxHealth;

            OnHealed?.Invoke(CurrentHealth - prev);

            Invincible = false;
        }
        public void Revive()
        {
            m_IsDead = false;
        }

    }
}

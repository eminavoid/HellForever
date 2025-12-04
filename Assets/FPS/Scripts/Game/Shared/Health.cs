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

        [Header("LootLocker")]
        [Tooltip("Puntos que da al morir. 0 = Es un Jugador. >0 = Es un Enemigo.")]
        public int PointsOnDeath = 0;

        [Header("Crítico")]
        [Tooltip("Umbral (0-1) para considerar vida crítica")]
        [Range(0f, 1f)] public float CriticalHealthRatio = 0.3f;

        [Header("Death Body")]
        [Tooltip("Prefab del cuerpo de muerte que se spawnea al morir (solo Player)")]
        public GameObject DeathBodyPrefab;

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
                    // Player: todos aplican el mismo cálculo local
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
                CommitDeath(null);
            }
        }

        public bool IsCritical()
        {
            if (MaxHealth <= 0f) return false;
            return (CurrentHealth / MaxHealth) <= CriticalHealthRatio;
        }

        // --- RPCs ---

        [PunRPC]
        void RPC_TakeDamage(float baseDamage, int sourceViewId)
        {
            GameObject src = sourceViewId != 0 ? PhotonView.Find(sourceViewId)?.gameObject : null;
            ApplyDamageWithTargetMultipliers(baseDamage, src);
        }

        [PunRPC]
        void RPC_RequestDamage(float baseDamage, int sourceViewId)
        {
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
            CommitDeath(null);
        }

        // --- Lógica Interna ---

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
                // Aquí pasamos quién nos mató
                CommitDeath(damageSource);
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

        // ===========================
        //  MUERTE + CADÁVER
        // ===========================
        void CommitDeath(GameObject damageSource)
        {
            if (m_IsDead) return;

            m_IsDead = true;
            CurrentHealth = 0f;

            // 1) Spawnear cuerpo de muerte (solo para jugadores)
            SpawnDeathBody();

            // 2) Notificar muerte a otros sistemas
            OnDie?.Invoke();

            // 3) Integración Score Manager (PVE y PVP)
            if (ScoreManager.Instance != null)
            {
                // 1. PVE: Si es un Enemigo (PointsOnDeath > 0) y soy el Host
                if (PointsOnDeath > 0 && PhotonNetwork.IsMasterClient)
                {
                    ScoreManager.Instance.AddScore(PointsOnDeath);
                }

                // 2. PVP: Si es un Jugador (PointsOnDeath == 0)
                if (PointsOnDeath == 0)
                {
                    // Detectar Asesino para sumar Kill
                    if (damageSource != null)
                    {
                        PhotonView killerView = damageSource.GetComponent<PhotonView>();

                        // Si el asesino es un jugador y no es suicidio
                        if (killerView != null && killerView.gameObject != gameObject)
                        {
                            var killHandler = killerView.GetComponent<PlayerKillHandler>();
                            if (killHandler != null)
                            {
                                // Enviar RPC al dueño del asesino: "Sumate 1 Kill"
                                killHandler.photonView.RPC("AddKillRPC", killerView.Owner);
                            }
                        }
                    }

                    // NOTA: LootLocker se maneja al final de la partida
                    // con ScoreManager.Instance.SubmitGameResult()
                }
            }
        }

        void SpawnDeathBody()
        {
            // Solo queremos cuerpo de muerte para el jugador, no para todos los enemigos
            if (DeathBodyPrefab == null) return;
            if (!CompareTag("Player")) return;

            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;

            if (PhotonNetwork.IsConnected)
            {
                // Prefab debe estar en una carpeta Resources y tener PhotonView
                PhotonNetwork.Instantiate(DeathBodyPrefab.name, pos, rot);
            }
            else
            {
                Instantiate(DeathBodyPrefab, pos, rot);
            }
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

        // --- Métodos de Compatibilidad ---

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

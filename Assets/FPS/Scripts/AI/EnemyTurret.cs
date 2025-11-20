using Unity.FPS.Game;
using UnityEngine;
using Photon.Pun;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyTurret : MonoBehaviourPun, IPunObservable
    {
        public enum AIState { Idle, Attack }

        public Transform TurretPivot;
        public Transform TurretAimPoint;
        public Animator Animator;
        public float AimRotationSharpness = 5f;
        public float LookAtRotationSharpness = 2.5f;
        public float DetectionFireDelay = 1f;
        public float AimingTransitionBlendTime = 1f;

        public ParticleSystem[] RandomHitSparks;
        public ParticleSystem[] OnDetectVfx;
        public AudioClip OnDetectSfx;

        public AIState AiState { get; private set; }

        EnemyController m_EnemyController;
        Health m_Health;
        Quaternion m_RotationWeaponForwardToPivot;
        float m_TimeStartedDetection;
        float m_TimeLostDetection;
        Quaternion m_PreviousPivotAimingRotation;
        Quaternion m_PivotAimingRotation;

        private Quaternion m_NetworkPivotRotation;

        const string k_AnimOnDamagedParameter = "OnDamaged";
        const string k_AnimIsActiveParameter = "IsActive";

        void Start()
        {
            m_Health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, EnemyTurret>(m_Health, this, gameObject);
            m_Health.OnDamaged += OnDamaged;

            m_EnemyController = GetComponent<EnemyController>();
            DebugUtility.HandleErrorIfNullGetComponent<EnemyController, EnemyTurret>(m_EnemyController, this, gameObject);

            m_EnemyController.onDetectedTarget += OnDetectedTarget;
            m_EnemyController.onLostTarget += OnLostTarget;

            m_RotationWeaponForwardToPivot =
                Quaternion.Inverse(m_EnemyController.GetCurrentWeapon().WeaponMuzzle.rotation) * TurretPivot.rotation;

            AiState = AIState.Idle;

            m_TimeStartedDetection = Mathf.NegativeInfinity;
            m_PreviousPivotAimingRotation = TurretPivot.rotation;

            m_NetworkPivotRotation = TurretPivot.localRotation;
        }

        void Update()
        {
            if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;
            UpdateCurrentAiState();
        }

        void LateUpdate()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                UpdateTurretAiming();
            }
            else
            {
                if (TurretPivot != null)
                {
                    TurretPivot.localRotation = Quaternion.Slerp(
                        TurretPivot.localRotation,
                        m_NetworkPivotRotation,
                        Time.deltaTime * AimRotationSharpness     
                    );
                }
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(TurretPivot.localRotation);
            }
            else
            {
                m_NetworkPivotRotation = (Quaternion)stream.ReceiveNext();
            }
        }
        void UpdateCurrentAiState()
        {
            switch (AiState)
            {
                case AIState.Attack:
                    bool mustShoot = Time.time > m_TimeStartedDetection + DetectionFireDelay;

                    if (m_EnemyController.KnownDetectedTarget == null) return;

                    Vector3 directionToTarget =
                        (m_EnemyController.KnownDetectedTarget.transform.position - TurretAimPoint.position).normalized;

                    Quaternion offsettedTargetRotation =
                        Quaternion.LookRotation(directionToTarget) * m_RotationWeaponForwardToPivot;

                    m_PivotAimingRotation = Quaternion.Slerp(
                        m_PreviousPivotAimingRotation,
                        offsettedTargetRotation,
                        (mustShoot ? AimRotationSharpness : LookAtRotationSharpness) * Time.deltaTime
                    );

                    if (mustShoot)
                    {
                        Vector3 correctedDirectionToTarget =
                            (m_PivotAimingRotation * Quaternion.Inverse(m_RotationWeaponForwardToPivot)) * Vector3.forward;

                        m_EnemyController.TryAtack(TurretAimPoint.position + correctedDirectionToTarget);
                    }
                    break;
            }
        }

        void UpdateTurretAiming()
        {
            switch (AiState)
            {
                case AIState.Attack:
                    TurretPivot.rotation = m_PivotAimingRotation;
                    break;
                default:
                    TurretPivot.rotation = Quaternion.Slerp(
                        m_PivotAimingRotation,
                        TurretPivot.rotation,
                        (Time.time - m_TimeLostDetection) / AimingTransitionBlendTime
                    );
                    break;
            }

            m_PreviousPivotAimingRotation = TurretPivot.rotation;
        }

        void OnDamaged(float dmg, GameObject source)
        {
            if (RandomHitSparks.Length > 0)
            {
                int n = Random.Range(0, RandomHitSparks.Length - 1);
                RandomHitSparks[n].Play();
            }

            if (!PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient)
            {
                Animator.SetTrigger(k_AnimOnDamagedParameter);
            }
        }

        void OnDetectedTarget()
        {
            if (AiState == AIState.Idle)
                AiState = AIState.Attack;

            for (int i = 0; i < OnDetectVfx.Length; i++)
                OnDetectVfx[i].Play();

            if (OnDetectSfx)
                AudioUtility.CreateSFX(OnDetectSfx, transform.position, AudioUtility.AudioGroups.EnemyDetection, 1f);

            Animator.SetBool(k_AnimIsActiveParameter, true);
            m_TimeStartedDetection = Time.time;
        }

        void OnLostTarget()
        {
            if (AiState == AIState.Attack)
                AiState = AIState.Idle;

            for (int i = 0; i < OnDetectVfx.Length; i++)
                OnDetectVfx[i].Stop();

            Animator.SetBool(k_AnimIsActiveParameter, false);
            m_TimeLostDetection = Time.time;
        }
    }
}
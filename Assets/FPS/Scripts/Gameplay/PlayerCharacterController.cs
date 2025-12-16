using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;
using Unity.FPS.Ours;
using Unity.FPS.ours;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputHandler), typeof(AudioSource))]
    public class PlayerCharacterController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the main camera used for the player")]
        public Camera PlayerCamera;

        [Tooltip("Audio source for footsteps, jump, etc...")]
        public AudioSource AudioSource;

        [Header("General")]
        [Tooltip("Force applied downward when in the air")]
        public float GravityDownForce = 20f;

        [Tooltip("Physic layers checked to consider the player grounded")]
        public LayerMask GroundCheckLayers = -1;

        [Tooltip("distance from the bottom of the character controller capsule to test for grounded")]
        public float GroundCheckDistance = 0.05f;

        [Header("Quake Movement")]
        [Tooltip("Max ground strafe/run speed (similar to sv_maxspeed feel)")]
        public float MaxSpeedOnGround = 7.0f;
        [Tooltip("Ground acceleration (Quake-style)")]
        public float GroundAcceleration = 14.0f;
        [Tooltip("Friction applied while grounded")]
        public float GroundFriction = 8.0f;
        [Tooltip("Speed at/below which friction uses this minimum")]
        public float StopSpeed = 2.5f;

        [Tooltip("Max horizontal speed in air (not a hard clamp; air-accel is the limiter)")]
        public float MaxSpeedInAir = 7.0f;
        [Tooltip("Air acceleration (strafing). Higher = easier to gain speed in air")]
        public float AirAcceleration = 12.0f;
        [Tooltip("How much control you have to turn in air (Q3 air control feel)")]
        [Range(0f, 1f)] public float AirControl = 0.3f;

        [Tooltip("Multiplicator for the sprint speed (based on grounded speed)")]
        public float SprintSpeedModifier = 1.5f;

        [Tooltip("Height at which the player dies instantly when falling off the map")]
        public float KillHeight = -50f;

        [Header("Rotation")]
        [Tooltip("Rotation speed for moving the camera")]
        public float RotationSpeed = 200f;

        [Range(0.1f, 1f)]
        [Tooltip("Rotation speed multiplier when aiming")]
        public float AimingRotationMultiplier = 0.4f;

        [Header("Jump")]
        [Tooltip("Force applied upward when jumping")]
        public float JumpForce = 8.0f;

        [Header("Stance")]
        [Tooltip("Ratio (0-1) of the character height where the camera will be at")]
        public float CameraHeightRatio = 0.9f;

        [Tooltip("Height of character when standing")]
        public float CapsuleHeightStanding = 1.8f;

        [Tooltip("Height of character when crouching")]
        public float CapsuleHeightCrouching = 0.9f;

        [Tooltip("Speed of crouching transitions")]
        public float CrouchingSharpness = 10f;

        [Header("Crouch Move")]
        [Tooltip("Max movement speed when crouching")]
        [Range(0, 1)]
        public float MaxSpeedCrouchedRatio = 0.55f;

        [Header("Audio")]
        [Tooltip("Amount of footstep sounds played when moving one meter")]
        public float FootstepSfxFrequency = 1.1f;

        [Tooltip("Amount of footstep sounds played when moving one meter while sprinting")]
        public float FootstepSfxFrequencyWhileSprinting = 1.4f;

        [Tooltip("Sound played for footsteps")]
        public AudioClip FootstepSfx;

        [Tooltip("Sound played when jumping")] public AudioClip JumpSfx;
        [Tooltip("Sound played when landing")] public AudioClip LandSfx;

        [Tooltip("Sound played when taking damage from a fall")]
        public AudioClip FallDamageSfx;

        [Header("Fall Damage")]
        [Tooltip("Whether the player will recieve damage when hitting the ground at high speed")]
        public bool RecievesFallDamage;

        [Tooltip("Minimun fall speed for recieving fall damage")]
        public float MinSpeedForFallDamage = 10f;

        [Tooltip("Fall speed for recieving the maximum amount of fall damage")]
        public float MaxSpeedForFallDamage = 30f;

        [Tooltip("Damage recieved when falling at the mimimum speed")]
        public float FallDamageAtMinSpeed = 10f;

        [Tooltip("Damage recieved when falling at the maximum speed")]
        public float FallDamageAtMaxSpeed = 50f;

        [Tooltip("Si está activo, mantener SPACE salta automáticamente al tocar el suelo (auto-bhop)")]
        public bool AutoBunnyHop = true;

        public UnityAction<bool> OnStanceChanged;

        public Vector3 CharacterVelocity { get; set; }
        public bool IsGrounded { get; private set; }
        public bool HasJumpedThisFrame { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsCrouching { get; private set; }

        public float RotationMultiplier
        {
            get
            {
                if (m_WeaponsManager != null && m_WeaponsManager.IsAiming)
                    return AimingRotationMultiplier;
                return 1f;
            }
        }

        Health m_Health;
        PlayerInputHandler m_InputHandler;
        CharacterController m_Controller;
        PlayerWeaponsManager m_WeaponsManager;
        Actor m_Actor;
        Vector3 m_GroundNormal = Vector3.up;
        Vector3 m_LatestImpactSpeed;
        float m_LastTimeJumped = 0f;
        float m_CameraVerticalAngle = 0f;
        float m_FootstepDistanceCounter;
        float m_TargetCharacterHeight;

        const float k_JumpGroundingPreventionTime = 0.2f;
        const float k_GroundCheckDistanceInAir = 0.07f;

        void Awake()
        {
            ActorsManager actorsManager = FindFirstObjectByType<ActorsManager>();
            if (actorsManager != null) actorsManager.SetPlayer(gameObject);
        }

        void Start()
        {
            m_Controller = GetComponent<CharacterController>();
            DebugUtility.HandleErrorIfNullGetComponent<CharacterController, PlayerCharacterController>(m_Controller, this, gameObject);

            m_InputHandler = GetComponent<PlayerInputHandler>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandler, PlayerCharacterController>(m_InputHandler, this, gameObject);

            m_WeaponsManager = GetComponent<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerWeaponsManager, PlayerCharacterController>(m_WeaponsManager, this, gameObject);

            m_Health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, PlayerCharacterController>(m_Health, this, gameObject);

            m_Actor = GetComponent<Actor>();
            DebugUtility.HandleErrorIfNullGetComponent<Actor, PlayerCharacterController>(m_Actor, this, gameObject);

            m_Controller.enableOverlapRecovery = true;
            m_Health.OnDie += OnDie;

            m_Health.OnDamaged += OnPLayerDamaged;

            SetCrouchingState(false, true);
            UpdateCharacterHeight(true);
        }

        void Update()
        {
            if (!IsDead && transform.position.y < KillHeight)
                m_Health.Kill();

            HasJumpedThisFrame = false;
            bool wasGrounded = IsGrounded;

            GroundCheck();

            // Landing SFX + fall damage
            if (IsGrounded && !wasGrounded)
            {
                float fallSpeed = -Mathf.Min(CharacterVelocity.y, m_LatestImpactSpeed.y);
                float fallSpeedRatio = (fallSpeed - MinSpeedForFallDamage) / (MaxSpeedForFallDamage - MinSpeedForFallDamage);
                if (RecievesFallDamage && fallSpeedRatio > 0f)
                {
                    float dmgFromFall = Mathf.Lerp(FallDamageAtMinSpeed, FallDamageAtMaxSpeed, fallSpeedRatio);
                    float dmgMul = (GameplayModifiers.I != null) ? GameplayModifiers.I.DamageTakenMultiplier : 1f;
                    m_Health.TakeDamage(dmgFromFall * dmgMul, null);
                    AudioSource.PlayOneShot(FallDamageSfx);
                }
                else
                {
                    AudioSource.PlayOneShot(LandSfx);
                }
            }

            if (m_InputHandler.GetCrouchInputDown())
                SetCrouchingState(!IsCrouching, false);

            UpdateCharacterHeight(false);

            HandleCharacterMovement();
        }

        void OnDie()
        {
            IsDead = true;
            m_WeaponsManager.SwitchToWeaponIndex(-1, true);
            EventManager.Broadcast(Events.PlayerDeathEvent);
        }

        void GroundCheck()
        {
            float chosenGroundCheckDistance = IsGrounded ? (m_Controller.skinWidth + GroundCheckDistance) : k_GroundCheckDistanceInAir;
            IsGrounded = false;
            m_GroundNormal = Vector3.up;

            if (Time.time >= m_LastTimeJumped + k_JumpGroundingPreventionTime)
            {
                if (Physics.CapsuleCast(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(m_Controller.height),
                    m_Controller.radius, Vector3.down, out RaycastHit hit, chosenGroundCheckDistance, GroundCheckLayers,
                    QueryTriggerInteraction.Ignore))
                {
                    m_GroundNormal = hit.normal;
                    if (Vector3.Dot(hit.normal, transform.up) > 0f && IsNormalUnderSlopeLimit(m_GroundNormal))
                    {
                        IsGrounded = true;
                        if (hit.distance > m_Controller.skinWidth)
                            m_Controller.Move(Vector3.down * hit.distance);
                    }
                }
            }
        }

        void HandleCharacterMovement()
        {
            // Horizontal rotation (yaw)
            transform.Rotate(new Vector3(0f, (m_InputHandler.GetLookInputsHorizontal() * RotationSpeed * RotationMultiplier), 0f), Space.Self);

            // Vertical camera rotation (pitch)
            m_CameraVerticalAngle += m_InputHandler.GetLookInputsVertical() * RotationSpeed * RotationMultiplier;
            m_CameraVerticalAngle = Mathf.Clamp(m_CameraVerticalAngle, -89f, 89f);
            PlayerCamera.transform.localEulerAngles = new Vector3(m_CameraVerticalAngle, 0, 0);

            // Input as worldspace (relative to yaw)
            Vector3 wishMove = transform.TransformVector(m_InputHandler.GetMoveInput()); // x,z in local -> world
            wishMove.y = 0f;

            bool isSprinting = m_InputHandler.GetSprintInputHeld();
            if (isSprinting) isSprinting = SetCrouchingState(false, false);

            float speedModifier = isSprinting ? SprintSpeedModifier : 1f;

            // Separate horizontal & vertical
            Vector3 vel = CharacterVelocity;
            Vector3 horizVel = Vector3.ProjectOnPlane(vel, Vector3.up);
            float verticalVel = vel.y;

            if (IsGrounded)
            {
                // Friction (only horizontal)
                horizVel = ApplyGroundFriction(horizVel);

                // Build target wish
                Vector3 wishDir = wishMove.normalized;
                float wishSpeed = MaxSpeedOnGround * speedModifier;
                if (IsCrouching) wishSpeed *= MaxSpeedCrouchedRatio;

                // Reorient along slope
                if (wishDir.sqrMagnitude > 0f)
                    wishDir = GetDirectionReorientedOnSlope(wishDir, m_GroundNormal);

                // Accelerate towards wish
                horizVel = Accelerate(horizVel, wishDir, wishSpeed, GroundAcceleration);

                // Jump: preserve horizontal speed (bunnyhop feel)
                bool wantJump = AutoBunnyHop ? m_InputHandler.GetJumpInputHeld()
                             : m_InputHandler.GetJumpInputDown();

                if (wantJump)
                {
                    float jMul = (GameplayModifiers.I != null) ? GameplayModifiers.I.JumpHeightMultiplier : 1f;
                    verticalVel = JumpForce * jMul;
                    AudioSource.PlayOneShot(JumpSfx);
                    m_LastTimeJumped = Time.time;
                    HasJumpedThisFrame = true;
                    IsGrounded = false;
                    m_GroundNormal = Vector3.up;
                }
                else
                {
                    // Stick to ground slightly
                    verticalVel = Mathf.Min(verticalVel, 0f);
                }

                // Footsteps
                float chosenFootstepSfxFrequency = isSprinting ? FootstepSfxFrequencyWhileSprinting : FootstepSfxFrequency;
                if (m_FootstepDistanceCounter >= 1f / chosenFootstepSfxFrequency)
                {
                    m_FootstepDistanceCounter = 0f;
                    if (horizVel.sqrMagnitude > 0.01f) AudioSource.PlayOneShot(FootstepSfx);
                }
                m_FootstepDistanceCounter += horizVel.magnitude * Time.deltaTime;
            }
            else
            {
                // Gravity
                verticalVel -= GravityDownForce * Time.deltaTime;

                // Air movement
                Vector3 wishDir = wishMove.normalized;
                float wishSpeed = MaxSpeedInAir * speedModifier;

                if (wishDir.sqrMagnitude > 0f)
                {
                    horizVel = AirAccelerateFunc(horizVel, wishDir, wishSpeed, AirAcceleration);

                    // Optional air control (turning in air)
                    horizVel = ApplyAirControl(horizVel, wishDir, wishSpeed, AirControl);
                }
            }

            // Recompose velocity
            CharacterVelocity = horizVel + Vector3.up * verticalVel;

            // Move controller
            Vector3 capsuleBottomBeforeMove = GetCapsuleBottomHemisphere();
            Vector3 capsuleTopBeforeMove = GetCapsuleTopHemisphere(m_Controller.height);
            m_Controller.Move(CharacterVelocity * Time.deltaTime);

            // Collision slide
            m_LatestImpactSpeed = Vector3.zero;
            if (Physics.CapsuleCast(capsuleBottomBeforeMove, capsuleTopBeforeMove, m_Controller.radius,
                CharacterVelocity.normalized, out RaycastHit hit, CharacterVelocity.magnitude * Time.deltaTime, -1,
                QueryTriggerInteraction.Ignore))
            {
                m_LatestImpactSpeed = CharacterVelocity;
                CharacterVelocity = Vector3.ProjectOnPlane(CharacterVelocity, hit.normal);
            }
        }

        // --- Quake helpers ---

        Vector3 ApplyGroundFriction(Vector3 horizVel)
        {
            float speed = horizVel.magnitude;
            if (speed < 0.0001f) return Vector3.zero;

            float control = Mathf.Max(speed, StopSpeed);
            float drop = control * GroundFriction * Time.deltaTime;

            float newSpeed = Mathf.Max(speed - drop, 0f);
            if (newSpeed != speed)
            {
                newSpeed /= speed;
                horizVel *= newSpeed;
            }
            return horizVel;
        }

        Vector3 Accelerate(Vector3 current, Vector3 wishDir, float wishSpeed, float accel)
        {
            if (wishDir.sqrMagnitude < 0.0001f) return current;

            float currentSpeedInWishDir = Vector3.Dot(current, wishDir);
            float addSpeed = wishSpeed - currentSpeedInWishDir;
            if (addSpeed <= 0f) return current;

            float accelSpeed = accel * Time.deltaTime * wishSpeed;
            if (accelSpeed > addSpeed) accelSpeed = addSpeed;

            return current + wishDir * accelSpeed;
        }

        Vector3 AirAccelerateFunc(Vector3 current, Vector3 wishDir, float wishSpeed, float accel)
        {
            float currentSpeedInWishDir = Vector3.Dot(current, wishDir);
            float addSpeed = wishSpeed - currentSpeedInWishDir;
            if (addSpeed <= 0f) return current;

            // Q3-ish: accel scales with wishSpeed
            float accelSpeed = accel * wishSpeed * Time.deltaTime;
            if (accelSpeed > addSpeed) accelSpeed = addSpeed;

            return current + wishDir * accelSpeed;
        }

        Vector3 ApplyAirControl(Vector3 current, Vector3 wishDir, float wishSpeed, float airControl)
        {
            if (airControl <= 0f) return current;
            float speed = current.magnitude;
            if (speed < 0.0001f) return current;

            float dot = Vector3.Dot(current.normalized, wishDir);
            // Only if trying to turn (dot > 0 gives "forward" air control feeling)
            if (dot > 0f)
            {
                // steer towards wishDir
                Vector3 perp = (wishDir - current.normalized * dot).normalized;
                current += perp * (airControl * dot * speed * Time.deltaTime);
            }
            return current;
        }

        // --- Utility & stance (mostly unchanged) ---

        bool IsNormalUnderSlopeLimit(Vector3 normal)
        {
            return Vector3.Angle(transform.up, normal) <= m_Controller.slopeLimit;
        }

        Vector3 GetCapsuleBottomHemisphere()
        {
            return transform.position + (transform.up * m_Controller.radius);
        }

        Vector3 GetCapsuleTopHemisphere(float atHeight)
        {
            return transform.position + (transform.up * (atHeight - m_Controller.radius));
        }

        public Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
        {
            Vector3 directionRight = Vector3.Cross(direction, transform.up);
            return Vector3.Cross(slopeNormal, directionRight).normalized;
        }

        void UpdateCharacterHeight(bool force)
        {
            if (force)
            {
                m_Controller.height = m_TargetCharacterHeight;
                m_Controller.center = Vector3.up * m_Controller.height * 0.5f;
                PlayerCamera.transform.localPosition = Vector3.up * m_TargetCharacterHeight * CameraHeightRatio;
                m_Actor.AimPoint.transform.localPosition = m_Controller.center;
            }
            else if (m_Controller.height != m_TargetCharacterHeight)
            {
                m_Controller.height = Mathf.Lerp(m_Controller.height, m_TargetCharacterHeight, CrouchingSharpness * Time.deltaTime);
                m_Controller.center = Vector3.up * m_Controller.height * 0.5f;
                PlayerCamera.transform.localPosition = Vector3.Lerp(PlayerCamera.transform.localPosition,
                    Vector3.up * m_TargetCharacterHeight * CameraHeightRatio, CrouchingSharpness * Time.deltaTime);
                m_Actor.AimPoint.transform.localPosition = m_Controller.center;
            }
        }

        bool SetCrouchingState(bool crouched, bool ignoreObstructions)
        {
            if (crouched)
            {
                m_TargetCharacterHeight = CapsuleHeightCrouching;
            }
            else
            {
                if (!ignoreObstructions)
                {
                    Collider[] standingOverlaps = Physics.OverlapCapsule(
                        GetCapsuleBottomHemisphere(),
                        GetCapsuleTopHemisphere(CapsuleHeightStanding),
                        m_Controller.radius,
                        -1,
                        QueryTriggerInteraction.Ignore);
                    foreach (Collider c in standingOverlaps)
                    {
                        if (c != m_Controller)
                            return false;
                    }
                }
                m_TargetCharacterHeight = CapsuleHeightStanding;
            }

            OnStanceChanged?.Invoke(crouched);
            IsCrouching = crouched;
            return true; 
        }

        void OnPLayerDamaged(float damageAmount, GameObject damageSource)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.RemoveScore(ScoreManager.Instance.scoreLostOnHit);
            }
        }

        public void AddImpulse(Vector3 impulse)
        {
            // Apply impulse directly to the character velocity
            CharacterVelocity += impulse;

            // Prevent immediate re-grounding (gives the impulse time to take effect)
            m_LastTimeJumped = Time.time;
            IsGrounded = false;
            HasJumpedThisFrame = true;

            // record latest impact speed so landing/fall-damage logic has a reference if needed
            m_LatestImpactSpeed = CharacterVelocity;
        }
    }
}

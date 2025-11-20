using UnityEngine;
using Photon.Pun;
using UnityEngine.Rendering;

public class PlayerAnimationController : MonoBehaviourPun
{
    [Header("References")]
    public Animator animator;                  // Animator del modelo (X Bot)
    public CharacterController cc;             // CharacterController del Player

    [Header("Ocultar cuerpo local (FPS)")]
    public SkinnedMeshRenderer[] meshesToHide; // Beta_Surface

    [Header("Animación direccional simple")]
    [Tooltip("Velocidad máxima horizontal del player (para normalizar Forward/Strafe entre -1 y 1).")]
    public float maxMoveSpeed = 6f;

    void Start()
    {
        // Ocultar el cuerpo SOLO en el player local
        if (photonView.IsMine)
        {
            foreach (var mesh in meshesToHide)
            {
                if (mesh == null) continue;

                // Opción B: ocultar modelo pero dejar sombras
                mesh.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
        }
    }

    void Update()
    {
        // Solo el dueño del Player actualiza parámetros
        if (!photonView.IsMine) return;
        if (animator == null || cc == null) return;

        // Velocidad horizontal en mundo
        Vector3 horizontal = new Vector3(cc.velocity.x, 0f, cc.velocity.z);
        float speed = horizontal.magnitude;

        // Velocidad en espacio LOCAL del player
        Vector3 localVel = transform.InverseTransformDirection(horizontal);

        // forward = adelante (+) / atrás (-)
        float forward = localVel.z;

        // strafe = derecha (+) / izquierda (-)
        float strafe = localVel.x;

        // Normalizar para animaciones (-1 a 1 aprox)
        if (maxMoveSpeed > 0.01f)
        {
            forward = Mathf.Clamp(forward / maxMoveSpeed, -1f, 1f);
            strafe = Mathf.Clamp(strafe / maxMoveSpeed, -1f, 1f);
        }

        // Enviar parámetros al Animator
        animator.SetFloat("Speed", speed);
        animator.SetFloat("Forward", forward);
        animator.SetFloat("Strafe", strafe);
        animator.SetBool("IsGrounded", cc.isGrounded);
    }
}

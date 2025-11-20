using UnityEngine;
using Photon.Pun;
using UnityEngine.Rendering;

public class PlayerAnimationController : MonoBehaviourPun
{
    [Header("References")]
    public Animator animator;
    public CharacterController cc;

    [Header("Ocultar cuerpo local (FPS)")]
    public SkinnedMeshRenderer[] meshesToHide;

    void Start()
    {
        // Si este player es el local → ocultamos su cuerpo
        if (photonView.IsMine)
        {
            foreach (var mesh in meshesToHide)
            {
                if (mesh == null) continue;

                // Opción A: ocultar completamente el cuerpo
                 mesh.enabled = false;

                // Opción B: el mesh sigue existiendo pero solo proyecta sombras
                //mesh.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
        }
    }

    void Update()
    {
        // Solo el dueño actualiza sus animaciones
        if (!photonView.IsMine) return;

        // Calcular velocidad horizontal
        Vector3 horizontal = new Vector3(cc.velocity.x, 0, cc.velocity.z);
        float speed = horizontal.magnitude;

        // Pasar datos al Animator
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsGrounded", cc.isGrounded);
    }
}

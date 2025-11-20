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
        ApplyLocalBodyVisibility();
    }

    void OnEnable()
    {
        ApplyLocalBodyVisibility();
    }

    void ApplyLocalBodyVisibility()
    {
        bool isLocal = photonView.IsMine;

        foreach (var mesh in meshesToHide)
        {
            if (mesh == null) continue;

            mesh.enabled = !isLocal;

        }
    }

    void Update()
    {
        // el locacl actualiza sus animaciones
        if (!photonView.IsMine) return;
        if (animator == null || cc == null) return;

        //velocidad horizontal
        Vector3 horizontal = new Vector3(cc.velocity.x, 0, cc.velocity.z);
        float speed = horizontal.magnitude;

        animator.SetFloat("Speed", speed);
        animator.SetBool("IsGrounded", cc.isGrounded);
    }
}
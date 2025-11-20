using UnityEngine;
using Photon.Pun;
using UnityEngine.Rendering;

namespace Unity.FPS.Game
{
    public class PlayerAnimationController : MonoBehaviourPun
    {
        [Header("References")]
        public Animator animator;
        public CharacterController cc;

        [Header("Ocultar cuerpo local (FPS)")]
        public SkinnedMeshRenderer[] meshesToHide;

        void Start()
        {
            // Lo llamamos al inicio
            ApplyLocalBodyVisibility();
        }

        // Usamos LateUpdate para "ganarle" a cualquier otro script que intente
        // activar las meshes al revivir.
        void LateUpdate()
        {
            // Solo nos importa si es MI jugador local
            if (!photonView.IsMine) return;

            // Forzamos que se oculten en cada frame antes de renderizar
            ForceHideLocalMeshes();

            // Actualizar animaciones (si es necesario hacerlo aquí o en Update)
            UpdateAnimations();
        }

        void ForceHideLocalMeshes()
        {
            foreach (var mesh in meshesToHide)
            {
                if (mesh == null) continue;


                if (mesh.enabled)
                {
                    mesh.enabled = false;


                }
            }
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

      
        void UpdateAnimations()
        {
            if (animator == null || cc == null) return;

 
            Vector3 horizontal = new Vector3(cc.velocity.x, 0, cc.velocity.z);
            float speed = horizontal.magnitude;

            animator.SetFloat("Speed", speed);
            animator.SetBool("IsGrounded", cc.isGrounded);
        }
    }
}
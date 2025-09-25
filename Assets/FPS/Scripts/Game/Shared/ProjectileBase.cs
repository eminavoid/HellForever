using UnityEngine;
using UnityEngine.Events;
using Unity.FPS.Ours;
using Photon.Pun;

namespace Unity.FPS.Game
{
    [RequireComponent(typeof(PhotonView))]
    public abstract class ProjectileBase : MonoBehaviourPun, IPunInstantiateMagicCallback
    {
        public GameObject Owner { get; private set; }
        public Vector3 InitialPosition { get; private set; }
        public Vector3 InitialDirection { get; private set; }
        public Vector3 InheritedMuzzleVelocity { get; private set; }
        public float InitialCharge { get; private set; }

        // Dirección que viaja por red
        protected Vector3 NetworkedDirection { get; private set; }

        public UnityAction OnShoot;

        public void Shoot(WeaponController controller)
        {
            Owner = controller.Owner;
            InitialPosition = transform.position;
            InitialDirection = transform.forward;
            InheritedMuzzleVelocity = controller.MuzzleWorldVelocity;
            InitialCharge = controller.CurrentCharge;

            if (NetworkedDirection == Vector3.zero)
                NetworkedDirection = InitialDirection;

            OnShoot?.Invoke();
        }

        public void SetDirection(Vector3 dir) => NetworkedDirection = dir.normalized;

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            var data = info.photonView?.InstantiationData;
            if (data != null && data.Length >= 3)
            {
                NetworkedDirection = new Vector3(
                    (float)data[0], (float)data[1], (float)data[2]
                ).normalized;
            }
        }
    }
}

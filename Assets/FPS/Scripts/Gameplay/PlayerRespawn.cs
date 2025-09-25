using UnityEngine;
using Photon.Pun;

namespace Unity.FPS.Gameplay
{
    public class PlayerRespawn : MonoBehaviourPun
    {
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;

        void Start()
        {
            if (photonView.IsMine)
            {
                spawnPosition = transform.position;
                spawnRotation = transform.rotation;
            }
        }

        public Vector3 GetSpawnPosition() => spawnPosition;
        public Quaternion GetSpawnRotation() => spawnRotation;
    }
}

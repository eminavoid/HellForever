using UnityEngine;

namespace Unity.FPS.Ours
{
    [RequireComponent(typeof(Collider))]
    public class PowerupPickup : MonoBehaviour
    {
        public PowerupBase Powerup; // assign the ScriptableObject in Inspector

        void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;

            var rb = GetComponent<Rigidbody>();
            if (!rb) rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        void OnTriggerEnter(Collider other) => TryGive(other);
        void OnTriggerStay(Collider other) => TryGive(other);

        void TryGive(Collider other)
        {
            var runner = other.GetComponent<EntityQueueRunner>();
            if (runner != null && Powerup != null)
            {
                runner.Enqueue(new PowerupTaskAdapter(Powerup));
                Destroy(gameObject);
            }
        }
        
    }
}

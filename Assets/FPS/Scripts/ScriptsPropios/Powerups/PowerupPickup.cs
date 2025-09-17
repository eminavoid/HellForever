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
        }

        void OnTriggerEnter(Collider other)
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

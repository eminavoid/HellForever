using UnityEngine;

namespace Unity.FPS.Ours
{
    [RequireComponent(typeof(Collider))]
    public class PowerupPickup : MonoBehaviour
    {
        public PowerupBase Powerup;

        void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
            var rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; rb.useGravity = false;
        }

        void OnTriggerEnter(Collider other) => TryGive(other);
        void OnTriggerStay(Collider other) => TryGive(other);

        void TryGive(Collider other)
        {
            var runner = other.GetComponent<PowerupStackRunner>();
            if (runner != null && Powerup != null)
            {
                runner.Push(new PowerupTaskAdapter(Powerup));
                Destroy(gameObject);
            }
        }
    }

}

using UnityEngine;

namespace Unity.FPS.Game
{
    [RequireComponent(typeof(Collider))]
    public class WeaponPickup : MonoBehaviour
    {
        public string WeaponId;           // must match prefab's WeaponId
        public float AmmoOnPickup = 30f;  // default 30

        void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
            var rb = GetComponent<Rigidbody>();
            if (!rb) rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        void OnTriggerEnter(Collider other)
        {
            var stack = other.GetComponent<WeaponStackManager>();
            if (!stack) return;

            stack.OnPickupWeapon(WeaponId, AmmoOnPickup);
            Destroy(gameObject);
        }

        void OnTriggerStay(Collider other) => OnTriggerEnter(other);
    }
}

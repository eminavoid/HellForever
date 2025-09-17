using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using Unity.FPS.Ours;

namespace Unity.FPS.Game
{
    [DisallowMultipleComponent]
    public class WeaponStackManager : MonoBehaviour
    {
        [Header("References")]
        public WeaponController StarterWeapon;
        public List<WeaponController> WeaponPrefabs = new List<WeaponController>();

        public float PickupAmmoAmount = 30f;
        public int BigClipMaxAmmo = 99999;

        GenericLinkedStack<WeaponController> _stack = new GenericLinkedStack<WeaponController>();
        Dictionary<string, WeaponController> _byId = new Dictionary<string, WeaponController>();
        WeaponController _current;

        void Start()
        {
            StarterWeapon.AutomaticReload = true;
            _current = StarterWeapon;
            if (!_byId.ContainsKey(StarterWeapon.WeaponId))
                _byId.Add(StarterWeapon.WeaponId, StarterWeapon);

            UpdateActiveVisibility();
        }

        public void OnPickupWeapon(string weaponId, float addAmmo)
        {
            if (weaponId == StarterWeapon.WeaponId)
            {
                StarterWeapon.AddAmmo(addAmmo);
                ForceEquip(StarterWeapon);
                return;
            }

            if (_byId.TryGetValue(weaponId, out var existing))
            {
                existing.AddAmmo(addAmmo);
                _stack.TryRemove(w => w == existing, out _);
                _stack.Push(existing);
                ForceEquip(existing);
                return;
            }

            var prefab = WeaponPrefabs.Find(w => w.WeaponId == weaponId);
            if (!prefab) return;

            var instance = Instantiate(prefab, StarterWeapon.transform.parent);
            instance.AutomaticReload = false;
            instance.MaxAmmo = BigClipMaxAmmo;
            instance.SetAmmo(addAmmo);

            _byId[weaponId] = instance;
            _stack.Push(instance);
            ForceEquip(instance);
        }

        void ForceEquip(WeaponController w)
        {
            _current = w;
            UpdateActiveVisibility();
        }

        void UpdateActiveVisibility()
        {
            foreach (var wc in GetComponentsInChildren<WeaponController>(true))
                wc.gameObject.SetActive(wc == _current);
        }

        void Update()
        {
            if (_current == StarterWeapon) return;
            if (_current.CurrentAmmo <= 0.01f)
            {
                _stack.TryPop(out var consumed);
                if (_stack.TryPeek(out var next))
                    _current = next;
                else
                    _current = StarterWeapon;

                UpdateActiveVisibility();
            }
        }
    }
}

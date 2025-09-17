using UnityEngine;

namespace Unity.FPS.Ours
{
    public abstract class PowerupBase : ScriptableObject
    {
        [Tooltip("Seconds the effect remains active")]
        public float Duration = 10f;

        protected GameObject _target;
        public void Init(GameObject target) => _target = target;

        public abstract void Apply();
        public abstract void Revert();
    }
}

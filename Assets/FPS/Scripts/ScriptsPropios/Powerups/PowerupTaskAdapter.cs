using System.Collections;
using UnityEngine;

namespace Unity.FPS.Ours
{
    public class PowerupTaskAdapter : IQueueTask
    {
        readonly PowerupBase _powerup;

        public PowerupTaskAdapter(PowerupBase powerup)
        {
            // Instance so we don't mutate the asset
            _powerup = Object.Instantiate(powerup);
        }

        public IEnumerator Run(GameObject target)
        {
            _powerup.Init(target);
            _powerup.Apply();

            float t = _powerup.Duration;
            while (t > 0f)
            {
                t -= Time.deltaTime;
                yield return null;
            }

            _powerup.Revert();
        }
    }
}

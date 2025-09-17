using System.Collections;
using UnityEngine;

namespace Unity.FPS.Ours
{
    public interface IQueueTask
    {
        IEnumerator Run(GameObject target);
    }
}
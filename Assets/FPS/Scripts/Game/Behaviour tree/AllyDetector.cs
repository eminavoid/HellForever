using UnityEngine;

public class AllyDetector : MonoBehaviour
{
    public string targetTag = "Enemy";

    public bool TargetDetected { get; private set; } = false;

    public Transform DetectedObject { get; private set; }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag(targetTag))
        {
            TargetDetected = true;
            DetectedObject = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            TargetDetected = false;
            DetectedObject = null;
        }
    }
}
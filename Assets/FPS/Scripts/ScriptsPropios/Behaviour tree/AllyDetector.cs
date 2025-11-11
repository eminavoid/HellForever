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
            Debug.Log(gameObject.name + " detectó a: " + other.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            TargetDetected = false;
            DetectedObject = null;
            Debug.Log(gameObject.name + " perdió a: " + other.name);
        }
    }
}
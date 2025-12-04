using Photon.Pun;
using Unity.FPS.Game;
using UnityEngine;

public class DeathCamera : MonoBehaviourPun
{
    [Header("Camera")]
    public Transform cameraTransform;

    [Header("Offsets de muerte")]
    public Vector3 deathOffset = new Vector3(0, 2f, -4f);
    public float deathFOV = 50f;

    private Vector3 originalLocalPos;
    private float originalFOV;

    private Camera cam;
    private Health health;

    void Start()
    {
        if (!photonView.IsMine)
        {
            enabled = false;
            return;
        }

        cam = cameraTransform.GetComponent<Camera>();
        health = GetComponent<Health>();

        originalLocalPos = cameraTransform.localPosition;
        originalFOV = cam.fieldOfView;


        health.OnDie += OnPlayerDie;
        health.OnHealed += _ => OnPlayerRespawn();
    }

    void OnPlayerDie()
    {
        cameraTransform.localPosition = deathOffset;
        cam.fieldOfView = deathFOV;
    }

    void OnPlayerRespawn()
    {
        cameraTransform.localPosition = originalLocalPos;
        cam.fieldOfView = originalFOV;
    }
}

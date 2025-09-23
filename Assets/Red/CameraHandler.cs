using UnityEngine;
using Photon.Pun;

public class CameraHandler : MonoBehaviourPun
{
    void Start()
    {
        if (!photonView.IsMine)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cam.enabled = false;

            AudioListener audio = GetComponentInChildren<AudioListener>();
            if (audio != null) audio.enabled = false;
        }
    }
}
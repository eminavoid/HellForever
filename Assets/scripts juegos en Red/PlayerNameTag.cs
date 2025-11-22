using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerNameTag : MonoBehaviourPun
{
    [SerializeField] private TMP_Text nameText;
    private Transform cam;

    void Start()
    {
        if (nameText != null)
            nameText.text = photonView.Owner.NickName;

        if (Camera.main != null)
            cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;

        if (cam != null && nameText != null)
        {
            nameText.transform.rotation = Quaternion.LookRotation(
                cam.forward,
                Vector3.up
            );
        }
    }
}

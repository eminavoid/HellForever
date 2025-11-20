using Photon.Pun;
using UnityEngine;

public class SyncLocalRotation : MonoBehaviourPun, IPunObservable
{
    Quaternion target;

    void Awake()
    {
        target = transform.localRotation;
    }

    void LateUpdate()
    {
        if (!photonView.IsMine)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation, target, 20f * Time.deltaTime);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.localRotation);
        }
        else
        {
            target = (Quaternion)stream.ReceiveNext();
        }
    }
}

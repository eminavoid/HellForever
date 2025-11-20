using UnityEngine;
using Photon.Pun;

public class EnemyTurretSync : MonoBehaviourPun, IPunObservable
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí el objeto Turret_Pivot_C_Jnt")]
    public Transform turretPivot;

    [Header("Configuración")]
    public float smoothing = 10f;           

    private Quaternion networkRotation;

    void Awake()
    {
        if (turretPivot != null)
        {
            networkRotation = turretPivot.localRotation;
        }
    }

    void Update()
    {
        if (!photonView.IsMine && turretPivot != null)
        {
            turretPivot.localRotation = Quaternion.Lerp(turretPivot.localRotation, networkRotation, Time.deltaTime * smoothing);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(turretPivot.localRotation);
        }
        else
        {
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
using UnityEngine;
using Photon.Pun;
using System;

public class CoOpButton : MonoBehaviour
{
    [Header("Configuración")]
    public LaserDoor targetDoor;
    [Range(1, 2)]
    public int buttonID = 1;

    [Header("Interacción")]
    public KeyCode interactKey = KeyCode.E;     
    public GameObject interactPrompt;      

    private bool isPlayerInZone = false;

    private float lastPressLocalTime = 0;
    private float buttonCooldown = 0.5f;

    private void Start()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        PhotonView pView = other.GetComponent<PhotonView>();
        if (other.CompareTag("Player") && pView != null && pView.IsMine)
        {
            isPlayerInZone = true;
            if (interactPrompt != null) interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PhotonView pView = other.GetComponent<PhotonView>();
        if (other.CompareTag("Player") && pView != null && pView.IsMine)
        {
            isPlayerInZone = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (isPlayerInZone)
        {
            if (Input.GetKey(interactKey))
            {
                if (Time.time - lastPressLocalTime > buttonCooldown)
                {
                    PressButton();
                    lastPressLocalTime = Time.time;
                }
            }
        }
    }

    private void PressButton()
    {
        if (targetDoor != null)
        {
            targetDoor.photonView.RPC(nameof(LaserDoor.RPC_RegisteButtonPress), RpcTarget.All, buttonID, PhotonNetwork.Time);
        }
    }
}
using UnityEngine;
using Photon.Pun;
using TMPro; 

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;

    public void OnConnectButton()
    {
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            PhotonNetwork.NickName = nameInput.text; 
            PhotonNetwork.ConnectUsingSettings();
        }
    }
}

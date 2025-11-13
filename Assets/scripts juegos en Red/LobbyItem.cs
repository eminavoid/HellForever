using UnityEngine;
using UnityEngine.UI;
using TMPro;    

public class LobbyItem : MonoBehaviour
{
    public TMP_Text roomInfoText;   
    public Button joinButton;

    private LobbyManager lobbyManager;
    private string roomName;

    public void Initialize(LobbyManager manager, string photonRoomName, string displayText)
    {
        lobbyManager = manager;
        roomName = photonRoomName;
        roomInfoText.text = displayText;

        joinButton.onClick.AddListener(OnJoinClicked);
    }

    private void OnJoinClicked()
    {
        lobbyManager.JoinRoom(roomName);
    }
}
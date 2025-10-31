using UnityEngine;
using UnityEngine.UI;

public class LobbyItem : MonoBehaviour
{
    public Text roomInfoText; // Assign this in the prefab inspector
    public Button joinButton; // Assign this in the prefab inspector

    private LobbyManager lobbyManager; // Reference to our manager
    private string roomName; // The *real* Photon room name

    public void Initialize(LobbyManager manager, string photonRoomName, string displayText)
    {
        lobbyManager = manager;
        roomName = photonRoomName;
        roomInfoText.text = displayText;

        // Add a listener to the button
        joinButton.onClick.AddListener(OnJoinClicked);
    }

    private void OnJoinClicked()
    {
        // Tell the lobby manager to join this room
        lobbyManager.JoinRoom(roomName);
    }
}
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI Panels")]
    public GameObject panel_Connect;
    public GameObject panel_MainMenu;
    public GameObject panel_CreateLobby;
    public GameObject panel_LobbyBrowser;
    public GameObject panel_InLobby;

    [Header("Create Lobby")]
    public TMP_InputField roomNameInput;
    public TMP_Dropdown maxPlayersDropdown;

    [Header("Lobby Browser")]
    public ScrollRect lobbyScrollView;
    public GameObject lobbyItemPrefab;
    private Transform lobbyItemParent;
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    [Header("In Lobby")]
    public TMP_Text playerListText;
    public string gameSceneName;

    void Start()
    {
        lobbyItemParent = lobbyScrollView.content;
        ActivatePanel(panel_Connect);
        PhotonNetwork.ConnectUsingSettings();
    }

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        cachedRoomList.Clear();
        ActivatePanel(panel_MainMenu);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                cachedRoomList.Remove(room.Name);
            }
            else
            {
                cachedRoomList[room.Name] = room;
            }
        }

        UpdateLobbyBrowserUI();
    }

    public override void OnJoinedRoom()
    {
        ActivatePanel(panel_InLobby);
        UpdatePlayerListUI();
        CheckIfRoomIsFull();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerListUI();
        CheckIfRoomIsFull();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerListUI();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        ActivatePanel(panel_MainMenu);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        ActivatePanel(panel_MainMenu);
    }

    #endregion

    #region UI Button Clicks

    public void OnClick_ShowCreateLobby()
    {
        ActivatePanel(panel_CreateLobby);
    }

    public void OnClick_ShowLobbyBrowser()
    {
        ActivatePanel(panel_LobbyBrowser);
        UpdateLobbyBrowserUI();
    }

    public void OnClick_BackToMainMenu()
    {
        ActivatePanel(panel_MainMenu);
    }

    public void OnClick_CreateLobby()
    {
        string roomName = roomNameInput.text;
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(1000, 10000);
        }

        string maxPlayersString = maxPlayersDropdown.options[maxPlayersDropdown.value].text;
        byte maxPlayers = byte.Parse(maxPlayersString);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayers;

        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "DisplayName", roomName }
        };

        roomOptions.CustomRoomPropertiesForLobby = new string[] { "DisplayName" };

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    #endregion

    #region Helper Functions

    private void ActivatePanel(GameObject panelToActivate)
    {
        panel_Connect.SetActive(panelToActivate == panel_Connect);
        panel_MainMenu.SetActive(panelToActivate == panel_MainMenu);
        panel_CreateLobby.SetActive(panelToActivate == panel_CreateLobby);
        panel_LobbyBrowser.SetActive(panelToActivate == panel_LobbyBrowser);
        panel_InLobby.SetActive(panelToActivate == panel_InLobby);
    }

    private void UpdateLobbyBrowserUI()
    {
        foreach (Transform child in lobbyItemParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var roomEntry in cachedRoomList)
        {
            RoomInfo info = roomEntry.Value;

            if (!info.IsVisible || !info.IsOpen || info.PlayerCount == 0)
                continue;

            GameObject lobbyItemGO = Instantiate(lobbyItemPrefab, lobbyItemParent);
            LobbyItem lobbyItem = lobbyItemGO.GetComponent<LobbyItem>();

            string displayName = info.Name;
            if (info.CustomProperties.ContainsKey("DisplayName"))
            {
                displayName = (string)info.CustomProperties["DisplayName"];
            }

            string roomInfoText = $"{displayName}   ({info.PlayerCount} / {info.MaxPlayers})";
            lobbyItem.Initialize(this, info.Name, roomInfoText);
        }
    }

    private void UpdatePlayerListUI()
    {
        playerListText.text = "";
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            playerListText.text += (p.IsMasterClient ? "(Host) " : "") + p.NickName + "\n";
        }
    }

    private void CheckIfRoomIsFull()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }

    #endregion
}
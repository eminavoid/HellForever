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

    [Header("Main Menu")]
    public TMP_InputField playerNameInput;

    [Header("Create Lobby")]
    public TMP_InputField roomNameInput;   
    public TMP_Dropdown maxPlayersDropdown;
    public TMP_Dropdown mapSelectorDropdown;

    [Header("Lobby Browser")]
    public ScrollRect lobbyScrollView;
    public GameObject lobbyItemPrefab;
    private Transform lobbyItemParent;
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    [Header("In Lobby")]
    public TMP_Text playerListText;
    public TMP_Text lobbyNameText;
    public string gameSceneName;

    private const string PlayerNamePrefKey = "PlayerName";

    void Start()
    {
        PhotonNetwork.SerializationRate = 30;
        PhotonNetwork.SendRate = 30;

        lobbyItemParent = lobbyScrollView.content;
        ActivatePanel(panel_Connect);

        PhotonNetwork.AutomaticallySyncScene = true;

        string defaultName = PlayerPrefs.GetString(PlayerNamePrefKey, "");
        playerNameInput.text = defaultName;
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

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("DisplayName"))
        {
            lobbyNameText.text = (string)PhotonNetwork.CurrentRoom.CustomProperties["DisplayName"];
        }
        else
        {
            lobbyNameText.text = PhotonNetwork.CurrentRoom.Name;
        }
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

    public override void OnLeftRoom()
    {
        ActivatePanel(panel_MainMenu);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        UpdatePlayerListUI();
    }

#endregion

#region UI Button Clicks

public void OnPlayerNameChanged(string newName)
    {
        PlayerPrefs.SetString(PlayerNamePrefKey, newName);
    }

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
        SetFinalPlayerName();    

        string displayName = roomNameInput.text;
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = "Room " + Random.Range(1000, 10000);
        }

        string maxPlayersString = maxPlayersDropdown.options[maxPlayersDropdown.value].text;
        byte maxPlayers = byte.Parse(maxPlayersString);

        string mapSceneName = mapSelectorDropdown.options[mapSelectorDropdown.value].text;

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayers;

        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "DisplayName", displayName },
            { "MapName", mapSceneName }
        };

        roomOptions.CustomRoomPropertiesForLobby = new string[] { "DisplayName", "MapName" };

        string internalRoomName = "Room_" + System.Guid.NewGuid().ToString();

        PhotonNetwork.CreateRoom(internalRoomName, roomOptions);
    }

    public void JoinRoom(string roomName)
    {
        SetFinalPlayerName();    

        PhotonNetwork.JoinRoom(roomName);
    }

    public void OnClick_LeaveLobby()
    {
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    #region Helper Functions

    private void SetFinalPlayerName()
    {
        string playerName = playerNameInput.text;

        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Player " + Random.Range(100, 1000);
            playerNameInput.text = playerName;        
        }

        PhotonNetwork.NickName = playerName;
        PlayerPrefs.SetString(PlayerNamePrefKey, playerName);
    }

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
            string mapName = "Unknown Map";

            if (info.CustomProperties.ContainsKey("DisplayName"))
                displayName = (string)info.CustomProperties["DisplayName"];

            if (info.CustomProperties.ContainsKey("MapName"))
                mapName = (string)info.CustomProperties["MapName"];

            string roomInfoText = $"{displayName} [{mapName}]   ({info.PlayerCount} / {info.MaxPlayers})";

            lobbyItem.Initialize(this, info.Name, roomInfoText);
        }
    }

    private void UpdatePlayerListUI()
    {
        playerListText.text = "";
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            playerListText.text += (p.IsMasterClient ? "(Host): " : "") + p.NickName + "\n";
        }
    }

    private void CheckIfRoomIsFull()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;


            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("MapName"))
            {
                string mapToLoad = (string)PhotonNetwork.CurrentRoom.CustomProperties["MapName"];
                PhotonNetwork.LoadLevel(mapToLoad);
            }
            else
            {
                Debug.LogError("No se encontró mapa seleccionado, cargando default.");
                PhotonNetwork.LoadLevel("GameScene");
            }
        }
    }

    #endregion
}
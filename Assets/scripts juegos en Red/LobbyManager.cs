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
    public TMP_Text roomInfoTextHeader; // Texto para mostrar el nombre de la sala
    public Button startGameButton; // Botón para iniciar juego (solo Host)
    public string gameSceneName; // Nombre de tu escena de juego

    void Start()
    {
        lobbyItemParent = lobbyScrollView.content;
        ActivatePanel(panel_Connect);
        PhotonNetwork.ConnectUsingSettings();
    }

    // Activa un panel y desactiva los demás
    void ActivatePanel(GameObject panelToActivate)
    {
        panel_Connect.SetActive(panelToActivate == panel_Connect);
        panel_MainMenu.SetActive(panelToActivate == panel_MainMenu);
        panel_CreateLobby.SetActive(panelToActivate == panel_CreateLobby);
        panel_LobbyBrowser.SetActive(panelToActivate == panel_LobbyBrowser);
        panel_InLobby.SetActive(panelToActivate == panel_InLobby);
    }

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true; // Importante
    }

    public override void OnJoinedLobby()
    {
        cachedRoomList.Clear();
        ActivatePanel(panel_MainMenu);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Actualizar la lista cacheada
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }
        UpdateRoomListUI();
    }

    public override void OnJoinedRoom()
    {
        ActivatePanel(panel_InLobby);

        // Configurar la UI de la sala
        roomInfoTextHeader.text = $"Sala: {PhotonNetwork.CurrentRoom.Name}";
        UpdatePlayerListUI();

        // Activar/Desactivar el botón de Start
        if (startGameButton != null)
            startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnLeftRoom()
    {
        cachedRoomList.Clear();
        ActivatePanel(panel_MainMenu);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerListUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerListUI();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // Si el host se va, el nuevo host debe ver el botón de Start
        if (startGameButton != null)
            startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);

        UpdatePlayerListUI(); // Para actualizar quién es el (Host)
    }

    #endregion

    #region Private UI Helpers

    private void UpdateRoomListUI()
    {
        // Limpiar lista visual
        foreach (Transform child in lobbyItemParent)
        {
            Destroy(child.gameObject);
        }

        // Volver a poblar la lista
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
        if (playerListText == null) return;

        playerListText.text = "";
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            playerListText.text += (p.IsMasterClient ? "(Host) " : "") + p.NickName + "\n";
        }
    }

    #endregion

    #region Public UI Methods (Botones)

    // --- Panel MainMenu ---
    public void OnClick_OpenCreateRoomPanel()
    {
        ActivatePanel(panel_CreateLobby);
        roomNameInput.text = $"{PhotonNetwork.NickName}s Room";
    }

    public void OnClick_OpenLobbyBrowser()
    {
        ActivatePanel(panel_LobbyBrowser);
    }

    // --- Panel CreateLobby ---
    // --- Panel CreateLobby ---
    public void OnClick_SubmitCreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            Debug.LogWarning("El nombre de la sala no puede estar vacío");
            return;
        }

        RoomOptions roomOptions = new RoomOptions();

        // Convertir dropdown (0=2, 1=4, 2=8) a int
        int maxPlayers = 2;
        if (maxPlayersDropdown.value == 1) maxPlayers = 4;
        if (maxPlayersDropdown.value == 2) maxPlayers = 8;

        roomOptions.MaxPlayers = (byte)maxPlayers;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;

        // -----------------------------------------------------------------
        //  LÍNEAS CORREGIDAS
        // -----------------------------------------------------------------

        // Propiedad custom para el nombre "bonito"
        // Se usa "CustomRoomProperties" (no "CustomProperties")
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        roomOptions.CustomRoomProperties["DisplayName"] = roomNameInput.text;

        // --- LÍNEA AÑADIDA ---
        // Debemos decirle a Photon qué propiedades son visibles en el Lobby
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "DisplayName" };

        // -----------------------------------------------------------------

        // Usar un ID único para el nombre real de la sala
        string roomID = System.Guid.NewGuid().ToString("N");

        PhotonNetwork.CreateRoom(roomID, roomOptions);
    }
    public void OnClick_BackToMainMenu()
    {
        ActivatePanel(panel_MainMenu);
    }

    // --- Panel LobbyBrowser ---
    // Este método es llamado por LobbyItem.cs
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    // --- Panel InLobby ---
    public void OnClick_LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    // -----------------------------------------------------------------
    //  MÉTODO MODIFICADO (Paso 1 del plan)
    // -----------------------------------------------------------------
    // Asigna este método al OnClick() de tu botón "Start Game"
    public void OnClick_StartGame()
    {
        // Solo el Master Client puede iniciar el juego
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        // --- LÍNEA AÑADIDA ---
        // Resetea los contadores para esta nueva partida
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScores();
        }
        // --- FIN DE LÍNEA AÑADIDA ---

        // Cerrar la sala para que nadie más entre
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        // Cargar la escena de juego para TODOS
        PhotonNetwork.LoadLevel(gameSceneName);
    }

    #endregion
}
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.UI; // Para NotificationHUDManager
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(PhotonView))]
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public string playerPrefabName;
    public Transform spawnPoint;

    [Header("Migration UI")]
    public GameObject migrationPanel;
    public float migrationPauseDuration = 3.0f;

    private const string PLAYER_LOADED_KEY = "PlayerLoaded";
    private HashSet<int> _deadPlayers = new HashSet<int>();

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (migrationPanel != null)
            migrationPanel.SetActive(false);

        // Marcamos que este cliente ya cargó su escena/juego
        Hashtable props = new Hashtable
        {
            { PLAYER_LOADED_KEY, true }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    #region Host Migration

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.LogWarning($"El Host se desconectó. Nuevo Host: {newMasterClient.NickName}");
        StartCoroutine(CoMigrateHost());
    }

    IEnumerator CoMigrateHost()
    {
        Debug.LogWarning("[MIGRATION] ⏸️ Iniciando pausa de seguridad...");

        if (migrationPanel != null)
        {
            migrationPanel.SetActive(true);
            Debug.Log("[MIGRATION] ✅ Panel activado.");
        }
        else
        {
            Debug.LogError("[MIGRATION] ❌ ERROR: No hay Migration Panel asignado en el Inspector.");
        }

        // Pausar tiempo del juego (solo lógica que respete timeScale)
        Time.timeScale = 0f;

        Debug.Log($"[MIGRATION] ⏳ Esperando {migrationPauseDuration} segundos...");
        yield return new WaitForSecondsRealtime(migrationPauseDuration);

        Time.timeScale = 1f;
        Debug.Log("[MIGRATION] ▶️ Juego reanudado.");

        if (migrationPanel != null)
            migrationPanel.SetActive(false);
    }

    #endregion

    #region Player Loading Callbacks

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(PLAYER_LOADED_KEY))
        {
            if (PhotonNetwork.IsMasterClient)
            {
                CheckIfAllPlayersAreLoaded();
            }
        }
    }

    private void CheckIfAllPlayersAreLoaded()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (!p.CustomProperties.TryGetValue(PLAYER_LOADED_KEY, out object playerLoaded) || !(bool)playerLoaded)
            {
                // Algún jugador todavía no está listo
                return;
            }
        }

        // Todos listos → arrancamos el juego para todos
        GetComponent<PhotonView>().RPC(nameof(RPC_StartGame), RpcTarget.All);
    }

    [PunRPC]
    public void RPC_StartGame()
    {
        PhotonNetwork.Instantiate(playerPrefabName, spawnPoint.position, spawnPoint.rotation);
    }

    #endregion

    #region Player Disconnect / Waves + Notificación

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[GAME] Player left room: {otherPlayer.NickName}");

        // 1) Mostrar notificación en pantalla usando tu sistema de HUD
        NotificationHUDManager hud = FindFirstObjectByType<NotificationHUDManager>();
        if (hud != null)
        {
            hud.CreateNotification($"{otherPlayer.NickName} has disconnect");
        }
        else
        {
            Debug.LogWarning("[GAME] No se encontró NotificationHUDManager en la escena.");
        }

        // 2) Solo el master resetea las oleadas
        if (!PhotonNetwork.IsMasterClient)
            return;

        WavesManager waves = FindFirstObjectByType<WavesManager>();
        if (waves != null)
        {
            Debug.Log("[GAME] Reiniciando WavesManager por desconexión de un jugador.");
            waves.ForceRestartFromSavedWave();
        }
        else
        {
            Debug.LogWarning("[GAME] No se encontró WavesManager en la escena.");
        }

        // 3) Si el jugador que se fue estaba marcado como muerto, lo removemos
        if (_deadPlayers.Contains(otherPlayer.ActorNumber))
        {
            _deadPlayers.Remove(otherPlayer.ActorNumber);
        }

        // Recalcular fin de juego por si el número de jugadores cambió
        TryEndGameIfAllDead();
    }

    #endregion

    #region Game Over Callbacks

    [PunRPC]
    public void RPC_NotifyPlayerDead(int playerActorNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        _deadPlayers.Add(playerActorNumber);
        TryEndGameIfAllDead();
    }

    [PunRPC]
    public void RPC_NotifyPlayerRespawn(int playerActorNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        _deadPlayers.Remove(playerActorNumber);
    }

    private void TryEndGameIfAllDead()
    {
        int totalPlayers = PhotonNetwork.PlayerList.Length;

        if (totalPlayers == 0)
            return;

        // Si la cantidad de jugadores muertos >= jugadores en la sala → todos muertos
        if (_deadPlayers.Count >= totalPlayers)
        {
            GetComponent<PhotonView>().RPC(nameof(RPC_GameOverLose), RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_GameOverLose()
    {
        var flow = FindFirstObjectByType<GameFlowManager>();
        if (flow != null)
        {
            flow.EndGame(false);
        }
    }

    #endregion
}

using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using Unity.FPS.Game;   

[RequireComponent(typeof(PhotonView))]
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public string playerPrefabName;
    public Transform spawnPoint;

    private const string PLAYER_LOADED_KEY = "PlayerLoaded";

    private HashSet<int> _deadPlayers = new HashSet<int>();

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        Hashtable props = new Hashtable
        {
            { PLAYER_LOADED_KEY, true }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

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
                return;
            }
        }

        GetComponent<PhotonView>().RPC(nameof(RPC_StartGame), RpcTarget.All);
    }

    [PunRPC]
    public void RPC_StartGame()
    {
        PhotonNetwork.Instantiate(playerPrefabName, spawnPoint.position, spawnPoint.rotation);
    }

    #endregion

    #region Game Over Callbacks

    [PunRPC]
    public void RPC_NotifyPlayerDead(int playerActorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        _deadPlayers.Add(playerActorNumber);
        TryEndGameIfAllDead();
    }

    [PunRPC]
    public void RPC_NotifyPlayerRespawn(int playerActorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        _deadPlayers.Remove(playerActorNumber);
    }

    private void TryEndGameIfAllDead()
    {
        int totalPlayers = PhotonNetwork.PlayerList.Length;
        if (totalPlayers == 0) return;

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
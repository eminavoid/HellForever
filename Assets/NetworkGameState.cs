using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Unity.FPS.Game; // GameFlowManager (para EndGame)
 
public class NetworkGameState : MonoBehaviourPun
{
    public static NetworkGameState Instance { get; private set; }
 
    readonly HashSet<int> _deadPlayers = new HashSet<int>(); // ViewIDs
 
    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
 
    public void NotifyPlayerDead(int playerViewId)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        _deadPlayers.Add(playerViewId);
        TryEndGameIfAllDead();
    }
 
    public void NotifyPlayerRespawn(int playerViewId)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        _deadPlayers.Remove(playerViewId);
    }
 
    void TryEndGameIfAllDead()
    {
        int totalPlayers = FindObjectsOfType<PlayerNetworkLife>().Length;
        if (totalPlayers == 0) return;
 
        if (_deadPlayers.Count >= totalPlayers)
            photonView.RPC(nameof(RPC_GameOverLose), RpcTarget.All);
    }
 
    [PunRPC]
    void RPC_GameOverLose()
    {
        var flow = FindFirstObjectByType<GameFlowManager>();
        if (flow != null)
            flow.SendMessage("EndGame", false, SendMessageOptions.DontRequireReceiver);
    }
}
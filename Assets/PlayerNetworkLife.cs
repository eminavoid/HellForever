using System.Collections;
using Photon.Pun;
using UnityEngine;
using Unity.FPS.Game;        
using Unity.FPS.Gameplay;    

public class PlayerNetworkLife : MonoBehaviourPun
{
    [Header("Respawn")]
    public float RespawnDelaySeconds = 10f;
    public Transform OptionalSpawnPoint;

    PlayerCharacterController _controller;
    PlayerWeaponsManager _weapons;
    CharacterController _cc;
    Collider[] _colliders;
    Renderer[] _renderers;
    Health _health;

    Vector3 _initialPos;
    Quaternion _initialRot;
    bool _isDead;

    private GameManager _gameManager;
    void Awake()
    {
        _controller = GetComponent<PlayerCharacterController>();
        _weapons = GetComponent<PlayerWeaponsManager>();
        _cc = GetComponent<CharacterController>();
        _colliders = GetComponentsInChildren<Collider>(true);
        _renderers = GetComponentsInChildren<Renderer>(true);
        _health = GetComponent<Health>();

        _initialPos = transform.position;
        _initialRot = transform.rotation;

        if (_health) _health.OnDie += OnDied;

        _gameManager = GameManager.Instance;     
    }

    void OnDestroy() { if (_health) _health.OnDie -= OnDied; }

    void OnDied()
    {
        if (_isDead) return;

        if (photonView.IsMine && _gameManager != null)
        {
            _gameManager.photonView.RPC(
                nameof(GameManager.RPC_NotifyPlayerDead),
                RpcTarget.MasterClient,
                photonView.Owner.ActorNumber
            );
        }
        photonView.RPC(nameof(RPC_PlayerDied), RpcTarget.All);
    }

    [PunRPC]
    void RPC_PlayerDied()
    {
        if (_isDead) return;
        _isDead = true;

        if (_controller) _controller.enabled = false;
        if (_weapons) _weapons.enabled = false;
        if (_cc) _cc.enabled = false;
        foreach (var c in _colliders) c.enabled = false;
        foreach (var r in _renderers) r.enabled = false;
        

        if (photonView.IsMine)
            StartCoroutine(CoRespawnAfterDelay());
    }

    IEnumerator CoRespawnAfterDelay()
    {
        yield return new WaitForSeconds(RespawnDelaySeconds);

        Vector3 pos = OptionalSpawnPoint ? OptionalSpawnPoint.position : _initialPos;
        Quaternion rot = OptionalSpawnPoint ? OptionalSpawnPoint.rotation : _initialRot;

        photonView.RPC(nameof(RPC_Respawn), RpcTarget.All, pos, rot);
    }

    [PunRPC]
    void RPC_Respawn(Vector3 pos, Quaternion rot)
    {
        transform.SetPositionAndRotation(pos, rot);

        if (_health) _health.RespawnFull();

        foreach (var r in _renderers) r.enabled = true;
        foreach (var c in _colliders) c.enabled = true;
        if (_cc) _cc.enabled = true;
        if (_weapons) _weapons.enabled = true;
        if (_controller) _controller.enabled = true;

        _isDead = false;

        if (photonView.IsMine && _gameManager != null)
        {
            _gameManager.photonView.RPC(
                nameof(GameManager.RPC_NotifyPlayerRespawn),
                RpcTarget.MasterClient,
                photonView.Owner.ActorNumber
            );
        }
    }
}
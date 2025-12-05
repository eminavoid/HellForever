using Photon.Pun;
using Unity.FPS.Game;
using UnityEngine;

public class LocalDeathHandler : MonoBehaviourPun
{
    [Header("Local Components")]
    [Tooltip("The parent GameObject that holds the local player's weapon model.")]
    public GameObject WeaponHolder;

    private GameObject HUD_Root;

    [Header("HUD Settings")]
    [Tooltip("The tag assigned to the root GameObject of the player's HUD/UI in the scene.")]
    public string HUD_Tag = "PlayerHUD";         

    private Health health;

    void Start()
    {
        if (!photonView.IsMine)
        {
            enabled = false;
            return;
        }

        health = GetComponent<Health>();

        if (health == null)
        {
            Debug.LogError("LocalDeathHandler requires a Health component on the same GameObject.");
            return;
        }

        HUD_Root = GameObject.FindGameObjectWithTag(HUD_Tag);

        if (HUD_Root == null)
        {
            Debug.LogError($"HUD root object with tag '{HUD_Tag}' not found in the scene. UI will not deactivate/reactivate correctly.");
        }
        health.OnDie += OnLocalPlayerDie;
        health.OnHealed += _ => OnLocalPlayerRespawn();
    }

    void OnLocalPlayerDie()
    {
        if (WeaponHolder != null)
        {
            WeaponHolder.SetActive(false);
        }

        if (HUD_Root != null)
        {
            HUD_Root.SetActive(false);
        }
    }

    void OnLocalPlayerRespawn()
    {
        if (WeaponHolder != null)
        {
            WeaponHolder.SetActive(true);
        }

        if (HUD_Root != null)
        {
            HUD_Root.SetActive(true);
        }
    }
}
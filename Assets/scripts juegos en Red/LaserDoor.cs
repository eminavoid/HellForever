using NUnit.Framework;
using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class LaserDoor : MonoBehaviourPun
{
    public List<GameObject> laserDoors;

    public double maxTimeDifferece = 0.2;

    private double lastTimeButton1 = -1;
    private double lastTimeButton2 = -1;

    [PunRPC]
    public void RPC_RegisteButtonPress(int buttonID, double serverTime)
    {
        Debug.Log($"Button {buttonID} pressed at server time {serverTime}");

        if (buttonID == 1)
        {
            lastTimeButton1 = serverTime;
        }
        else if (buttonID == 2)
        {
            lastTimeButton2 = serverTime;
        }
        else
        {
            Debug.LogWarning($"Unknown button ID: {buttonID}");
            return;
        }
    }
    private void CheckSync()
    {
        if (lastTimeButton1 < 0 || lastTimeButton2 < 0)
        {
            return;
        }
        
        double timeDiff = System.Math.Abs(lastTimeButton1 - lastTimeButton2);
        Debug.Log($"Dif de tiempo: {timeDiff} segs");

        if (timeDiff <= maxTimeDifferece)
        {
            OpenDoor();
        }
        else
        {
            Debug.Log("Buttons not pressed in sync.");
        }
    }

    private void OpenDoor()
    {
        foreach (GameObject door in laserDoors)
        {
            door.SetActive(false);
        }
    }
}

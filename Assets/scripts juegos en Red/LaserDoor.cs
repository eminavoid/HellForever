using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class LaserDoor : MonoBehaviourPun
{
    [Header("Visuals")]
    public List<GameObject> doors;

    [Header("Configuración de Timing")]
    [Tooltip("Tiempo máximo de diferencia entre los dos clicks (en segundos).")]
    public double maxTimeDifference = 0.2f;            

    private double lastTimeButton1 = -1;
    private double lastTimeButton2 = -1;

    [PunRPC]
    public void RPC_RegisterButtonPress(int buttonID, double serverTime)
    {
        Debug.Log($"Botón {buttonID} presionado en tiempo server: {serverTime}");

        if (buttonID == 1) lastTimeButton1 = serverTime;
        else if (buttonID == 2) lastTimeButton2 = serverTime;

        CheckSync();
    }

    private void CheckSync()
    {
        if (lastTimeButton1 < 0 || lastTimeButton2 < 0) return;

        double timeDiff = System.Math.Abs(lastTimeButton1 - lastTimeButton2);

        Debug.LogWarning($"? CALCULO DE TIEMPO:\n" +
                         $"Diferencia Real: {timeDiff.ToString("F4")} segs\n" +
                         $"Máximo Permitido: {maxTimeDifference.ToString("F4")} segs\n" +
                         $"¿Se abre?: {(timeDiff <= maxTimeDifference)}");
        if (timeDiff <= maxTimeDifference)
        {
            OpenDoor();
            lastTimeButton1 = -1;
            lastTimeButton2 = -1;
        }
        else
        {
            Debug.LogError(" FALLO: Los botones no se presionaron suficientemente rápido.");
        }
    }

    private void OpenDoor()
    {
        Debug.Log("¡Sincronización EXITOSA! Puerta abierta.");
        foreach (var door in doors)
        {
            door.SetActive(false);
        }
    }
}
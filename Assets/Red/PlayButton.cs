using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayButton : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private string gameSceneName = "MainScene";

    public void OnPlay()
    {
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            // ✅ Guardar nombre
            PhotonNetwork.NickName = nameInput.text;

            // ✅ Conectar a Photon (si no está conectado)
            if (!PhotonNetwork.IsConnected)
                PhotonNetwork.ConnectUsingSettings();

            // ✅ Cambiar de escena SOLO para este jugador
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);

            //PhotonNetwork.LoadLevel(1);
            //PhotonNetwork.AutomaticallySyncScene = true;
        }
        else
        {
            Debug.LogWarning("⚠️ Ingresa un nombre antes de jugar");
        }
    }
}

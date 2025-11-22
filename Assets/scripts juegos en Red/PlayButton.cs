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
            PhotonNetwork.NickName = nameInput.text;

            if (!PhotonNetwork.IsConnected)
                PhotonNetwork.ConnectUsingSettings();

            UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);

        }
        else
        {
            Debug.LogWarning("⚠️ Ingresa un nombre antes de jugar");
        }
    }
}

using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun; 


namespace Unity.FPS.UI
{
    public class LoadSceneButton : MonoBehaviour
    {
        public string SceneName = "";

        void Update()
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject
                && Input.GetButtonDown(GameConstants.k_ButtonNameSubmit))
            {
                LoadTargetScene();
            }
        }

        public void LoadTargetScene()
        {
            
            PhotonNetwork.LoadLevel(SceneName);
        }
    }
}

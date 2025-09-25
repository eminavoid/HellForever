using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun; // 👈 agregado para detectar multiplayer

namespace Unity.FPS.Game
{
    public class GameFlowManager : MonoBehaviour
    {
        [Header("Parameters")]
        [Tooltip("Duration of the fade-to-black at the end of the game")]
        public float EndSceneLoadDelay = 3f;

        [Tooltip("The canvas group of the fade-to-black screen")]
        public CanvasGroup EndGameFadeCanvasGroup;

        [Header("Win")]
        [Tooltip("This string has to be the name of the scene you want to load when winning")]
        public string WinSceneName = "WinScene";

        [Tooltip("Duration of delay before the fade-to-black, if winning")]
        public float DelayBeforeFadeToBlack = 4f;

        [Tooltip("Win game message")]
        public string WinGameMessage;
        [Tooltip("Duration of delay before the win message")]
        public float DelayBeforeWinMessage = 2f;

        [Tooltip("Sound played on win")]
        public AudioClip VictorySound;

        [Header("Lose")]
        [Tooltip("This string has to be the name of the scene you want to load when losing")]
        public string LoseSceneName = "LoseScene";

        public bool GameIsEnding { get; private set; }

        float m_TimeLoadEndGameScene;

        string m_SceneToLoadByName;
        int m_SceneToLoadByIndex = -1;

        void Awake()
        {
            EventManager.AddListener<AllObjectivesCompletedEvent>(OnAllObjectivesCompleted);
            EventManager.AddListener<PlayerDeathEvent>(OnPlayerDeath);
        }

        void Start()
        {
            AudioUtility.SetMasterVolume(1);
        }

        void Update()
        {
            if (GameIsEnding)
            {
                float timeRatio = 1 - (m_TimeLoadEndGameScene - Time.time) / EndSceneLoadDelay;
                EndGameFadeCanvasGroup.alpha = timeRatio;

                AudioUtility.SetMasterVolume(1 - timeRatio);

                if (Time.time >= m_TimeLoadEndGameScene)
                {
                    if (m_SceneToLoadByIndex >= 0)
                        SceneManager.LoadScene(m_SceneToLoadByIndex);
                    else
                        SceneManager.LoadScene(m_SceneToLoadByName);

                    GameIsEnding = false;
                }
            }
        }

        void OnAllObjectivesCompleted(AllObjectivesCompletedEvent evt) => EndGame(true);

        void OnPlayerDeath(PlayerDeathEvent evt)
        {
            // 🔹 Solo cambiar de escena si estamos en singleplayer
            if (!PhotonNetwork.InRoom)
            {
                EndGame(false);
            }
            else
            {
                Debug.Log("[GameFlowManager] Player murió en multiplayer → respawn manejado localmente.");
            }
        }

        void EndGame(bool win)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            GameIsEnding = true;
            EndGameFadeCanvasGroup.gameObject.SetActive(true);

            if (win)
            {
                if (TryGetNextLevelIndex(out int nextIndex))
                {
                    m_SceneToLoadByIndex = nextIndex;
                    m_SceneToLoadByName = null;
                }
                else
                {
                    m_SceneToLoadByIndex = -1;
                    m_SceneToLoadByName = WinSceneName;
                }

                m_TimeLoadEndGameScene = Time.time + EndSceneLoadDelay + DelayBeforeFadeToBlack;

                var audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = VictorySound;
                audioSource.playOnAwake = false;
                audioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDVictory);
                audioSource.PlayScheduled(AudioSettings.dspTime + DelayBeforeWinMessage);

                DisplayMessageEvent displayMessage = Events.DisplayMessageEvent;
                displayMessage.Message = WinGameMessage;
                displayMessage.DelayBeforeDisplay = DelayBeforeWinMessage;
                EventManager.Broadcast(displayMessage);
            }
            else
            {
                m_SceneToLoadByIndex = -1;
                m_SceneToLoadByName = LoseSceneName;
                m_TimeLoadEndGameScene = Time.time + EndSceneLoadDelay;
            }
        }

        bool TryGetNextLevelIndex(out int nextIndex)
        {
            int current = SceneManager.GetActiveScene().buildIndex;
            switch (current)
            {
                case 3: nextIndex = 4; return true;
                case 4: nextIndex = 5; return true;
                case 5: nextIndex = 0; return true;
                default:
                    nextIndex = -1;
                    return false;
            }
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<AllObjectivesCompletedEvent>(OnAllObjectivesCompleted);
            EventManager.RemoveListener<PlayerDeathEvent>(OnPlayerDeath);
        }
    }
}

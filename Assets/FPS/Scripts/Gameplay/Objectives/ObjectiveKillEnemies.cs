using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveKillEnemies : Objective
    {
        [Tooltip("Chose whether you need to kill every enemies or only a minimum amount")]
        public bool MustKillAllEnemies = true;

        [Tooltip("If MustKillAllEnemies is false, this is the amount of enemy kills required")]
        public int KillsToCompleteObjective = 5;

        [Tooltip("Start sending notification about remaining enemies when this amount of enemies is left")]
        public int NotificationEnemiesRemainingThreshold = 3;

        public int wavesTotal;
        public int wavesRemaining;

        private WavesManager m_WaveManager;

        int m_KillTotal;

        protected override void Start()
        {
            

            m_WaveManager = FindAnyObjectByType<WavesManager>();

            wavesTotal = m_WaveManager.WavesCount;
            wavesRemaining = m_WaveManager.WavesCount - 1; 

            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);

            // set a title and description specific for this type of objective, if it hasn't one
            if (string.IsNullOrEmpty(Title))
                Title = "Survive " + (MustKillAllEnemies ? "all the" : wavesTotal.ToString()) +
                        " waves";

            if (string.IsNullOrEmpty(Description))
                Description = GetUpdatedCounterAmount();

            base.Start();
        }

        void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (IsCompleted)
                return;

            m_KillTotal++;

            if (MustKillAllEnemies)
                KillsToCompleteObjective = evt.RemainingEnemyCount + m_KillTotal;

            int targetRemaining = MustKillAllEnemies ? evt.RemainingEnemyCount : KillsToCompleteObjective - m_KillTotal;

            // update the objective text according to how many enemies remain to kill
            if (targetRemaining == 0)
            {
                Debug.Log("waves restantes: " + wavesTotal);
                if (wavesRemaining > 0)
                {
                    m_WaveManager.WaveExecute();
                    m_KillTotal = 0;

                    targetRemaining = MustKillAllEnemies ? evt.RemainingEnemyCount : KillsToCompleteObjective - m_KillTotal;
                    string notificationText = NotificationEnemiesRemainingThreshold >= targetRemaining
                    ? targetRemaining + " enemies to kill left"
                    : string.Empty;

                    UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
                    wavesRemaining--;

                    DisplayNewTitle("Wave " + (wavesTotal-wavesRemaining) + "incoming");
                } else 
                {
                    CompleteObjective(string.Empty, GetUpdatedCounterAmount(), "Objective complete : " + Title);
                }
            }
            else if (targetRemaining == 1)
            {
                string notificationText = NotificationEnemiesRemainingThreshold >= targetRemaining
                    ? "One enemy left"
                    : string.Empty;
                UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
            }
            else
            {
                // create a notification text if needed, if it stays empty, the notification will not be created
                string notificationText = NotificationEnemiesRemainingThreshold >= targetRemaining
                    ? targetRemaining + " enemies to kill left"
                    : string.Empty;

                UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
            }
        }

        string GetUpdatedCounterAmount()
        {
            return m_KillTotal + " / " + KillsToCompleteObjective;
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
        }
    }
}
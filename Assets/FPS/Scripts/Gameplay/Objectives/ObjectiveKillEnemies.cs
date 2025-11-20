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

        // Variables visuales
        private int _currentWaveDisplay = 1;
        private int _totalWavesDisplay = 1;
        private int _killTotal = 0;

        protected override void Start()
        {
            // Intentamos obtener el total inicial
            var wm = FindAnyObjectByType<WavesManager>();
            if (wm != null)
            {
                _totalWavesDisplay = wm.WavesCount;
            }

            // Suscribirse a eventos
            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);

            // --- SUSCRIPCIÓN AL EVENTO DEL WAVESMANAGER ---
            WavesManager.OnWaveChanged += HandleWaveChange;

            if (string.IsNullOrEmpty(Title)) Title = "Sobrevive a las oleadas";
            if (string.IsNullOrEmpty(Description)) Description = GetUpdatedCounterAmount();

            base.Start();
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
            // --- DESUSCRIPCIÓN IMPORTANTE ---
            WavesManager.OnWaveChanged -= HandleWaveChange;
        }

        // Este método se ejecuta automáticamente cuando el WavesManager cambia de ronda
        void HandleWaveChange(int current, int total)
        {
            _currentWaveDisplay = current;
            _totalWavesDisplay = total;

            // Actualizamos el texto del objetivo
            UpdateObjective(string.Empty, GetUpdatedCounterAmount(), string.Empty);

            // Mostramos el título grande (Banner) en pantalla
            if (total == 999) // Código para modo infinito
            {
                DisplayNewTitle($"Endless Wave {_currentWaveDisplay}");
            }
            else
            {
                DisplayNewTitle($"Wave {_currentWaveDisplay} / {_totalWavesDisplay}");
            }
        }

        void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (IsCompleted) return;

            _killTotal++;

            // Solo actualizamos el contador de kills, no tocamos las ondas
            UpdateObjective(string.Empty, GetUpdatedCounterAmount(), string.Empty);
        }

        string GetUpdatedCounterAmount()
        {
            // Si es 999 mostramos el símbolo de infinito, si no el número
            string totalStr = (_totalWavesDisplay == 999) ? "∞" : _totalWavesDisplay.ToString();

            return $"Wave: {_currentWaveDisplay} / {totalStr}\nEnemies Killed: {_killTotal}";
        }
    }
}
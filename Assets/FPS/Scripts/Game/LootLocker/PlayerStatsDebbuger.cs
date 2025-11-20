using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

namespace Unity.FPS.Game
{
    // Este script va en el PREFAB DEL PLAYER
    public class PlayerStatsDebugger : MonoBehaviourPun
    {
        [Header("--- VISUALIZADOR (Solo Lectura) ---")]
        public string PlayerName;
        public int NetworkScore;
        public int NetworkKills;

        [Header("--- EDITOR (Modificar aquí) ---")]
        [Tooltip("Escribe aquí el nuevo puntaje que quieras poner")]
        public int SetScoreTo;

        [Tooltip("Escribe aquí las nuevas kills que quieras poner")]
        public int SetKillsTo;

        [Tooltip("¡Dale click a esta casilla para aplicar los cambios!")]
        public bool SEND_UPDATE = false;

        void Update()
        {
            // 1. LEER DATOS (Sincronizar Nube -> Inspector)
            if (photonView.Owner != null)
            {
                PlayerName = photonView.Owner.NickName;

                // Leemos Score
                if (photonView.Owner.CustomProperties.ContainsKey("Score"))
                    NetworkScore = (int)photonView.Owner.CustomProperties["Score"];

                // Leemos Kills
                if (photonView.Owner.CustomProperties.ContainsKey("Kills"))
                    NetworkKills = (int)photonView.Owner.CustomProperties["Kills"];
            }

            // 2. ESCRIBIR DATOS (Inspector -> Nube)
            if (SEND_UPDATE)
            {
                ApplyChanges();
                SEND_UPDATE = false; // Desmarcamos la casilla automáticamente
            }
        }

        void ApplyChanges()
        {
            if (photonView.Owner == null) return;

            Hashtable props = new Hashtable();

            // Preparamos los nuevos datos
            props["Score"] = SetScoreTo;
            props["Kills"] = SetKillsTo;

            // Enviamos a Photon (Esto actualiza a todos los jugadores y al ScoreManager)
            photonView.Owner.SetCustomProperties(props);

            Debug.Log($"🔧 [DEBUG] Stats actualizadas para {PlayerName}: Score={SetScoreTo}, Kills={SetKillsTo}");
        }

        // Esto es para copiar los valores actuales a las casillas de edición al iniciar
        void Start()
        {
            if (photonView.Owner != null)
            {
                if (photonView.Owner.CustomProperties.ContainsKey("Score"))
                    SetScoreTo = (int)photonView.Owner.CustomProperties["Score"];
                if (photonView.Owner.CustomProperties.ContainsKey("Kills"))
                    SetKillsTo = (int)photonView.Owner.CustomProperties["Kills"];
            }
        }
    }
}
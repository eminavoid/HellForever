using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq; // Para ordenar la lista

namespace Unity.FPS.UI
{
    public class MatchScoreboardUI : MonoBehaviourPunCallbacks
    {
        [Header("Referencias UI")]
        public GameObject scoreboardPanel; // El panel negro completo
        public Transform container;        // Donde van las filas
        public GameObject rowPrefab;       // El prefab de la fila (Item)

        [Header("Configuración")]
        public KeyCode openKey = KeyCode.Tab;

        void Start()
        {
            // Asegurarnos que empiece cerrado
            if (scoreboardPanel) scoreboardPanel.SetActive(false);
        }

        void Update()
        {
            // Al presionar TAB, mostramos
            if (Input.GetKeyDown(openKey))
            {
                scoreboardPanel.SetActive(true);
                RefreshBoard();
            }

            // Al soltar TAB, ocultamos
            if (Input.GetKeyUp(openKey))
            {
                scoreboardPanel.SetActive(false);
            }
        }

        // Esta función reconstruye la tabla
        void RefreshBoard()
        {
            // 1. Borrar filas viejas
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            // 2. Obtener jugadores y ordenarlos por Score (Mayor a menor)
            // Nota: Usamos una función segura para leer el score y ordenar
            var players = PhotonNetwork.PlayerList.OrderByDescending(p =>
                p.CustomProperties.ContainsKey("Score") ? (int)p.CustomProperties["Score"] : 0
            ).ToList();

            // 3. Crear filas nuevas
            foreach (Player p in players)
            {
                GameObject newRow = Instantiate(rowPrefab, container);
                MatchScoreboardItem item = newRow.GetComponent<MatchScoreboardItem>();
                if (item != null)
                {
                    item.Initialize(p);
                }
            }
        }

        // --- CALLBACKS DE PHOTON ---
        // Si alguien suma puntos mientras tenemos la tabla abierta, actualizamos al instante
        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            if (scoreboardPanel.activeSelf)
            {
                RefreshBoard();
            }
        }

        // Si alguien entra o sale, actualizamos
        public override void OnPlayerEnteredRoom(Player newPlayer) { if (scoreboardPanel.activeSelf) RefreshBoard(); }
        public override void OnPlayerLeftRoom(Player otherPlayer) { if (scoreboardPanel.activeSelf) RefreshBoard(); }
    }
}
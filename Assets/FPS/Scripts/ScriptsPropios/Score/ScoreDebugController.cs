using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.ours
{
    public class ScoreDebugController : MonoBehaviour
    {
        [Header("Gestión de Datos")]
        [SerializeField] public Button botonGenerar;
        [SerializeField] public Button botonBorrar;

        [Header("Ordenamiento (Sorting)")]
        [SerializeField] public Button btnSortTimeFastest;     
        [SerializeField] public Button btnSortTimeSlowest;     
        [SerializeField] public Button btnSortScoreHighest;     
        [SerializeField] public Button btnSortScoreLowest;      

        void Start()
        {
            if (botonGenerar != null) botonGenerar.onClick.AddListener(GenerarDatos);
            if (botonBorrar != null) botonBorrar.onClick.AddListener(BorrarDatos);

            if (btnSortTimeFastest != null)
                btnSortTimeFastest.onClick.AddListener(() => ScoreManager.Instance.SortTimeAscending());

            if (btnSortTimeSlowest != null)
                btnSortTimeSlowest.onClick.AddListener(() => ScoreManager.Instance.SortTimeDescending());

            if (btnSortScoreHighest != null)
                btnSortScoreHighest.onClick.AddListener(() => ScoreManager.Instance.SortScoreDescending());

            if (btnSortScoreLowest != null)
                btnSortScoreLowest.onClick.AddListener(() => ScoreManager.Instance.SortScoreAscending());
        }

        void GenerarDatos()
        {
            if (ScoreManager.Instance != null) ScoreManager.Instance.InjectRandomGames();
        }

        void BorrarDatos()
        {
            if (ScoreManager.Instance != null) ScoreManager.Instance.ResetAllData();
        }
    }
}
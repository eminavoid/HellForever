using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class NotificationHUDManager : MonoBehaviour
    {
        [Tooltip("UI panel containing the layoutGroup for displaying notifications")]
        public RectTransform NotificationPanel;

        [Tooltip("Prefab for the notifications")]
        public GameObject NotificationPrefab;

        void Awake()
        {
            // --- CORRECCIÓN MULTIPLAYER ---
            // En lugar de asumir que el jugador existe, verificamos primero.

            PlayerWeaponsManager playerWeaponsManager = FindFirstObjectByType<PlayerWeaponsManager>();
            if (playerWeaponsManager != null)
            {
                playerWeaponsManager.OnAddedWeapon += OnPickupWeapon;
            }
            // Si es null, no hacemos nada (evitamos el crash), así la UI sigue viva para las Waves.

            Jetpack jetpack = FindFirstObjectByType<Jetpack>();
            if (jetpack != null)
            {
                jetpack.OnUnlockJetpack += OnUnlockJetpack;
            }

            // Esto sí lo dejamos porque el EventManager es global
            EventManager.AddListener<ObjectiveUpdateEvent>(OnObjectiveUpdateEvent);
        }

        void OnObjectiveUpdateEvent(ObjectiveUpdateEvent evt)
        {
            if (!string.IsNullOrEmpty(evt.NotificationText))
                CreateNotification(evt.NotificationText);
        }

        void OnPickupWeapon(WeaponController weaponController, int index)
        {
            if (index != 0)
                CreateNotification("Picked up weapon : " + weaponController.WeaponName);
        }

        void OnUnlockJetpack(bool unlock)
        {
            CreateNotification("Jetpack unlocked");
        }

        // Esta es la función que llama el WavesManager
        public void CreateNotification(string text)
        {
            // Protección extra: si el prefab no está asignado, avisa y sal.
            if (NotificationPrefab == null || NotificationPanel == null)
            {
                Debug.LogWarning("NotificationHUDManager: Faltan referencias (Prefab o Panel) en el Inspector.");
                return;
            }

            GameObject notificationInstance = Instantiate(NotificationPrefab, NotificationPanel);
            notificationInstance.transform.SetSiblingIndex(0);

            NotificationToast toast = notificationInstance.GetComponent<NotificationToast>();
            if (toast)
            {
                toast.Initialize(text);
            }

            // Debug para confirmar que el mensaje llegó
            Debug.Log($"[Notification] Mostrando: {text}");
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<ObjectiveUpdateEvent>(OnObjectiveUpdateEvent);

            // Limpieza segura (por si acaso encontramos al player antes)
            PlayerWeaponsManager playerWeaponsManager = FindFirstObjectByType<PlayerWeaponsManager>();
            if (playerWeaponsManager != null)
                playerWeaponsManager.OnAddedWeapon -= OnPickupWeapon;

            Jetpack jetpack = FindFirstObjectByType<Jetpack>();
            if (jetpack != null)
                jetpack.OnUnlockJetpack -= OnUnlockJetpack;
        }
    }
}
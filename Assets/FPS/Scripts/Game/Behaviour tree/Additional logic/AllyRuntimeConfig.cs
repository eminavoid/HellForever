using UnityEngine;

public class AllyRuntimeConfig : MonoBehaviour
{
    [Header("--- Órbita (RotateAroundPlayer) ---")]
    [Tooltip("Distancia base del jugador")]
    public float orbitRadius = 3f;
    [Tooltip("Velocidad de giro en grados/segundo")]
    public float orbitSpeed = 60f;
    [Tooltip("Velocidad de regreso suave (horizontal)")]
    public float returnSpeed = 3f;

    [Header("--- Vibración (Oscilación) ---")]
    [Tooltip("Velocidad de la onda de seno/coseno")]
    public float vibrationSpeed = 10f;
    [Tooltip("Distancia que sube y baja (eje Y)")]
    public float yAmplitude = 0.5f;
    [Tooltip("Cuánto se acerca y aleja (Radio)")]
    public float rAmplitude = 0.2f;
    [Tooltip("Velocidad de seguimiento vertical (Y)")]
    public float verticalFollowSpeed = 15f;

    [Header("--- Apuntado (Rotación) ---")]
    [Tooltip("Velocidad de giro al apuntar")]
    public float rotationSlerpSpeed = 10f;
    [Tooltip("Ángulo para 'acostar' al aliado (90 = horizontal)")]
    public float fixedPitchAngle = 90f;

    [Header("--- Lógica de Persecución (IsTooFar) ---")]
    [Tooltip("Distancia máxima que el aliado puede alejarse del jugador para atacar")]
    public float maxChaseDistance = 20f;

    [Header("--- Lógica de Ataque (MoveToContact) ---")]
    [Tooltip("Velocidad de movimiento al atacar/rebotar")]
    public float attackSpeed = 8f;
    [Tooltip("Distancia a la que se considera 'contacto' para iniciar el rebote")]
    public float minContactDistance = 1.5f;
    [Tooltip("Distancia a la que se aleja durante el rebote")]
    public float bounceDistance = 3f;
}
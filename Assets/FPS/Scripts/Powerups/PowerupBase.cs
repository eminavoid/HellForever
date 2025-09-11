using UnityEngine;

public abstract class PowerupBase : ScriptableObject
{
    [Tooltip("Duración en segundos del efecto")]
    public float Duration = 10f;

    // Referencias opcionales si necesitás aplicar sobre player/arma
    protected GameObject _target;

    public void Init(GameObject target) => _target = target;

    // Lógica específica de cada powerup
    public abstract void Apply();
    public abstract void Revert();
}

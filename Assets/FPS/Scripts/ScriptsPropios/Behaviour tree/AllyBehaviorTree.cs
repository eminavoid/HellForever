using UnityEngine;

public class AllyBehaviorTree : MonoBehaviour
{
    private Node _rootNode;

    public AllyDetector enemyDetector;
    public AllyDetector playerRangeDetector;

    public Transform playerTransform;
    public Transform currentTarget;

    public bool hasTakenDamage = false;

    void Start()
    {
        // 1. Crear las acciones, condiciones y nodos compuestos aquí

        // 2. Definir el _rootNode (la estructura lógica que definimos antes)

        // Ejemplo de inicialización (simplificado):
        // _rootNode = new Selector(new List<Node>
        // {
        //     new Sequence(new List<Node> { new HasTakenDamage(), new Retaliate() }),
        //     new Sequence(new List<Node> { new IsTargetDetected(), new AttackTarget() }),
        //     new RotateAroundPlayer()
        // });
    }

    void Update()
    {
        // Ejecutar el árbol de comportamiento en cada frame
        if (_rootNode != null)
        {
            _rootNode.Evaluate();
        }
    }
}
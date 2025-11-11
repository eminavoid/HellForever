using UnityEngine;
using System.Collections.Generic;

public class AllyBehaviorTree : MonoBehaviour
{
    private Node _rootNode;
    private Transform _allyTransform;

    public Transform playerTransform;
    public AllyDetector enemyDetector;

    public Transform CurrentTarget { get; set; }     
    public Transform LastAttacker { get; set; }        
    public bool HasTakenDamage = false;             

    public float _initialOrbitHeight;

    public AllyRuntimeConfig Config { get; private set; }

    void Awake()
    {
        _allyTransform = this.transform;
        Config = GetComponent<AllyRuntimeConfig>();
    }

    void Start()
    {
        _initialOrbitHeight = _allyTransform.position.y - playerTransform.position.y;

        Node rotateAction = new RotateAroundPlayer(this, _allyTransform);

        Node checkEnemy = new CheckForEnemy(this);
        Node checkDistance = new IsTooFarFromPlayer(this, _allyTransform);
        Node attackAction = new MoveToContact(this, _allyTransform);

        Node checkDamage = new CheckDamageAndSetTarget(this);

        Node retaliateSequence = new Sequence(new List<Node> { checkDamage });

        Node attackSequence = new Sequence(new List<Node>
        {
            checkEnemy,             
            checkDistance,            
            attackAction         
        });

        Node patrolSequence = new Sequence(new List<Node> { rotateAction });

        _rootNode = new Selector(new List<Node>
        {
            retaliateSequence,
            attackSequence,
            patrolSequence
        });
    }

    void Update()
    {
        if (_rootNode != null)
        {
            _rootNode.Evaluate();
        }
    }
}
using UnityEngine;


public class WavesManager : MonoBehaviour
{
    [SerializeField] int wavesCount = 5;
    [SerializeField] int minEnemyCount = 1;
    [SerializeField] int maxEnemyCount = 10;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void registerSpawner(EnemySpawner enemySpawner) { }
}

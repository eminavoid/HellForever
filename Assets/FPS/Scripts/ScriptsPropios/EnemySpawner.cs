using System;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.ours
{
    public class EnemySpawner : MonoBehaviour
    {
        WavesManager m_waveManager;
        [SerializeField] float radius;
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(gameObject.transform.position, radius);
        }

        void Start()
        {
            m_waveManager = FindAnyObjectByType<WavesManager>();

            if (m_waveManager != null) 
            {
                m_waveManager.registerSpawner(this);
            } else
            {
                Debug.Log("Failed to find wave manager");
            }

                
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SpawnEnemyOnRadius(GameObject enemy)
        {
            Instantiate(enemy.gameObject, new Vector3(gameObject.transform.position.x + UnityEngine.Random.Range(-1*radius, radius), gameObject.transform.position.y , gameObject.transform.position.z + UnityEngine.Random.Range(-1 * radius, radius)), transform.rotation);
        }
    }
}

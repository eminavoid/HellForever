using Photon.Pun;
using System;
using System.Collections;
using Unity.FPS.Game;
using UnityEngine;
using Random = UnityEngine.Random;

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

        public void SpawnEnemyOnRadius(GameObject enemy)
        {
            WaitTSeconds(3f, enemy);
            //Debug.Log("esta haciendo spawn " + enemy.gameObject.name);
            //Instantiate(enemy.gameObject, new Vector3(gameObject.transform.position.x + UnityEngine.Random.Range(-1*radius, radius), gameObject.transform.position.y , gameObject.transform.position.z + UnityEngine.Random.Range(-1 * radius, radius)), transform.rotation);

        }
        IEnumerator WaitTSeconds(float seconds, GameObject enemy)
        {
            yield return new WaitForSeconds(seconds);

            if (!PhotonNetwork.IsMasterClient) yield break;

            var pos = new Vector3(
                transform.position.x + Random.Range(-1 * radius, radius),
                transform.position.y,
                transform.position.z + Random.Range(-1 * radius, radius));
            Debug.Log("esta haciendo spawn " + enemy.gameObject.name);
            PhotonNetwork.Instantiate(enemy.name, transform.position, transform.rotation);
        }
    }
}

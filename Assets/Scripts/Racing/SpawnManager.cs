using UnityEngine;

namespace Racing
{
    public class SpawnManager : MonoBehaviour
    {
        public static SpawnManager Instance { get; private set; }
        
        [SerializeField] public Transform[] spawnPoints;
        private int nextSpawnPoint = 0;
        
        private void Awake() {
            if (Instance != null && Instance != this){
                Destroy(gameObject);
            } else {
                Instance = this;
            }
        }

        public Transform GetNextSpawnPoint()
        {
            if (nextSpawnPoint == spawnPoints.Length - 1) nextSpawnPoint = 0;
            return spawnPoints[nextSpawnPoint++];
        }
    }
}
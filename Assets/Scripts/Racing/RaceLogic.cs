using UnityEngine;

public class RaceLogic : MonoBehaviour
{
    public int lapCount = 1;
    public int checkpointIndex;
    private GameObject[] points = RaceManager.Instance.checkpoints;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Checkpoint")){
            Checkpoint checkpoint = other.GetComponent<Checkpoint>();
            int idx = checkpoint.index;
            
            int nextCheckpoint = (checkpointIndex + 1) % points.Length;
            if (nextCheckpoint == idx) {
                if (nextCheckpoint == points.Length - 1) lapCount++;
                checkpointIndex = nextCheckpoint;

                RaceManager.Instance.CheckPlayerFinished(this);
                Debug.Log((lapCount, checkpointIndex));
            }
        }
    }
}

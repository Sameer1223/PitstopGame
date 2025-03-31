using System.Collections.Generic;
using System.Linq;
using Racing;
using UI;
using Unity.Netcode;
using UnityEngine;

public class LivePosition : NetworkBehaviour
{
    private List<Racer> liveRacePositions = new();


    private void Awake()
    {
        this.enabled = false;
    }
    
    private void Start()
    {
        InitializeRacePositions();
    }

    private void InitializeRacePositions()
    {
        Debug.Log("racer list loaded: " + RaceManager.Instance.Racers.Count);
        foreach (var racerRef in RaceManager.Instance.Racers)
        {
            if (racerRef.TryGet(out NetworkObject racerObject))
            {
                Racer racerComponent = racerObject.GetComponent<Racer>();
                if (racerComponent != null)
                {
                    liveRacePositions.Add(racerComponent);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        // liveRacePositions = liveRacePositions
        //     .OrderByDescending(racer => racer.lapCount.Value)
        //     .ThenByDescending(racer => racer.checkpointIndex.Value)
        //     .ThenBy(racer => racer.distToNextCheckpoint.Value)
        //     .ToList();
        
        liveRacePositions.Sort((a, b) =>
        {
            var lapCompare = b.lapCount.Value.CompareTo(a.lapCount.Value);
            if (lapCompare != 0) return lapCompare;

            var checkpointCompare = b.checkpointIndex.Value.CompareTo(a.checkpointIndex.Value);
            return checkpointCompare != 0 ? checkpointCompare : a.distToNextCheckpoint.Value.CompareTo(b.distToNextCheckpoint.Value);
        });
        
        
        NetworkObjectReference[] sortedRacerRefs = liveRacePositions
            .Select(racer => (NetworkObjectReference)racer.NetworkObject)
            .ToArray();

        if (!HasRaceOrderChanged(sortedRacerRefs)) return;
        UpdateRacePositionClientRpc(sortedRacerRefs);
    }
    
    private bool HasRaceOrderChanged(NetworkObjectReference[] newOrder)
    {
        if (liveRacePositions == null || liveRacePositions.Count != newOrder.Length) return true;

        return newOrder.Where((t, i) => !t.Equals(liveRacePositions[i])).Any();
    }

    [ClientRpc]
    private void UpdateRacePositionClientRpc(NetworkObjectReference[] racerRefs)
    {
        for (int i = 0; i < racerRefs.Length; i++)
        {
            if (racerRefs[i].TryGet(out NetworkObject racerObject))
            {
                Racer racer = racerObject.GetComponent<Racer>();
                if (racer != null)
                {
                    string positionString = GetOrdinal(i + 1);
                    racer.GetComponent<PlayerUI>().UpdateRacePosition(positionString);
                }
            }
        }
    }

    private string GetOrdinal(int position)
    {
        if (position % 100 >= 11 && position % 100 <= 13) return position + "th";

        switch (position % 10)
        {
            case 1: return position + "st";
            case 2: return position + "nd";
            case 3: return position + "rd";
            default: return position + "th";
        }
    }
}

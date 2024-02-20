using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour {
    public static SpawnManager INSTANCE;

    private List<Transform> spawnPoints = new();

    private void Awake() {
        INSTANCE = this;

        foreach (Transform child in transform) {
            spawnPoints.Add(child);
        }
    }

    public Transform GetSpawnPoint(int playerId) {
        return spawnPoints[playerId % spawnPoints.Count];
    }
}
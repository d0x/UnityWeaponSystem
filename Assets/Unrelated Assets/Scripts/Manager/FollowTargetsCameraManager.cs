using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FollowTargetsCameraManager : MonoBehaviour {
    public static FollowTargetsCameraManager INSTANCE;

    public List<Transform> targets;

    private void Awake() {
        INSTANCE = this;
    }

    void Update() {
        if (targets.Count == 0)
            return;

        var totalPos = Vector3.zero;
        foreach (Transform target in targets) {
            if (!target.IsDestroyed())
                totalPos += target.position;
        }

        var averagePos = totalPos / targets.Count;
        var lookRotation = Quaternion.LookRotation(averagePos - transform.position);

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime);
    }
}
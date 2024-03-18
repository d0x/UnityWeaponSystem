using UnityEngine;

public class FollowCamController : MonoBehaviour {
    private void OnEnable() {
        FollowTargetsCameraManager.targets.Add(transform);
    }

    private void OnDisable() {
        FollowTargetsCameraManager.targets.Remove(transform);
    }
}
using UnityEngine;

public class FollowCamController : MonoBehaviour {
    private void OnEnable() {
        FollowTargetsCameraManager.INSTANCE.targets.Add(transform);
    }

    private void OnDisable() {
        FollowTargetsCameraManager.INSTANCE.targets.Remove(transform);
    }
}
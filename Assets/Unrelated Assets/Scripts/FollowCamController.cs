using UnityEngine;

public class FollowCamController : MonoBehaviour {
    private void Start() {
        FollowTargetsCameraManager.INSTANCE.targets.Add(transform);
    }

    private void OnDestroy() {
        FollowTargetsCameraManager.INSTANCE.targets.Remove(transform);
    }
}
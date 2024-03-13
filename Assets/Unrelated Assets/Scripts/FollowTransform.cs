using UnityEngine;

public class FollowTransform : MonoBehaviour {
    public Transform followTarget;

    private void Update() {
        if (followTarget != null) {
            transform.position = followTarget.position;
            transform.rotation = followTarget.rotation;
        }
    }
}
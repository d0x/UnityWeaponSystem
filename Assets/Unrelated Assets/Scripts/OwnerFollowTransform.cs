using Unity.Netcode;
using UnityEngine;

public class OwnerFollowTransform : NetworkBehaviour {
    public Transform followTarget;

    public override void OnNetworkSpawn() {
        if (!IsOwner)
            enabled = false;
    }

    private void Update() {
        if (followTarget != null) {
            transform.position = followTarget.position;
            transform.rotation = followTarget.rotation;
        }
    }
}
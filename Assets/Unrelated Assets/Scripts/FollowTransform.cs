using Unity.Netcode;
using UnityEngine;

public class FollowTransform : NetworkBehaviour {
    public Transform followTarget;

    public void setFollowTarget(NetworkObject no) {
        performSetFollowTarget(no);
        distributeSetFollowTargetServerRpc(no);
    }

    [ServerRpc]
    private void distributeSetFollowTargetServerRpc(NetworkObjectReference nor) {
        distributeSetFollowTargetClientRpc(nor);
    }

    [ClientRpc]
    private void distributeSetFollowTargetClientRpc(NetworkObjectReference nor) {
        if (IsOwner) return;
        nor.TryGet(out var networkObject);
        performSetFollowTarget(networkObject);
    }

    private void performSetFollowTarget(NetworkObject no) {
        followTarget = no == null ? null : no.transform;
    }

    private void LateUpdate() {
        if (followTarget != null) {
            transform.position = followTarget.position;
            transform.rotation = followTarget.rotation;
        }
    }

    public void reset() {
        followTarget = null;
    }
}
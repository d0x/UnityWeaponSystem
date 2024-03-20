using Unity.Netcode;
using UnityEngine;

public enum ProjectileType {
    BULLET,
    CLUSTER_PART,
    GRENADE,
    ROCKET
}

public class ProjectileSimulator : NetworkBehaviour {
    public static ProjectileSimulator INSTANCE;

    private void Start() {
        INSTANCE = this;
    }

    [ServerRpc]
    public void simulateFireServerRpc(int projectileId, Vector3 position, Quaternion rotation) {
        simulateFireClientRpc(projectileId, position, rotation);
    }

    [ClientRpc]
    private void simulateFireClientRpc(int projectileId, Vector3 position, Quaternion rotation) {
        if (IsOwner) return;

        var projectile = ProjectilePool.INSTANCE.release(projectileId);
        projectile.fly(position, rotation);
        projectile.simulateActivation();
    }

    [ServerRpc]
    public void simulateBlowUpServerRpc(int projectileId, Vector3 position) {
        simulateBlowUpClientRpc(projectileId, position);
    }

    [ClientRpc]
    private void simulateBlowUpClientRpc(int projectileId, Vector3 position) {
        if (IsOwner) return;

        var projectile = ProjectilePool.INSTANCE.get(projectileId);

        // correct position in case the local simulation was off
        projectile.transform.position = position;

        projectile.explosiveForceEmitter.simulateBlowUp();

        ProjectilePool.INSTANCE.returnToPool(projectile);
    }

    [ServerRpc]
    public void simulateSpawnClustersServerRpc(ClusterPartInfo[] clusterPartInfos) {
        simulateSpawnClustersClientRpc(clusterPartInfos);
    }

    [ClientRpc]
    private void simulateSpawnClustersClientRpc(ClusterPartInfo[] clusterPartInfos) {
        if (IsOwner) return;

        foreach (var clusterPartInfo in clusterPartInfos) {
            var clusterPart = ProjectilePool.INSTANCE.release(clusterPartInfo.id);
            clusterPart.fly(clusterPartInfo.position, clusterPartInfo.rotation, clusterPartInfo.force);
            clusterPart.simulateActivation();
        }
    }
}
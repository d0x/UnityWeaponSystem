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
    public void blowUpServerRpc(int projectileId, Vector3 position) {
        blowUpClientRpc(projectileId, position);
    }

    [ClientRpc]
    private void blowUpClientRpc(int projectileId, Vector3 position) {
        if (IsOwner) return;

        var projectile = ProjectilePool.INSTANCE.get(projectileId);

        // correct position in case the local simulation was off
        projectile.transform.position = position;

        projectile.explosiveForceEmitter.simulateBlowUp();

        ProjectilePool.INSTANCE.returnToPool(projectile);
    }

    [ServerRpc]
    public void spawnClustersServerRpc(ClusterPartInfo[] clusterPartInfos) {
        spawnClustersClientRpc(clusterPartInfos);
    }

    [ClientRpc]
    private void spawnClustersClientRpc(ClusterPartInfo[] clusterPartInfos) {
        if (IsOwner) return;

        foreach (var clusterPartInfo in clusterPartInfos) {
            var clusterPart = ProjectilePool.INSTANCE.release(clusterPartInfo.id);
            clusterPart.fly(clusterPartInfo.position, clusterPartInfo.rotation, clusterPartInfo.force);
            clusterPart.simulateActivation();
        }
    }
}
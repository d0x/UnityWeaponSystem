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
    public void simulateBlowUpServerRpc(int projectileId, Vector3 position) {
        simulateBlowUpClientRpc(projectileId, position);
    }

    [ClientRpc]
    private void simulateBlowUpClientRpc(int projectileId, Vector3 position) {
        if (IsOwner) return;

        var projectile = ProjectilePool.INSTANCE.get(projectileId);

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
            clusterPart.transform.position = clusterPartInfo.position;
            clusterPart.GetComponent<Rigidbody>().velocity = clusterPartInfo.force;
            clusterPart.simulateActivation();
        }
    }
}
using System.Collections.Generic;
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

    private Dictionary<ulong, PlayerWeaponController> playerWeaponControllers = new();

    private void Awake() {
        INSTANCE = this;
    }

    public void add(ulong ownerClientId, PlayerWeaponController playerWeaponController) {
        playerWeaponControllers[ownerClientId] = playerWeaponController;
    }

    [ServerRpc]
    public void simulateFireServerRpc(ulong playerId, int projectileId, Vector3 position, Quaternion rotation) {
        simulateFireClientRpc(playerId, projectileId, position, rotation);
    }

    [ClientRpc]
    private void simulateFireClientRpc(ulong playerId, int projectileId, Vector3 position, Quaternion rotation) {
        if (IsOwner) return;

        var activeProjectile = getActiveProjectile(playerId);
        var projectileIdMatches = activeProjectile != null && activeProjectile.id == projectileId;

        var projectile = projectileIdMatches ? activeProjectile : ProjectilePool.INSTANCE.release(projectileId);

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

    private Projectile getActiveProjectile(ulong playerId) {
        return playerWeaponControllers[playerId].activeProjectile;
    }
}
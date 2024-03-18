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
}
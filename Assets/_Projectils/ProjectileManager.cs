using Unity.Netcode;
using UnityEngine;

public enum ProjectileType {
    BULLET,
    CLUSTER_PART,
    GRENADE,
    ROCKET
}

public class ProjectileManager : NetworkBehaviour {
    public static ProjectileManager INSTANCE;

    private void Start() {
        INSTANCE = this;
    }

    public void blowUp(Projectile projectile) {
        performBlowUp(projectile);
        simulateBlowUpServerRpc(projectile.id, projectile.transform.position);
    }

    private void performBlowUp(Projectile projectile) {
        projectile.explosiveForceEmitter.performBlowUp();
        ProjectilePool.INSTANCE.returnToPool(projectile);
    }

    [ServerRpc]
    private void simulateBlowUpServerRpc(int projectileId, Vector3 position) {
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
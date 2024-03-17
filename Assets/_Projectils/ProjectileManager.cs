using Unity.Netcode;

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
        simulateBlowUpServerRpc(projectile.id);
    }

    private void performBlowUp(Projectile projectile) {
        var explosiveForceEmitter = projectile.GetComponent<ExplosiveForceEmitter>();
        explosiveForceEmitter.blowUp();
        ProjectilePool.INSTANCE.returnToPool(projectile);
    }

    [ServerRpc]
    private void simulateBlowUpServerRpc(int projectileId) {
        simulateBlowUpClientRpc(projectileId);
    }

    [ClientRpc]
    private void simulateBlowUpClientRpc(int projectileId) {
        if (TurnManager.INSTANCE.isLocalPlayersTurn()) return;

        var projectile = ProjectilePool.INSTANCE.get(projectileId);

        ExplosionManager.INSTANCE.spawnExplosion(projectile.transform.position);
        ProjectilePool.INSTANCE.returnToPool(projectile);
    }
}
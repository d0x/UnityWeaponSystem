using System;
using Unity.Netcode;

public class ProjectileManager : NetworkBehaviour {
    public static ProjectileManager INSTANCE;

    public Projectile activeProjectile { get; set; }

    private void Awake() {
        INSTANCE = this;
    }

    public void mange(Projectile projectile) {
        activeProjectile = projectile;
    }

    public void blowUp(Projectile projectile) {
        performBlowUp(projectile);
        simulateBlowUpServerRpc();
    }

    private void performBlowUp(Projectile projectile) {
        var explosiveForceEmitter = projectile.GetComponent<ExplosiveForceEmitter>();
        explosiveForceEmitter.blowUp();
        Destroy(projectile.gameObject);
        activeProjectile = null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void simulateBlowUpServerRpc() {
        simulateBlowUpClientRpc();
    }

    [ClientRpc]
    private void simulateBlowUpClientRpc() {
        if (TurnManager.INSTANCE.isLocalPlayersTurn()) return;

        if (activeProjectile == null) return;
        ExplosionManager.INSTANCE.spawnExplosion(activeProjectile.transform.position);
        Destroy(activeProjectile.gameObject);
        activeProjectile = null;
    }
}
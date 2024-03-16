using System;
using UnityEngine;

public class Projectile : MonoBehaviour {
    public Transform followTransform;

    public ExplosiveForceEmitter explosiveForceEmitter;
    public ClusterPartSpawner clusterPartSpawner;
    public ExplodeOnCollision explodeOnCollision;

    private void Awake() {
        explosiveForceEmitter = GetComponent<ExplosiveForceEmitter>();
        clusterPartSpawner = GetComponent<ClusterPartSpawner>();
        explodeOnCollision = GetComponent<ExplodeOnCollision>();
    }

    private void Update() {
        if (followTransform == null) return;

        transform.position = followTransform.position;
        transform.rotation = followTransform.rotation;
    }

    public void blowUp() {
        ProjectileManager.INSTANCE.blowUp(this);
    }

    public void activateOwner() {
        Debug.Log($"{GetType().logName()}: Activate Real Projectile");
        
        if (clusterPartSpawner != null) clusterPartSpawner.enabled = false;
        explodeOnCollision.activate();
        activateExplosionTimer();
    }

    public void activateDummy() {
        Debug.Log($"{GetType().logName()}: Activate Dummy Projectile");
        
        if (explosiveForceEmitter != null) Destroy(explosiveForceEmitter);
        if (clusterPartSpawner != null) Destroy(clusterPartSpawner);
    }

    private void activateExplosionTimer() {
        Invoke(nameof(blowUp), 3f);
    }
}
using System;
using System.Collections;
using UnityEngine;

public class ExplosiveForceEmitter : MonoBehaviour {
    [SerializeField] private Boolean spawnExplosion = true;
    [SerializeField] private Boolean applyForces = true;

    public float explosionRadius = 5f;
    public float explosionForce = 10f;
    public float upwardsModifier = 5f;

    private void OnDestroy() {
        if (applyForces) {
            ApplyForces();
        }

        if (spawnExplosion) {
            ExplosionManager.INSTANCE.addExplosion(transform.position);
        }
    }

    private void ApplyForces() {
        var colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in colliders) {
            var rb = hit.GetComponent<Rigidbody>();
            if (rb != null) {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsModifier,
                    ForceMode.Impulse);
            }
        }
    }
}
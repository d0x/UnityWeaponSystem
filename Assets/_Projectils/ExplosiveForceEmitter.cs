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
            SpawnExplosion();
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

    private void SpawnExplosion() {
        var explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(explosion.GetComponent<SphereCollider>());

        explosion.transform.position = transform.position;
        explosion.transform.localScale = new Vector3(2, 2, 2);

        var explosionController = explosion.AddComponent<ExplosionController>();
        explosionController.startColor = Color.red;
        explosionController.endColor = Color.yellow;
        explosionController.transitionRenderer = explosion.GetComponent<Renderer>();

        explosionController.StartCoroutine(explosionController.ScaleAndDestroy(explosion));
    }
}

public class ExplosionController : MonoBehaviour {
    public Color startColor;
    public Color endColor;
    public Renderer transitionRenderer;

    public IEnumerator ScaleAndDestroy(GameObject obj) {
        float timeElapsed = 0;

        while (timeElapsed < 0.5f) {
            var targetScale = Mathf.Lerp(2, 5, timeElapsed / 0.2f);
            obj.transform.localScale = new Vector3(targetScale, targetScale, targetScale);
            Color newColor = Color.Lerp(startColor, endColor, timeElapsed / 0.5f);
            transitionRenderer.material.color = newColor;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(obj);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour {
    public static ExplosionManager INSTANCE;

    private HashSet<Vector3> explosions = new();

    private void Awake() {
        INSTANCE = this;
        Application.quitting += OnApplicationQuitting;
    }

    void Update() {
        var explosionsCopy = new HashSet<Vector3>(explosions);
        explosions.Clear();

        foreach (var explosion in explosionsCopy) {
            spawnExplosion(explosion);
        }
    }

    public void addExplosion(Vector3 t) {
        explosions.Add(t);
    }

    void spawnExplosion(Vector3 t) {
        var explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(explosion.GetComponent<SphereCollider>());

        explosion.transform.position = t;
        explosion.transform.localScale = new Vector3(2, 2, 2);

        var explosionController = explosion.AddComponent<ExplosionController>();
        explosionController.startColor = Color.red;
        explosionController.endColor = Color.yellow;
        explosionController.transitionRenderer = explosion.GetComponent<Renderer>();

        explosionController.StartCoroutine(explosionController.ScaleAndDestroy(explosion));
    }

    private void OnApplicationQuitting() {
        if (explosions.Count > 0) {
            Debug.Log($"Removed {explosions.Count} explosions beacuse the application is quitting.");
        }

        explosions.Clear();
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
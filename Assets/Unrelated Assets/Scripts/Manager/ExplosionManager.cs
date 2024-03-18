using System.Collections;
using UnityEngine;

public class ExplosionManager : MonoBehaviour {
    public static ExplosionManager INSTANCE;

    private void Awake() {
        INSTANCE = this;
    }
    
    public void spawnExplosion(Vector3 position) {
        var explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(explosion.GetComponent<SphereCollider>());

        explosion.transform.position = position;
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
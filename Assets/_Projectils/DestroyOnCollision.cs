using UnityEngine;
using System.Collections;

public class DestroyOnCollision : MonoBehaviour {
    // becomes true after 200ms
    // used that cluster parts don't collide with each-other
    private bool canDestroy = false;

    private void Start() {
        StartCoroutine(EnableDestroy());
    }

    IEnumerator EnableDestroy() {
        yield return new WaitForSeconds(0.2f);
        canDestroy = true;
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.name != gameObject.name && canDestroy) {
            Destroy(gameObject);
        }
    }
}
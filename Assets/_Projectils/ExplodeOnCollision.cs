using UnityEngine;
using System.Collections;


public class ExplodeOnCollision : MonoBehaviour {

    private bool canDestroy = false;

    public void activate() {
        StartCoroutine(EnableDestroy());
    }

    IEnumerator EnableDestroy() {
        yield return new WaitForSeconds(0.2f);
        canDestroy = true;
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.name != gameObject.name && canDestroy) {
            GetComponent<Projectile>().blowUp();
        }
    }
}
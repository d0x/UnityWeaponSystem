using UnityEngine;
using System.Collections;
using Unity.Netcode;


public class ExplodeOnCollision : NetworkBehaviour {
    private bool canDestroy = false;

    private void Start() {
        if (IsOwner)
            StartCoroutine(EnableDestroy());
        else
            enabled = false;
    }

    IEnumerator EnableDestroy() {
        yield return new WaitForSeconds(0.2f);
        canDestroy = true;
    }

    void OnCollisionEnter(Collision collision) {
        if (canDestroy && collision.gameObject.name != gameObject.name) {
            GetComponent<Projectile>().blowUp();
        }
    }
}
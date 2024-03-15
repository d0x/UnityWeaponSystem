using System;
using UnityEngine;

// TODO Should be NetworkBehaviour
public class Projectile : MonoBehaviour {
    public Action ExplodeEvent;

    public void blowUp() {
        ExplodeEvent?.Invoke();
        Destroy(gameObject);
    }

    public void activateExplosionTimer() {
        Invoke(nameof(blowUp), 3f);
    }
}
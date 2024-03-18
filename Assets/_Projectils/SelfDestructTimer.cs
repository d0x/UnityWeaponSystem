using UnityEngine;

public class SelfDestructTimer : MonoBehaviour {
    private static readonly float NEVER = float.MaxValue;

    [SerializeField] private float selfDestructTime = NEVER;

    private Projectile projectile;
    private float destroyAfterTime = NEVER;

    void Awake() {
        projectile = GetComponent<Projectile>();
    }

    private void Update() {
        if (Time.time < destroyAfterTime) return;

        projectile.blowUp();
    }

    public void activate() {
        destroyAfterTime = Time.time + selfDestructTime;
    }

    public void reset() {
        destroyAfterTime = NEVER;
    }
}
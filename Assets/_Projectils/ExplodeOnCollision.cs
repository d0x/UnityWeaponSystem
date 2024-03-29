using UnityEngine;

public class ExplodeOnCollision : MonoBehaviour {
    private static readonly float NEVER = 999999.0f;

    [Tooltip("Delay after activation to prevent immediate explosion. Helps in scenarios like cluster grenades.")]
    [SerializeField]
    private float delayAfterActivation = .2f;

    private Projectile projectile;
    private float destroyAfterTime = NEVER;

    void Awake() {
        projectile = GetComponent<Projectile>();
    }

    public void activate() {
        destroyAfterTime = Time.time + delayAfterActivation;
    }

    public void reset() {
        destroyAfterTime = NEVER;
    }

    void OnCollisionEnter(Collision collision) {
        handleCollision(collision);
    }

    void OnCollisionStay(Collision collision) {
        handleCollision(collision);
    }

    void handleCollision(Collision collision) {
        if (enabled && collision.gameObject.name != gameObject.name && Time.time >= destroyAfterTime) {
            projectile.blowUp();
        }
    }
}
using UnityEngine;

public class Projectile : MonoBehaviour {
    public ProjectileType type;
    public ExplosiveForceEmitter explosiveForceEmitter;
    public ClusterPartSpawner clusterPartSpawner;
    public ExplodeOnCollision explodeOnCollision;
    public FollowTransform followTransform;

    [SerializeField] private float selfDestructTime;
    [SerializeField] private bool useGravity;
    [SerializeField] private float force;

    // this ID is used to identify projectiles across the network.
    public int id;

    private Rigidbody rb;

    private float activationTime;
    private bool isBlowUpActive;

    private void Awake() {
        explosiveForceEmitter = GetComponent<ExplosiveForceEmitter>();
        clusterPartSpawner = GetComponent<ClusterPartSpawner>();
        explodeOnCollision = GetComponent<ExplodeOnCollision>();
        followTransform = GetComponent<FollowTransform>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        if (isBlowUpActive && Time.time >= activationTime) {
            blowUp();
        }
    }

    public void blowUp() {
        isBlowUpActive = false;
        ProjectileManager.INSTANCE.blowUp(this);
    }

    public void activateOwner() {
        Debug.Log($"{GetType().logName()}: Activate Real Projectile - {gameObject.name}");

        if (clusterPartSpawner != null) clusterPartSpawner.enabled = false;
        explodeOnCollision.activate();
        activateExplosionTimer();
    }

    public void activateDummy() {
        Debug.Log($"{GetType().logName()}: Activate Dummy Projectile - {gameObject.name}");

        if (explosiveForceEmitter != null) explosiveForceEmitter.enabled = false;
        if (clusterPartSpawner != null) clusterPartSpawner.enabled = false;
    }

    private void activateExplosionTimer() {
        isBlowUpActive = true;
        activationTime = Time.time + selfDestructTime;
    }

    public void reset() {
        isBlowUpActive = false;
        explodeOnCollision.reset();
        followTransform.reset();
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void fly(Vector3 position, Quaternion rotation) {
        transform.position = position;
        transform.rotation = rotation;

        rb.isKinematic = false;
        rb.useGravity = useGravity;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.AddForce(transform.forward * force);
    }
}
using UnityEngine;

public class Projectile : MonoBehaviour {
    public ProjectileType type;
    public ExplosiveForceEmitter explosiveForceEmitter;
    public ClusterPartSpawner clusterPartSpawner;
    public ExplodeOnCollision explodeOnCollision;
    public FollowTransform followTransform;
    public SelfDestructTimer selfDestructTimer;
    
    [SerializeField] private bool useGravity;
    [SerializeField] private float force;

    // this ID is used to identify projectiles across the network.
    public int id;

    private Rigidbody rb;
    
    private void Awake() {
        explosiveForceEmitter = GetComponent<ExplosiveForceEmitter>();
        clusterPartSpawner = GetComponent<ClusterPartSpawner>();
        explodeOnCollision = GetComponent<ExplodeOnCollision>();
        followTransform = GetComponent<FollowTransform>();
        selfDestructTimer = GetComponent<SelfDestructTimer>();
        rb = GetComponent<Rigidbody>();
    }
    
    public void blowUp() {
        explosiveForceEmitter.performBlowUp();
        ProjectileSimulator.INSTANCE.simulateBlowUpServerRpc(id, transform.position);
        ProjectilePool.INSTANCE.returnToPool(this);
    }

    public void performActivation() {
        Debug.Log($"{GetType().logName()}: Activate {gameObject.name}");

        if (clusterPartSpawner != null) clusterPartSpawner.enabled = false;
        explodeOnCollision.activate();
        selfDestructTimer.activate();
    }

    public void simulateActivation() {
        Debug.Log($"{GetType().logName()}: Simulate {gameObject.name}");
        // there is nothing to be done here.
    }

    public void reset() {
        selfDestructTimer.reset();
        explodeOnCollision.reset();
        followTransform.reset();
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void fly(Vector3 position, Quaternion rotation) {
        followTransform.followTarget = null;
        transform.position = position;
        transform.rotation = rotation;

        rb.isKinematic = false;
        rb.useGravity = useGravity;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.AddForce(transform.forward * force);
    }
}
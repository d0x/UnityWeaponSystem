using UnityEngine;

public class ClusterPartSpawner : MonoBehaviour {
    [SerializeField] private Projectile clusterPartPrefab;
    [SerializeField] private int clusterCount;

    private Rigidbody rb;

    private void Start() {
        rb = GetComponent<Rigidbody>();
    }

    private void OnDestroy() {
        spawnClusters(clusterCount, transform.position, clusterPartPrefab);
    }

    public void spawnClusters(int numberOfClusters, Vector3 position, Projectile projectile) {
        var spreadDirection = Vector3.up;
        var clusterForce = 3f;

        var angleStep = 360f / numberOfClusters;
        var angle = 0f;

        for (int i = 0; i < numberOfClusters; i++) {
            var direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            var cluster = Instantiate(projectile, position, Quaternion.identity);
            var rb = cluster.GetComponent<Rigidbody>();

            var finalDirection = (direction + spreadDirection.normalized).normalized;

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(finalDirection * clusterForce + this.rb.velocity, ForceMode.Impulse);
            angle += angleStep;
            cluster.activateServerRpc();
        }
    }
}
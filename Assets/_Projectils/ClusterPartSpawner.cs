using Unity.Netcode;
using UnityEngine;

public struct ClusterPartInfo : INetworkSerializable {
    public int id;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 force;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref id);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref rotation);
        serializer.SerializeValue(ref force);
    }
}

public class ClusterPartSpawner : MonoBehaviour {
    [SerializeField] private int clusterCount;

    private Rigidbody rb;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    public void spawnClusters(Vector3 transformPosition) {
        var clusterPartInfos = performSpawnClusters(clusterCount, transformPosition, rb.velocity);
        ProjectileSimulator.INSTANCE.simulateSpawnClustersServerRpc(clusterPartInfos);
    }

    public ClusterPartInfo[] performSpawnClusters(int numberOfClusters, Vector3 position, Vector3 velocity) {
        var spreadDirection = Vector3.up;
        var clusterForce = 3f;

        var angleStep = 360f / numberOfClusters;
        var angle = 0f;

        var clusterParts = new ClusterPartInfo[numberOfClusters];

        for (int i = 0; i < numberOfClusters; i++) {
            var direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            var projectile = ProjectilePool.INSTANCE.release(ProjectileType.CLUSTER_PART);
            var finalDirection = (direction + spreadDirection.normalized).normalized;
            var force = finalDirection * clusterForce + velocity;

            projectile.fly(position, Quaternion.identity, force);
            projectile.performActivation();

            angle += angleStep;

            clusterParts[i] = new ClusterPartInfo { id = projectile.id, position = position, force = force };
        }

        return clusterParts;
    }
}
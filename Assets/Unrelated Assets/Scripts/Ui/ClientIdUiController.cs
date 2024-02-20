using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ClientIdUiController : NetworkBehaviour {
    [SerializeField] private TextMeshProUGUI text;

    public override void OnNetworkSpawn() {
        text.text = $"ClientId: {NetworkManager.LocalClientId}";
    }
}
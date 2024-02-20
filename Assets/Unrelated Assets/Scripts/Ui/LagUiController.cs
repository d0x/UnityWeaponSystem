using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class LagUiController : MonoBehaviour {
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI label;

    private NetworkManager networkManager;

    private void Start() {
        networkManager = NetworkManager.Singleton;
        slider.onValueChanged.AddListener(ChangePacketDelay);
        ChangePacketDelay(slider.value);
    }

    private void ChangePacketDelay(float delay) {
        label.text = $"{(int)delay} ms";
        if (networkManager != null && networkManager.NetworkConfig.NetworkTransport is UnityTransport transport) {
            transport.DebugSimulator.PacketDelayMS = (int)delay;
        }
    }
}
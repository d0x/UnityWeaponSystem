using TMPro;
using Unity.Netcode;
using UnityEngine;

public class TurnUiController : NetworkBehaviour {
    [SerializeField] private TextMeshProUGUI activeTurnPlayer;
    [SerializeField] private GameObject nextButton;

    private void Awake() {
        nextButton.SetActive(false);
    }

    public override void OnNetworkSpawn() {
        TurnManager.INSTANCE.turn.OnValueChanged += (_, _) => TurnManger_OnTurnChanged();
        TurnManger_OnTurnChanged();
    }

    private void TurnManger_OnTurnChanged() {
        ulong newvalue = TurnManager.INSTANCE.turn.Value;
        activeTurnPlayer.text = $"Turn: {newvalue}";
        nextButton.SetActive(newvalue == NetworkManager.LocalClientId);
    }
}
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour {
    [SerializeField] private WeaponType defaultWeapon = WeaponType.NONE;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 100.0f;
    [SerializeField] private Transform weaponAnchor;
    [SerializeField] private PlayerWeaponController playerWeaponController;
    [SerializeField] private List<Color> colors;

    private NetworkVariable<float> weaponRotation = new(writePerm: NetworkVariableWritePermission.Owner);

    public Renderer bodyRenderer;
    private Rigidbody rb;

    // because we juggle with ownerships, we need to store the original OwnerClientId
    // this PlayerController was associated with.
    private ulong playerClientId;

    void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn() {
        var clientId = NetworkObject.OwnerClientId;
        playerClientId = clientId;
        bodyRenderer.material.color = colors[(int)clientId];
        weaponRotation.OnValueChanged += (_, _) => rotateWeaponAnchor();

        TurnManager.INSTANCE.turn.OnValueChanged += (_, _) => TurnManager_TurnChanged();
        TurnManager_TurnChanged();

        if (!IsOwner) {
            enabled = false;
            return;
        }

        WeaponUiController.INSTANCE.addPlayerController(this);

        var spawnPoint = SpawnManager.INSTANCE.GetSpawnPoint((int)clientId);
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        equip(defaultWeapon, false);
    }

    private void rotateWeaponAnchor() {
        var degrees = weaponRotation.Value;
        weaponAnchor.localRotation = Quaternion.Euler(degrees, 0, 0);
    }

    private void TurnManager_TurnChanged() {
        var isPlayerObjectsTurn = TurnManager.INSTANCE.turn.Value == playerClientId &&
                                  TurnManager.INSTANCE.turn.Value == NetworkManager.LocalClientId;
        
        enabled = isPlayerObjectsTurn;
    }

    void Update() {
        handleMovement();
        handleRotation();
        handleWeapon();
        handleEndTurn();
    }

    private void handleEndTurn() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            TurnManager.INSTANCE.endTurn();
        }
    }

    private void handleMovement() {
        var rotationHorizontal = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up, rotationHorizontal * rotationSpeed * Time.deltaTime);

        if (isSpecialKeyDown()) {
            rb.velocity = Vector3.zero;
            return;
        }

        var acceleration = Input.GetAxis("Vertical");
        if (acceleration != 0) {
            rb.velocity = transform.forward * acceleration * speed;
        }
    }

    private void handleRotation() {
        if (!isSpecialKeyDown()) return;
        weaponRotation.Value = weaponRotation.Value += Input.GetAxis("Vertical") * -rotationSpeed * Time.deltaTime;
    }

    private bool isSpecialKeyDown() {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ||
               Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    private void handleWeapon() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            fire();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            equip(WeaponType.BAZOOKA);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            equip(WeaponType.GRENADE);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            equip(WeaponType.RIFLE);
        }
    }

    public void equip(WeaponType weaponType, bool checkCurrentTurn = true) {
        if (checkCurrentTurn && !TurnManager.INSTANCE.isLocalPlayersTurn()) return;

        playerWeaponController.equip(weaponType);
        WeaponUiController.INSTANCE.updateCurrentWeapon(weaponType);
    }

    public void fire() {
        if (!TurnManager.INSTANCE.isLocalPlayersTurn()) return;

        playerWeaponController.fire();
    }
}
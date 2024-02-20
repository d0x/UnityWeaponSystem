using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUiController : NetworkBehaviour {
    public static WeaponUiController INSTANCE;

    [SerializeField] private TextMeshProUGUI currentWeapon;
    [SerializeField] private Button fireButton;
    [SerializeField] private Button bazookaButton;
    [SerializeField] private Button grenadeButton;
    [SerializeField] private Button rifleButton;
    
    private PlayerController playerController;

    private void Awake() {
        INSTANCE = this;
        updateButtonsVisible(false);
    }

    public void addPlayerController(PlayerController controller) {
        if (!controller.IsOwner) return;

        playerController = controller;
        updateButtonsVisible(controller != null);
    }

    public void equipBazooka() {
        playerController.equip(WeaponType.BAZOOKA);
    }

    public void equipGrenade() {
        playerController.equip(WeaponType.GRENADE);
    }

    public void equipRifle() {
        playerController.equip(WeaponType.RIFLE);
    }

    public void fire() {
        playerController.fire();
    }

    private void updateButtonsVisible(bool visible) {
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(visible);
        }
    }

    public void updateCurrentWeapon(WeaponType weaponType) {
        if (currentWeapon != null) {
            currentWeapon.text = $"{weaponType}";
        }

        fireButton.interactable = weaponType != WeaponType.NONE;
        bazookaButton.interactable = weaponType != WeaponType.BAZOOKA;
        grenadeButton.interactable = weaponType != WeaponType.GRENADE;
        rifleButton.interactable = weaponType != WeaponType.RIFLE;
    }
}
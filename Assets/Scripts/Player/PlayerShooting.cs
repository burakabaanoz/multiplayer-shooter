using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem; // Yeni Input sistemini dahil ediyoruz

public class PlayerShooting : NetworkBehaviour
{
    private InputSystem_Actions controls;

    // Karakter oyunda doðduðunda kontrolleri kur
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        controls = new InputSystem_Actions();
        controls.Player.Enable();
    }

    // Karakter yok olduðunda kontrolleri kapat (Hata almamak için önemli)
    public override void OnNetworkDespawn()
    {
        if (controls != null)
        {
            controls.Player.Disable();
        }
    }

    void Update()
    {
        if (!IsOwner || controls == null) return;

        // Eski 'Input.GetButtonDown' yerine, yeni sistemin 'WasPressedThisFrame' komutunu kullanýyoruz
        if (controls.Player.Attack.WasPressedThisFrame())
        {
            ShootServerRpc();
        }
    }

    // Bu metod sadece sunucuda (Server/Host) çalýþýr
    [ServerRpc]
    void ShootServerRpc()
    {
        Debug.Log("Bir oyuncu ateþ etti!");
    }
}
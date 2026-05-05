using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem; // Yeni Input sistemini koda dahil ediyoruz

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5f;

    // Otomatik oluþturduðumuz C# sýnýfýný çaðýrýyoruz
    private InputSystem_Actions controls;
    private Vector2 moveInput;

    // Karakter aðda (sahnede) yaratýldýðýnda çalýþýr
    public override void OnNetworkSpawn()
    {
        // Eðer bu karakterin sahibi biz deðilsek hiçbir þey yapma!
        if (!IsOwner) return;

        // Kontrolleri sadece bizim oyuncumuz için baþlat ve aktif et
        controls = new InputSystem_Actions();
        controls.Player.Enable();
    }

    // Karakter yok olduðunda (oyundan çýktýðýmýzda) kontrolleri kapatýrýz
    public override void OnNetworkDespawn()
    {
        if (controls != null)
        {
            controls.Player.Disable();
        }
    }

    void Update()
    {
        // Karakter bizim deðilse veya kontroller henüz yüklenmediyse hareket etme
        if (!IsOwner || controls == null) return;

        // Yeni sistemden WASD tuþlarýna basýlma oranýný (X ve Y olarak) okuyoruz
        moveInput = controls.Player.Move.ReadValue<Vector2>();

        // 2D'deki X ve Y verisini, 3D dünyadaki X (sað-sol) ve Z (ileri-geri) eksenlerine çeviriyoruz
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);

        transform.Translate(movement * speed * Time.deltaTime);
    }
}
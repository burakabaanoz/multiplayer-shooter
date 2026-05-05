using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : NetworkBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private float sensitivity = 0.5f; // Fare hassasiyeti
    [SerializeField] private float maxPitch = 90f; // Yukarý/Aþaðý bakma limiti
    [SerializeField] private float minPitch = -90f; // Yukarý/Aþaðý bakma limiti

    [Header("Referanslar")]
    [SerializeField] private Transform cameraTransform; // Adým 1'de oluþturduðumuz kameranýn Transformu

    private InputSystem_Actions controls;
    private float rotationX = 0f; // Kameranýn dikey dönüþ açýsýný saklar

    // Karakter oyunda doðduðunda çalýþýr
    public override void OnNetworkSpawn()
    {
        // Eðer bu karakterin sahibi biz DEÐÝLSEK (baþka bir oyuncuysa)
        if (!IsOwner)
        {
            // Onun kamerasýný ve ses dinleyicisini kapat, hata almayalým!
            cameraTransform.gameObject.SetActive(false);
            return; // Kodun geri kalanýný çalýþtýrma
        }

        // --- BURADAN AÞAÐISI SADECE BÝZÝM KARAKTERÝMÝZ ÝÇÝN ÇALIÞIR ---

        // Kontrolleri baþlat
        controls = new InputSystem_Actions();
        controls.Player.Enable();

        // Fareyi ekrana kilitle ve gizle (oyun içi niþan alma için)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Karakter yok olduðunda kontrolleri kapat (ÖNEMLÝ)
    public override void OnNetworkDespawn()
    {
        if (controls != null)
        {
            controls.Player.Disable();
        }
    }

    void Update()
    {
        // Karakter bizim deðilse veya kontroller yüklenmediyse hareket etme
        if (!IsOwner || controls == null) return;

        // Fare hareketini okuyoruz (Vector2)
        Vector2 mouseDelta = controls.Player.Look.ReadValue<Vector2>();

        // --- Dikey Dönüþ (Kamera Yukarý-Aþaðý) ---
        rotationX -= mouseDelta.y * sensitivity; // Y ekseni hareketini tersine çeviriyoruz (doðal his için)
        rotationX = Mathf.Clamp(rotationX, minPitch, maxPitch); // Döndürme limitini uyguluyoruz (tam tur atmasýn)

        // Sadece Kamerayý dikeyde döndürüyoruz (vücudu deðil)
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        // --- Yatay Dönüþ (Tüm Vücut Saða-Sola) ---
        // Vücudun tamamýný fare X ekseni hareketi kadar kendi etrafýnda (Y ekseninde) döndürüyoruz
        transform.Rotate(Vector3.up * mouseDelta.x * sensitivity);
    }
}
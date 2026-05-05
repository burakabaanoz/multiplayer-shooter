using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Text ve InputField için gerekli kütüphane

public class NetworkUI : MonoBehaviour
{
    [Header("Arayüz Elemanlarý")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_InputField joinCodeInput; // Arkadaþýnýn kodu yazacaðý yer
    [SerializeField] private TextMeshProUGUI codeDisplayText; // Senin kodunu gösterecek yazý

    private async void Start()
    {
        // Unity servislerini baþlat ve gizlice (anonim) giriþ yap
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        hostButton.onClick.AddListener(() => StartRelayHost());
        clientButton.onClick.AddListener(() => StartRelayClient(joinCodeInput.text));
    }

    // --- HOST (KURUCU) ÝÞLEMLERÝ ---
    private async void StartRelayHost()
    {
        try
        {
            // Unity sunucularýnda 4 kiþilik bir oda (Allocation) ayýr
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3); // 3 misafir + 1 Host = 4 kiþi

            // Oda için bir "Katýlým Kodu" (Join Code) al
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Kodu ekrana yazdýr ki arkadaþýna söyleyebilesin
            codeDisplayText.text = "Oda Kodu: " + joinCode;

            // NetworkManager'a Relay ayarlarýný tanýt
            RelayServerData relayServerData = allocation.ToRelayServerData("dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Host olarak oyunu baþlat
            NetworkManager.Singleton.StartHost();

            // Menüdeki butonlarý ve girdi alanýný gizle (yazý kalsýn ki kodu görelim)
            hostButton.gameObject.SetActive(false);
            clientButton.gameObject.SetActive(false);
            joinCodeInput.gameObject.SetActive(false);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Host oluþturulamadý: " + e.Message);
        }
    }

    // --- CLIENT (KATILIMCI) ÝÞLEMLERÝ ---
    private async void StartRelayClient(string joinCode)
    {
        try
        {
            // Arkadaþýnýn girdiði kod ile odayý bul
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // NetworkManager'a Relay ayarlarýný tanýt
            RelayServerData relayServerData = joinAllocation.ToRelayServerData("dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Client olarak oyunu baþlat
            NetworkManager.Singleton.StartClient();

            // Tüm menüyü gizle
            gameObject.SetActive(false);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Odaya katýlýnamadý: Kodu yanlýþ girmiþ olabilirsiniz. " + e.Message);
        }
    }
}
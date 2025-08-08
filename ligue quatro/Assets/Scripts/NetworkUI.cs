using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;
using TMPro;

public class NetworkUI : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public TMP_InputField ipInput;

    void Start()
    {
        hostButton.onClick.AddListener(StartAsHost);
        clientButton.onClick.AddListener(StartAsClient);
    }

    void StartAsHost()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        // Permite conexões de fora (ou seja, de outras máquinas)
        transport.SetConnectionData("0.0.0.0", 7777); 
        NetworkManager.Singleton.StartHost();
        Debug.Log("Host iniciado na porta 7777 (escutando todas interfaces)");
    }

    void StartAsClient()
    {
        string ip = ipInput.text.Trim();
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, 7777); // Conecta ao IP do host
        NetworkManager.Singleton.StartClient();
        Debug.Log($"Tentando conectar ao host em {ip}:7777");
    }
}
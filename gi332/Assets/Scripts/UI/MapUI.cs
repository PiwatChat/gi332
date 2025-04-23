using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MapUI : NetworkBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private MapSelectionManager _mapManager;
    
    [Header("UI References")]
    [SerializeField] private TMP_Text _mapNameText;
    [SerializeField] private TMP_Text _readyStatusText;

    void Start()
    {
        _startButton.gameObject.SetActive(NetworkManager.Singleton.IsHost);
    }

    public void OnHostSelectMap(string mapName)
    {
        _mapManager.HostSelectMap(mapName);
        GameSession.Instance.currentMapId = mapName;
    }

    public void OnPlayerReady()
    {
        var player = NetworkManager.Singleton.LocalClient.PlayerObject;
        player.GetComponent<PlayerReady>().SetReadyServerRpc(true);
    }

    void Update()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            bool isMapSelected = !string.IsNullOrEmpty(_mapManager.SelectedMap.Value.ToString());
            _startButton.interactable = _mapManager.CheckAllReady() && isMapSelected;
        }
        
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        
        string selectedMap = _mapManager.SelectedMap.Value.ToString();
        _mapNameText.text = string.IsNullOrEmpty(selectedMap) ? "No map selected" : $"Selected Map: {selectedMap}";

        
        int readyCount = _mapManager.GetReadyCount();
        int totalPlayers = _mapManager.GetTotalPlayers();
        _readyStatusText.text = $"Ready: {readyCount}/{totalPlayers}";
    }
}
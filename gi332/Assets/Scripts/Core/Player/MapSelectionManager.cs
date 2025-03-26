using Unity.Netcode;
using UnityEngine.SceneManagement;
using Unity.Collections;
using UnityEngine;

public class MapSelectionManager : NetworkBehaviour
{
    public NetworkVariable<FixedString128Bytes> SelectedMap = new NetworkVariable<FixedString128Bytes>();
    
    private void Awake()
    {
        SelectedMap.OnValueChanged += OnSelectedMapChanged;
    }
    
    private void OnDestroy()
    {
        SelectedMap.OnValueChanged -= OnSelectedMapChanged;
    }
    
    private void OnSelectedMapChanged(FixedString128Bytes oldValue, FixedString128Bytes newValue)
    {
        Debug.Log($"Map changed from {oldValue} to {newValue}");
    }
    
    
    public void HostSelectMap(string mapName)
    {
        if (IsServer) SelectedMap.Value = new FixedString128Bytes(mapName);
    }

    public bool CheckAllReady()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerReady = client.PlayerObject.GetComponent<PlayerReady>();
            if (!playerReady.IsReady.Value) return false;
        }
        return true;
    }
    
    public int GetReadyCount()
    {
        int count = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerReady = client.PlayerObject.GetComponent<PlayerReady>();
            if (playerReady.IsReady.Value) count++;
        }
        return count;
    }

    public int GetTotalPlayers()
    {
        return NetworkManager.Singleton.ConnectedClientsList.Count;
    }
    
    public void StartGame()
    {
        if (IsServer && CheckAllReady())
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var playerReady = client.PlayerObject.GetComponent<PlayerReady>();
                playerReady.SetReadyServerRpc(false);
            }

            NetworkManager.Singleton.SceneManager.LoadScene(SelectedMap.Value.ToString(), LoadSceneMode.Single);
        }
    }
}
using System;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class LimitLifeManager : NetworkBehaviour
{
    public static LimitLifeManager Instance { get; private set; }

    [SerializeField] private float maxLife = 3;
    private NetworkVariable<float> currentLife = new NetworkVariable<float>();

    [SerializeField] private TMP_Text lifeText;
    
    private NetworkVariable<Vector3> savePoint = new NetworkVariable<Vector3>();
    public event Action OnDie;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentLife.Value = maxLife;
            savePoint.Value = Vector3.zero;
        }
        currentLife.OnValueChanged += OnLifeChanged;
        UpdateText(currentLife.Value);
    }

    public void TakeDamage()
    {
        if (IsServer)
        {
            currentLife.Value = Mathf.Max(0, currentLife.Value - 1);
            if (currentLife.Value <= 0)
            {
                OnDie?.Invoke();
                Debug.Log("Team Life Depleted!");
            }
            RespawnAllPlayers();
        }
        else
        {
            TakeDamageServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc()
    {
        TakeDamage();
    }

    private void OnLifeChanged(float oldValue, float newValue)
    {
        UpdateText(newValue);
    }

    private void UpdateText(float life)
    {
        if (lifeText != null)
        {
            lifeText.text = life.ToString() + "/" + maxLife.ToString();
        }
    }
    
    public void SetSavePoint(Vector3 newSavePoint)
    {
        if (IsServer)
        {
            savePoint.Value = newSavePoint;
            Debug.Log("Save Point Updated: " + newSavePoint);
        }
        else
        {
            SetSavePointServerRpc(newSavePoint);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetSavePointServerRpc(Vector3 newSavePoint)
    {
        savePoint.Value = newSavePoint;
    }
    
    private void RespawnAllPlayers()
    {
        foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (player.PlayerObject != null)
            {
                player.PlayerObject.transform.position = savePoint.Value;
            }
        }
        RespawnAllPlayersClientRpc(savePoint.Value);
    }
    
    [ClientRpc]
    private void RespawnAllPlayersClientRpc(Vector3 respawnPosition)
    {
        if (NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            NetworkManager.Singleton.LocalClient.PlayerObject.transform.position = respawnPosition;
        }
    }
}
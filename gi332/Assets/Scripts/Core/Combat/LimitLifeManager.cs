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
        }
        else
        {
            TakeDamageServerRpc();
        }
    }

    [ServerRpc]
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
}
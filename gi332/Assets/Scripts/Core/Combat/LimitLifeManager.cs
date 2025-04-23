using System;
using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class LimitLifeManager : NetworkBehaviour
{
    public static LimitLifeManager Instance { get; private set; }

    [SerializeField] private float maxLife = 3;
    private NetworkVariable<float> currentLife = new NetworkVariable<float>();
    [SerializeField] private TMP_Text lifeText;
    [SerializeField] private GameObject defeatUI;
    [SerializeField] private TMP_Text defeatText;
    [SerializeField] private GameObject defeatButton;
    [SerializeField] private float delayBeforeNextScene = 20f;
    
    private NetworkVariable<Vector3> savePoint = new NetworkVariable<Vector3>();
    public event Action OnDie;
    
    private bool isCancelled = false;

    private bool isDis = false;
    public bool IsDis { get{ return isDis; }}

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
        
        defeatButton.SetActive(IsHost);
    }

    public void TakeDamage()
    {
        if (IsServer)
        {
            isDis = true;
            currentLife.Value = Mathf.Max(0, currentLife.Value - 1);
            if (currentLife.Value <= 0)
            {
                OnDie?.Invoke();
                TriggerDefeatClientRpc(delayBeforeNextScene);
                StartCoroutine(LoadMapSelectionAfterDelay());
                Debug.Log("Team Life Depleted!");
            }
            RespawnAllPlayers(savePoint.Value);
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
        Debug.Log("Save Point Updated: " + newSavePoint);
    }
    
    private void RespawnAllPlayers(Vector3 respawnPosition)
    {
        RespawnAllPlayersClientRpc(respawnPosition);
    }
    
    [ClientRpc]
    private void RespawnAllPlayersClientRpc(Vector3 respawnPosition)
    {
        if (NetworkManager.Singleton.LocalClient != null &&
            NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
            
            StartCoroutine(FadeAndTeleport(playerObj, respawnPosition));

            Debug.Log($"Teleported to Save Point at {respawnPosition}");
        }
    }
    
    private IEnumerator FadeAndTeleport(NetworkObject playerObj, Vector3 respawnPosition)
    {
        yield return ScreenFader.Instance.FadeOut();

        playerObj.transform.position = respawnPosition;

        if (playerObj.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = Vector2.zero;
        }

        yield return ScreenFader.Instance.FadeIn();
    }
    
    [ClientRpc]
    private void TriggerDefeatClientRpc(float delay)
    {
        if (defeatUI != null && defeatText != null)
        {
            defeatUI.SetActive(!defeatUI.activeInHierarchy);
            StartCoroutine(UpdateCountdownText(delay));
        }
    }

    private IEnumerator UpdateCountdownText(float delay)
    {
        float remaining = delay;
        while (remaining > 0  && !isCancelled)
        {
            if (isCancelled) yield break;
            
            defeatText.text = $"Next to Menu {Mathf.CeilToInt(remaining)} Second...";
            yield return new WaitForSecondsRealtime(1f);
            remaining -= 1f;
        }
    }
    
    private IEnumerator LoadMapSelectionAfterDelay()
    {
        float elapsed = 0f;
        while (elapsed < delayBeforeNextScene && !isCancelled)
        {
            if (isCancelled) yield break;
            
            yield return null;
            elapsed += Time.deltaTime;
        }

        if (!isCancelled)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MapSelection", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    public void OnPlayAgainClicked()
    {
        isDis = false;
        isCancelled = true;
        defeatUI.SetActive(false);
        currentLife.Value = maxLife;
        RespawnAllPlayers(Vector3.zero);
        HideDefeatUIClientRpc();
        ResetAllSavePointsClientRpc();
        
        StartCoroutine(ResetCancelFlagNextFrame());
    }
    
    [ClientRpc]
    private void HideDefeatUIClientRpc()
    {
        if (defeatUI != null)
        {
            defeatUI.SetActive(false);
        }
        
        StopCoroutine(UpdateCountdownText(delayBeforeNextScene));
        StopCoroutine(LoadMapSelectionAfterDelay());
    }
    
    [ClientRpc]
    private void ResetAllSavePointsClientRpc()
    {
        foreach (var save in FindObjectsOfType<PlayerSave>())
        {
            save.ResetSave();
        }
    }
    
    private IEnumerator ResetCancelFlagNextFrame()
    {
        yield return null;
        isCancelled = false;
    }
}
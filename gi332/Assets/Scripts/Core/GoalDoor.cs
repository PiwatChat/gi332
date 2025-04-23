using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GoalDoor : NetworkBehaviour
{ 
    public HashSet<ulong> playersInGoal = new HashSet<ulong>();

    [SerializeField] private Animator doorAnimator;
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private TMP_Text victoryText;
    [SerializeField] private float delayBeforeNextScene = 3f;
    
    public static GoalDoor Instance;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public bool HasPlayerReachedGoal(ulong clientId)
    {
        return playersInGoal.Contains(clientId);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;
        
        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsPlayerObject)
        {
            OnColliderToggle(other);
            playersInGoal.Add(netObj.OwnerClientId);
            CheckWinCondition();
        }
    }

    private void OnColliderToggle(Collider2D player)
    {
        NetworkObject netObj = player.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            TogglePlayerClientRpc(netObj.OwnerClientId);
        }
    }
    
    [ClientRpc]
    private void TogglePlayerClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        GameObject playerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().gameObject;

        if (playerObj.TryGetComponent(out BoxCollider2D c2D) &&
            playerObj.TryGetComponent(out PlayerMovement pMove) &&
            playerObj.TryGetComponent(out GoalPlayerState state))
        {
            c2D.isTrigger = !c2D.isTrigger;
            pMove.IsNotMove = !pMove.IsNotMove;
            state.ToggleVisibilityServerRpc();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsServer) return;

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsPlayerObject)
        {
            OnColliderToggle(other);
            playersInGoal.Remove(netObj.OwnerClientId);
        }
    }

    private void CheckWinCondition()
    {
        int totalPlayers = NetworkManager.Singleton.ConnectedClients.Count;
        if (playersInGoal.Count >= totalPlayers)
        {
            Debug.Log("All players reached the goal! You win!");
            OpenDoor();
            TriggerWinClientRpc(delayBeforeNextScene);
            
            if (IsServer)
            {
                var timer = FindObjectOfType<GameTimer>();
                string mapId = GameSession.Instance.currentMapId;

                OnGameEnd(true, LimitLifeManager.Instance.IsDis, timer, mapId);
            }
            
            StartCoroutine(LoadMapSelectionAfterDelay());
        }
    }

    private void OpenDoor()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }
    }

    [ClientRpc]
    private void TriggerWinClientRpc(float delay)
    {
        if (victoryUI != null && victoryText != null)
        {
            Time.timeScale = 0;
            victoryUI.SetActive(true);
            StartCoroutine(UpdateCountdownText(delay));
        }
    }

    private IEnumerator UpdateCountdownText(float delay)
    {
        float remaining = delay;
        while (remaining > 0)
        {
            victoryText.text = $"Next to Menu {Mathf.CeilToInt(remaining)} Second...";
            yield return new WaitForSecondsRealtime(1f);
            remaining -= 1f;
        }
    }

    private IEnumerator LoadMapSelectionAfterDelay()
    {
        yield return new WaitForSecondsRealtime(delayBeforeNextScene);
        NetworkManager.Singleton.SceneManager.LoadScene("MapSelection", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    
    void OnGameEnd(bool isWinner, bool died, GameTimer timer, string mapId)
    {
        var data = new MapScoreData();
        data.mapId = mapId;
        data.win = isWinner;
        data.noDeath = !died;
        data.timeChallenge = timer.GetElapsedTime() <= timer.timeLimit;

        GameSession.Instance.SaveScore(data);
    }
}

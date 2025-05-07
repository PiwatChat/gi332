using System;
using System.Collections;
using Unity.Netcode;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : NetworkBehaviour
{
    [SerializeField] 
    private GameObject gameMenu;
    [SerializeField]
    private PlayerMovement playerController;
    [SerializeField] 
    private InputReader inputReader;
    [SerializeField]
    private GameObject leaveMapButton;

    private bool isOpen = false;
    private bool delayOpen = false;
    private LobbiesList lobbiesList;
    private NetworkManager networkManager;
    private bool isMapSelectionScene;

    private void Start()
    {
        networkManager = NetworkManager.Singleton;
        
        leaveMapButton.SetActive(IsHost);
    }

    void OnEnable()
    {
        if (inputReader != null)
        {
            inputReader.OpenMenu += HandleOpen;
        }
    }

    void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.OpenMenu -= HandleOpen;
        }
    }
    public void LeaveServer()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("MapSelection", LoadSceneMode.Single);
    }
    
    private void HandleOpen(bool v)
    {
        isOpen = v;
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        
        isMapSelectionScene = SceneManager.GetActiveScene().name == "MapSelection";
        
        if (isOpen && !delayOpen && !isMapSelectionScene)
        {
            delayOpen = true;
            gameMenu.SetActive(!gameMenu.active);
            StartCoroutine(DelayOpenMenu());
        }
        
        if (gameMenu.active)
        {
            playerController.enabled = false;
            
        }
        else
        {
            playerController.enabled = true;
        }
    }

    private IEnumerator DelayOpenMenu()
    {
        yield return new WaitForSeconds(0.5f);
        delayOpen = false;
    }
}
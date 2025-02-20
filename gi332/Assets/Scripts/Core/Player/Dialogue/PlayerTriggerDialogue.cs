using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerTriggerDialogue : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    
    private bool playerInteracting;
    private DialogueTrigger dialogue;
    private bool inRange;
    
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }
        inputReader.InteractEvent += HandleInteract;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
        {
            return;
        }

        inputReader.InteractEvent -= HandleInteract;
    }

    private void Update()
    {
        if (inRange && playerInteracting)
        {
            dialogue.TriggerDialogue();
        }
    }

    private void HandleInteract(bool isPressed)
    {
        if (isPressed)
        {
            playerInteracting = true;
        }
        else
        {
            playerInteracting = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Dialogue"))
        {
            dialogue = other.GetComponent<DialogueTrigger>();
            inRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Dialogue"))
        {
            dialogue = null;
            inRange = false;
        }
    }
}

using System;
using Inventory.Model;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PickUpSystem : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField]
    private InventorySO inventor;
    [SerializeField]
    private AudioSource source;
    [SerializeField]
    private AudioClip itemPickup;
    
    private bool isPlayerInRange = false;
    private Item item;
    private bool playerInteracting;
    private bool canInteract = true;
    
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

    private void Update()
    {
        if (isPlayerInRange && playerInteracting && canInteract)
        { 
            StartCoroutine(PickUp());
            if (item != null)
            {
                int reminder = inventor.AddItem(item.InventoryItem, item.Quantity);
                if (reminder == 0)
                {
                    //source.PlayOneShot(itemPickup);
                    item.DestroyItem();
                }
                else
                {
                    item.Quantity = reminder;
                }
            }
        }
    }

    private IEnumerator PickUp()
    {
        canInteract = false;
        yield return new WaitForSeconds(1.5f);
        canInteract = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            item = collision.GetComponent<Item>();
            isPlayerInRange = true;
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            item = null;
            isPlayerInRange = false;
        }
    }
}


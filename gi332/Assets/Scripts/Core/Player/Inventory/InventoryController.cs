using System;
using System.Collections.Generic;
using Inventory.Model;
using Inventory.Ui;
using Unity.Netcode;
using UnityEngine;

namespace Inventory
{
    public class InventoryController : NetworkBehaviour
    {
        [SerializeField]
        private UIInventoryPage inventoryUI;
        public UIInventoryPage InventoryPage { get => inventoryUI; set => inventoryUI = value; }
    
        [SerializeField] private InventorySO inventoryData;
        
        public List<InventoryItem> initialItems = new List<InventoryItem>();
        
        [SerializeField] private AudioClip dropSound;
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioSource audioSource;
        
        private bool isOpen = false;

        private void Awake()
        {
            inventoryUI = FindObjectOfType<UIInventoryPage>();
            if (inventoryUI == null)
            {
                Debug.LogError("UIInventoryPage not found!");
                return;
            }
        }

        private void Start()
        {
            PrepareUI();
            PrepareInventoryData();
        }

        private void PrepareInventoryData()
        {
            inventoryData.Initialize();
            inventoryData.OnInventoryUpdated += UpdataInventoryUI;
            foreach (InventoryItem item in initialItems)
            {
                if (item.IsEmpty)
                {
                    continue;
                }
                inventoryData.AddItem(item);
            }
        }

        private void UpdataInventoryUI(Dictionary<int, InventoryItem> inventoryState)
        {
            inventoryUI.ResetAllItem();
            foreach (var item in inventoryState)
            {
                inventoryUI.UpdateData(item.Key, item.Value.item.ItemImage, item.Value.quantity);
            }
        }

        private void PrepareUI()
        {
            inventoryUI.InitializeInventoryUI(inventoryData.Size);
            this.inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
            this.inventoryUI.OnSwapItems += HandleSwapItem;
            this.inventoryUI.OnStartDragging += HandleDragging;
            this.inventoryUI.OnItemActionRequested += HandleItemActionReaquest;
        }

        private void HandleItemActionReaquest(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
            {
                return;
            }
            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null)
            {
                inventoryUI.ShowItemActions(itemIndex);
                inventoryUI.AddAction(itemAction.ActionName, () => PerformAction(itemIndex));
            }
            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventoryUI.AddAction("Drop",()=> DropItem(itemIndex, inventoryItem.quantity));
            }
        }

        public void PerformAction(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
            {
                return;
            }
            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventoryData.RemoveItem(itemIndex, 1);
            }
            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null)
            {
                if (itemAction.actionSFX != null)
                {
                    //audioSource.PlayOneShot(itemAction.actionSFX);
                }
                itemAction.PerformAction(gameObject);
                
                INote noteItem = inventoryItem.item as INote;
                if (noteItem != null)
                {
                    inventoryUI.DisplayNote(noteItem.GetNoteContent(), noteItem.GetNoteImage());
                    inventoryUI.Hide();
                }

                if (inventoryItem.item is KeyItemSO)
                {
                    inventoryUI.Hide();
                }
            }
        }

        private void DropItem(int itemIndex, int quantity)
        {
            inventoryData.RemoveItem(itemIndex, quantity);
            inventoryUI.ResetSelection();
            //audioSource.PlayOneShot(dropSound);
        }

        private void HandleDragging(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
            {
                return;
            }
            inventoryUI.CreateDraggedItem(inventoryItem.item.ItemImage, inventoryItem.quantity);
        }

        private void HandleSwapItem(int itemIndex_1, int itemIndex_2)
        {
            inventoryData.SwapItem(itemIndex_1, itemIndex_2);
        }

        private void HandleDescriptionRequest(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
            {
                inventoryUI.ResetSelection();
                return;
            }
            ItemSO item = inventoryItem.item;
            inventoryUI.UpdataDescription(itemIndex, item.ItemImage,
                item.Name, item.Description);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (isOpen)
                {
                    inventoryUI.Hide();
                    isOpen = false;
                }
                else
                {
                    inventoryUI.Show();
                    //audioSource.PlayOneShot(openSound);
                    foreach (var item in inventoryData.GetCurrentInventoryState())
                    {
                        inventoryUI.UpdateData(item.Key,
                            item.Value.item.ItemImage,
                            item.Value.quantity);
                    }
                    isOpen = true;
                }
            }
        }
    }
}

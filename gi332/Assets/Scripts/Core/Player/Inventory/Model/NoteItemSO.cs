using Inventory.Ui;
using UnityEngine;

namespace Inventory.Model
{
    public enum NoteType
    {
        TextOnly,
        ImageOnly,
        Both
    }
    [CreateAssetMenu]
    public class NoteItemSO : ItemSO, IItemAction, INote
    {
        [field: SerializeField]
        public NoteType NoteType { get; private set; }
        [field: SerializeField]
        public string NoteContent { get; private set; }
        
        [field: SerializeField]
        public Sprite NoteImage { get; private set; }
        
        public string ActionName => "Read";
        [field: SerializeField]
        public AudioClip actionSFX { get; private set; }
        public bool PerformAction(GameObject character)
        {
            UIInventoryPage inventoryUI = character.GetComponent<InventoryController>()?.InventoryPage;
            if (inventoryUI == null) return false;

            switch (NoteType)
            {
                case NoteType.TextOnly:
                    inventoryUI.DisplayNote(NoteContent, null);
                    break;
                case NoteType.ImageOnly:
                    inventoryUI.DisplayNote("", NoteImage);
                    break;
                case NoteType.Both:
                    inventoryUI.DisplayNote(NoteContent, NoteImage);
                    break;
            }
            return true;
        }

        public string GetNoteContent()
        {
            return NoteType == NoteType.TextOnly || NoteType == NoteType.Both ? NoteContent : "";
        }

        public Sprite GetNoteImage()
        {
            return NoteType == NoteType.ImageOnly || NoteType == NoteType.Both ? NoteImage : null;
        }
    }

    public interface INote
    {
        string GetNoteContent();
        Sprite GetNoteImage();
    }
}

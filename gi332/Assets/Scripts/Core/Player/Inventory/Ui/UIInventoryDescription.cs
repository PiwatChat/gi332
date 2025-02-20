using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory.Ui
{
    public class UIInventoryDescription : MonoBehaviour
    {
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;

        public void Awake()
        {
            ResetDescription();
        }

        public void ResetDescription()
        {
            this.itemImage.gameObject.SetActive(false);
            this.titleText.text = "";
            this.descriptionText.text = "";
        }

        public void SetDescription(Sprite sprite, string itemName, string description)
        {
            this.itemImage.gameObject.SetActive(true);
            this.itemImage.sprite = sprite;
            this.titleText.text = itemName;
            this.descriptionText.text = description;
        }
    }
}

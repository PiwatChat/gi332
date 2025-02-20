using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu]
    public class KeyItemSO : ItemSO, IItemAction
    {
        [SerializeField] private string actionName = "ใช้กญแจ";
        [field: SerializeField] public AudioClip actionSFX { get; private set; }

        public string ActionName => actionName;

        public bool PerformAction(GameObject user)
        {
            /*DoorLock door = FindObjectOfType<DoorLock>();
            if (door != null && door.IsLocked)
            {
                if (door.PlayerNearby)
                {
                    door.Unlock();
                }
                else
                {
                    return false;
                }
            }*/
            return false;
        }
    }

    public interface ILockable
    {
        bool IsLocked { get; }
        void Unlock();
    }
}
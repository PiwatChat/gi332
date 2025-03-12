using UnityEngine;
using Unity.Netcode;

public class LifePlayer : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner) return;

        if (collision.CompareTag("Obstacle"))
        {
            LimitLifeManager.Instance?.TakeDamage();
        }
    }
}
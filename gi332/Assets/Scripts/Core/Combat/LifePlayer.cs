using UnityEngine;

public class LifePlayer : MonoBehaviour
{
    private LimitLifeManager limitLifeManager;

    void Awake()
    {
        limitLifeManager = FindObjectOfType<LimitLifeManager>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle"))
        {
            limitLifeManager.TakeDamage();
        }
    }
}

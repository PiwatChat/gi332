using System;
using UnityEngine;
using TMPro;

public class LimitLifeManager : MonoBehaviour
{
    [SerializeField]
    private float maxLife;
    [SerializeField]
    private float currentLife;
    [SerializeField]
    private TMP_Text lifeText;
    
    private bool isDead;
    public Action<LimitLifeManager> OnDie;

    void Start()
    {
        currentLife = maxLife;
        UpdateText();
    }
    
    public void TakeDamage()
    {
        if (isDead)
        {
            return;
        }
        
        currentLife = Mathf.Max(0, currentLife - 1);
        UpdateText();
        if (currentLife <= 0)
        {
            isDead = true;
            OnDie?.Invoke(this);
            Time.timeScale = 0;
        }
    }
    
    private void UpdateText()
    {
        lifeText.text = maxLife.ToString() + "/" + currentLife.ToString();
    }
}

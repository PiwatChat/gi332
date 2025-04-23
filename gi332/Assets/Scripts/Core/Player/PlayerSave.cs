using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSave : MonoBehaviour
{
    [SerializeField] private Transform savePoint;
    private bool hasSaved = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasSaved)
        {
            LimitLifeManager.Instance.SetSavePoint(savePoint.position);
            hasSaved = true;
        }
    }

    public void ResetSave()
    {
        hasSaved = false;
    }
}

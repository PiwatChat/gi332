using System;
using System.Collections;
using MultiTargetCameraMovement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraAdd : MonoBehaviour
{
    private bool isAdded = false;

    private void Start()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void Update()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName.StartsWith("Map") && !isAdded)
        {
            StartCoroutine(DelayAdd());
        }
    }

    private IEnumerator DelayAdd()
    {
        isAdded = true;
        yield return new WaitForSeconds(0.5f);
        CameraMovement.Instance?.AddTarget(transform);
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (CameraMovement.Instance != null)
        {
            CameraMovement.Instance.RemoveTarget(transform);
        }
        isAdded = false; // รีเซ็ตให้สามารถ Add ใหม่เมื่อเข้าเกมอีกครั้ง
    }
}

using System;
using System.Collections;
using MultiTargetCameraMovement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraAdd : MonoBehaviour
{
    private bool isAdd = false;

    private void Update()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "Game"&& !isAdd)
        {
            StartCoroutine(DelayAdd());
        }
    }

    private IEnumerator DelayAdd()
    {
        isAdd = true;
        yield return new WaitForSeconds(0.5f);
        CameraMovement.Instance.AddTarget(gameObject.transform);
    }
}

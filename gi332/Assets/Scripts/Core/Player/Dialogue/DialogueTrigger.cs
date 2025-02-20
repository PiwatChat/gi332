using System;
using Unity.Netcode;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private bool removeDialogue;
    public Dialogue dialogue;
    [SerializeField] private bool fToIgnore = false;
    
    private bool triggered = false;
    private bool isPlaying = false;

    public void TriggerDialogue()
    {
        triggered = true;
        DialogueManager manager = FindObjectOfType<DialogueManager>();
        if (manager != null)
        {
            manager.StartDialogue(dialogue , this);
        }
        else
        {
            Debug.LogError("No DialogueManager found in the scene.");
        }
    }

    private void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !triggered)
        {
            isPlaying = true;
            if (fToIgnore)
            {
                TriggerDialogue();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")&& !triggered)
        {
            isPlaying = false;
            if (!triggered)
            {
                triggered = false;
            }
        }
    }

    public void ResetTrigger()
    {
        if (!removeDialogue)
        {
            triggered = false;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}

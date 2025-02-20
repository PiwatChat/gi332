using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image image;
    [SerializeField] private Animator animator;

    private Queue<Dialogue.DialogueLine> dialogueLines;
    private DialogueTrigger dialogueTrigger;
    private static DialogueManager instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        dialogueLines = new Queue<Dialogue.DialogueLine>();
    }

    public void StartDialogue(Dialogue dialogue, DialogueTrigger trigger)
    {
        animator.SetBool("IsOpen", true);
        dialogueTrigger = trigger;

        dialogueLines.Clear();

        foreach (var line in dialogue.dialogueLines)
        {
            dialogueLines.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (dialogueLines.Count == 0)
        {
            EndDialogue();
            return;
        }

        Dialogue.DialogueLine line = dialogueLines.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(line));
    }

    IEnumerator TypeSentence(Dialogue.DialogueLine line)
    {
        nameText.text = line.name;
        image.sprite = line.sprite;
        dialogueText.text = "";
        
        foreach (var letter in line.sentences.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }
    }

    void EndDialogue()
    {
        animator.SetBool("IsOpen", false);
        dialogueTrigger.ResetTrigger();
        dialogueTrigger  = null;
    }
}


using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Dialogue
{
    [System.Serializable]
    public class DialogueLine
    {
        public string name;
        public Sprite sprite;
    
        [TextArea(3,10)]
        public string sentences;
    }
    public List<DialogueLine> dialogueLines;
}

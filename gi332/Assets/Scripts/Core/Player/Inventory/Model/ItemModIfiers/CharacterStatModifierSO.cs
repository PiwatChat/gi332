using UnityEngine;
using UnityEngine.TextCore.Text;

public abstract class CharacterStatModifierSO : ScriptableObject
{
    public abstract void AffectCharacter(GameObject character, float value);
}

using UnityEngine;

[CreateAssetMenu]
public class CharacterStatHealModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float value)
    {
        /*HealthBar healthBar = character.GetComponent<HealthBar>();
        if (healthBar != null)
        {
            healthBar.AddHealth((int)value);
        }*/
    }
}

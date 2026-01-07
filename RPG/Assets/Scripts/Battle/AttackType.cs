using UnityEngine;

[System.Serializable]
public class AttackType
{
    public string attackName;
    public VoiceController.PitchTrend pitchTrend;
    public float minDamage;
    public float maxDamage;
    public int maxUses;
    public int remainingUses;
    public Sprite projectileSprite;
    public Color projectileColor = Color.white;

    public AttackType(string name, VoiceController.PitchTrend trend, float minDmg, float maxDmg, int uses, Sprite sprite = null)
    {
        attackName = name;
        pitchTrend = trend;
        minDamage = minDmg;
        maxDamage = maxDmg;
        maxUses = uses;
        remainingUses = uses;
        projectileSprite = sprite;
    }

    public bool CanUse()
    {
        return remainingUses > 0;
    }

    public void UseAttack()
    {
        if (remainingUses > 0)
        {
            remainingUses--;
        }
    }

    public float GetRandomDamage()
    {
        return Random.Range(minDamage, maxDamage);
    }
}

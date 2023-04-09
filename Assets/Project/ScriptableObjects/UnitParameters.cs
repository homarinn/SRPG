using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitParameters", menuName = "ScriptableObject/UnitParameters")]
public class UnitParameters : ScriptableObject
{
    public int maxHealth;
    public int strength;
    public int magic;
    public int defense;
    public int dexterity;
    public int agility;
    public int resist;
    public int luck;
    public int moveRange;
    public int level;
    public int currentExperience = 0;
    public int experience;
}

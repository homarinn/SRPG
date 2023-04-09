public class EnemyUnit : Unit
{
    public UnitParameters parameters;

    protected override void InitializeParameters()
    {
        maxHealth = parameters.maxHealth;
        health = parameters.maxHealth;
        strength = parameters.strength;
        magic = parameters.magic;
        defense = parameters.defense;
        dexterity = parameters.dexterity;
        agility = parameters.agility;
        resist = parameters.resist;
        luck = parameters.luck;
        moveRange = parameters.moveRange;
        level = parameters.level;
        currentExperience = parameters.currentExperience;
        experience = parameters.experience;
    }

    public override int[] GetAttackRange()
    {
        return new int[2] { 2, 4 };
    }
}


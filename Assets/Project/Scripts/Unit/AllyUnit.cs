public class AllyUnit : Unit
{
    protected override void InitializeParameters()
    {
        health = maxHealth;
    }

    public override int[] GetAttackRange()
    {
        return new int[2] { 1, 2 };
    }
}

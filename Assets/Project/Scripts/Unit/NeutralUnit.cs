using UnityEngine;

public class NeutralUnit : Unit
{
    public UnitParameters parameters;

    [SerializeField]
    private bool canGrow = false; // 成長可能かどうかを示すフラグ

    protected override void InitializeParameters()
    {
        health = maxHealth;
    }

    public override int[] GetAttackRange()
    {
        return new int[2] { 1, 1 };
    }

    // 味方ユニットや敵ユニットを倒したときに呼び出されるメソッド
    public void OnDefeatEnemy(Unit defeatedEnemy)
    {
        if (canGrow)
        {
        }
    }
}


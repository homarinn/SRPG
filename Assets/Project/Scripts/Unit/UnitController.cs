using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DG.Tweening;

public class UnitController : MonoBehaviour
{
    public float moveSpeed = 8f;

    public static UnitController Instance { get; private set; }

    public UnitEvent OnSelectEvent;
    public UnitEvent OnDeselectEvent;

    public UnitEvent OnMouseEnterEvent;
    public UnitEvent OnMouseExitEvent;

    [NonSerialized]
    public AllyUnit selectedUnit;

    bool canControll = true;

    public enum ACTION
    {
        ATTACK, // 攻撃
        MAGIC, // 魔法
        TALK, // 話す
        ITEM, // アイテム
        EXCHANGE, // 交換
        WAIT, // 待機
    }

    const string ATTACK_DISPLAY_NAME = "攻撃";
    const string MAGIC_DISPLAY_NAME = "魔法";
    const string TALK_DISPLAY_NAME = "話す";
    const string ITEM_DISPLAY_NAME = "道具";
    const string EXCHANGE_DISPLAY_NAME = "交換";
    const string WAIT_DISPLAY_NAME = "待機";

    public Dictionary<ACTION, string> actionDisplayNames = new ();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        actionDisplayNames[ACTION.ATTACK] = ATTACK_DISPLAY_NAME;
        actionDisplayNames[ACTION.MAGIC] = MAGIC_DISPLAY_NAME;
        actionDisplayNames[ACTION.TALK] = TALK_DISPLAY_NAME;
        actionDisplayNames[ACTION.ITEM] = ITEM_DISPLAY_NAME;
        actionDisplayNames[ACTION.EXCHANGE] = EXCHANGE_DISPLAY_NAME;
        actionDisplayNames[ACTION.WAIT] = WAIT_DISPLAY_NAME;
    }

    public void OnSelectUnit(Unit unit)
    {
        SelectUnit(unit);
    }

    public void OnHandleSelectedUnit(GridCell targetCell)
    {
        // マップの外だったらDeselect
        if (targetCell == null)
        {
            DeselectUnit();
            return;
        }

        // 移動可能なら移動
        if (CanMove(selectedUnit, targetCell))
        {
            MoveUnit(selectedUnit, targetCell);
            return;
        }

        Unit unit = targetCell.unit;

        // 移動可能でもなくUnitもいなければreturn
        if (unit == null)
        {
            DeselectUnit();
            return;
        }

        // 自分以外のUnitがいたらここの分岐(自分だった場合は↑の移動可能の条件にヒット)
        if (selectedUnit.IsAlly(unit))
        {
            DeselectUnit();
            SelectUnit(unit);
        }
        else
        {
            // TODO: 攻撃画面の表示
        }
    }

    private void SelectUnit(Unit unit)
    {
        if (unit is AllyUnit)
        {
            SelectUnit((AllyUnit)unit);
        }
        else if (unit is EnemyUnit)
        {
            SelectUnit((EnemyUnit)unit);
        }
        else if (unit is NeutralUnit)
        {
            SelectUnit((NeutralUnit)unit);
        }
    }

    private void SelectUnit(AllyUnit unit)
    {
        if (canControll)
        {
            selectedUnit = unit;
        }
    }

    private void SelectUnit(EnemyUnit unit)
    {
        // TODO: ステータス詳細表示
    }

    private void SelectUnit(NeutralUnit unit)
    {
        // TODO: ステータス詳細表示
    }

    private void DeselectUnit()
    {
        if (canControll) {
            Unit unit = selectedUnit;
            selectedUnit = null;
            GridManager.Instance.HideMoveAndAttackRange(unit);
        }
    }

    public void MoveUnit(Unit unit, GridCell targetCell)
    {
        // TODO: カメラ追従

        GridManager.Instance.HideMoveAndAttackRange(unit);

        GameController.Instance.IsLocked = true;
        canControll = false;

        unit.MoveStart();

        // Unitの高さを保持して、新しい位置でも高さを維持する
        float unitY = unit.cachedTransform.position.y;

        // pathを取得
        HashSet<GridCell> path = GridManager.Instance.FindPath(unit, targetCell);

        // 複数のGridCellを順番に移動するためのSequenceを作成
        Sequence moveSequence = DOTween.Sequence();

        foreach (GridCell gridCell in path)
        {
            Vector3 targetPosition = new Vector3(gridCell.x, unitY, gridCell.z);
            float moveDuration = Vector3.Distance(unit.cachedTransform.position, targetPosition) / moveSpeed;
            moveSequence.Append(unit.cachedTransform.DOMove(targetPosition, moveDuration).SetEase(Ease.Linear));
        }

        // 全ての移動が終わった時の処理を登録
        moveSequence.OnComplete(() => {
            GridManager.Instance.SetUnitToCell(unit, targetCell);

            GameController.Instance.IsLocked = false;
            canControll = true;

            unit.MoveEnd();

            GridManager.Instance.ShowMoveAndAttackRange(unit);

            ShowSelectActionUI(unit);
        });

        // 移動Sequenceを実行
        moveSequence.Play();
    }

    private bool CanMove(Unit unit, GridCell targetCell)
    {
        if (targetCell.unit != null && unit != targetCell.unit)
        {
            return false;
        }

        return GridManager.Instance.IsInCells(targetCell, unit.GetMoveableCells());
    }

    private void ShowSelectActionUI(Unit unit)
    {
        List<ACTION> actions = new();

        if (CanTalk(unit))
        {
            actions.Add(ACTION.TALK);
        }

        if (CanAttack(unit))
        {
            actions.Add(ACTION.ATTACK);
        }

        if (CanMagic(unit))
        {
            actions.Add(ACTION.MAGIC);
        }

        actions.Add(ACTION.ITEM);

        if (CanExchange(unit))
        {
            actions.Add(ACTION.EXCHANGE);
        }

        actions.Add(ACTION.WAIT);

        // TODO: UIの表示
        DoUnitAction(unit, ACTION.WAIT);
    }

    private bool CanTalk(Unit unit)
    {
        foreach (GridCell cell in GridManager.Instance.GetUnitNeighborCells(unit))
        {
            if (cell.unit != null && unit.canTalkUnits.Contains(cell.unit))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanAttack(Unit unit)
    {
        foreach (GridCell cell in unit.GetAttackableCells())
        {
            if (GridManager.Instance.IsOccupiedByEnemy(unit, cell))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanMagic(Unit unit)
    {
        return false;
    }

    private bool CanExchange(Unit unit)
    {
        foreach (GridCell cell in GridManager.Instance.GetUnitNeighborCells(unit))
        {
            if (GridManager.Instance.IsOccupiedByAlly(unit, cell))
            {
                return true;
            }
        }

        return false;
    }

    private void DoUnitAction(Unit unit, ACTION action)
    {
        unit.IsActioned = true;
        unit.TurnEnd();
    }

    public void OnMouseEnterUnitGridCell(GridCell gridCell)
    {
        Unit unit = gridCell.unit;
        GridManager.Instance.ShowMoveAndAttackRange(unit);
    }

    public void OnMouseExitUnitGridCell(GridCell gridCell)
    {
        Unit unit = gridCell.unit;

        if (!unit.IsSelected)
        {
            GridManager.Instance.HideMoveAndAttackRange(unit);
        }
    }
}

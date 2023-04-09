using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using static UnityEngine.UI.CanvasScaler;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    private UnitController unitController;

    public bool IsChangedPointedGridCell { get; set; } = false;

    public bool IsLocked { get; set; } = false;

    public GridCell pointedGridCell;
    public GridCell beforePointedGridCell;

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

    // Use this for initialization
    void Start()
	{
        unitController = UnitController.Instance;
    }

	// Update is called once per frame
	void Update()
	{
        if (IsLocked)
        {
            return;
        }

        if (IsChangedPointedGridCell)
        {
            // このタイミングで本当に切り替わる
            if (pointedGridCell != beforePointedGridCell)
            {
                IsChangedPointedGridCell = false;

                if (pointedGridCell?.unit != null)
                {
                    unitController.OnMouseEnterUnitGridCell(pointedGridCell);
                }

                if (beforePointedGridCell?.unit != null)
                {
                    unitController.OnMouseExitUnitGridCell(beforePointedGridCell);
                }
            }
        } else if (Input.GetMouseButtonDown(0))
        {
            if (unitController.selectedUnit == null)
            {
                if (pointedGridCell?.unit != null)
                {
                    unitController.OnSelectUnit(pointedGridCell.unit);
                }
            } else
            {
                unitController.OnHandleSelectedUnit(pointedGridCell);
            }
        }
    }
}


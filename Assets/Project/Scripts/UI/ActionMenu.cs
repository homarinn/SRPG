using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ActionMenu : MonoBehaviour
{
    public static ActionMenu Instance { get; private set; }

    const string ATTACK_DISPLAY_NAME = "攻撃";
    const string MAGIC_DISPLAY_NAME = "魔法";
    const string TALK_DISPLAY_NAME = "話す";
    const string ITEM_DISPLAY_NAME = "道具";
    const string EXCHANGE_DISPLAY_NAME = "交換";
    const string WAIT_DISPLAY_NAME = "待機";

    public Dictionary<Unit.ACTION, string> actionDisplayNames = new();

    public List<ActionMenuButton> actionMenuButtons = new();

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
    private void Start()
	{
        actionDisplayNames[Unit.ACTION.ATTACK] = ATTACK_DISPLAY_NAME;
        actionDisplayNames[Unit.ACTION.MAGIC] = MAGIC_DISPLAY_NAME;
        actionDisplayNames[Unit.ACTION.TALK] = TALK_DISPLAY_NAME;
        actionDisplayNames[Unit.ACTION.ITEM] = ITEM_DISPLAY_NAME;
        actionDisplayNames[Unit.ACTION.EXCHANGE] = EXCHANGE_DISPLAY_NAME;
        actionDisplayNames[Unit.ACTION.WAIT] = WAIT_DISPLAY_NAME;
    }

    public void Show(HashSet<Unit.ACTION> actions)
    {
        int i = 0;
        foreach (Unit.ACTION action in actions)
        {
            actionMenuButtons[i].action = action;
            i++;
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }

        foreach (ActionMenuButton actionMenuButton in actionMenuButtons)
        {
            GameObject ambObj = actionMenuButton.gameObject;
            if (ambObj.activeSelf)
            {
                ambObj.SetActive(false);
            }
        }


    }
}


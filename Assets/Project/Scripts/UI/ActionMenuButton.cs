using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionMenuButton : MonoBehaviour
{
    public Unit.ACTION action;
    private string textUI;
    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    public void Show(Unit.ACTION action)
    {
        SetAction(action);

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    public void SetAction(Unit.ACTION action)
    {
        this.action = action;
        textUI = ActionMenu.Instance.actionDisplayNames[action];
    }
}

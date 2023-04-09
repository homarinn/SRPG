using UnityEngine;

public class RangeLayer : MonoBehaviour
{
	GameObject cache;

    private void Start()
    {
        cache = gameObject;
        cache.SetActive(false);
    }

    public void ShowLayer()
    {
        if (!cache.activeSelf)
        {
            cache.SetActive(true);
        }
    }

    public void HideLayer()
    {
        if (cache.activeSelf)
        {
            cache.SetActive(false);
        }
    }
}


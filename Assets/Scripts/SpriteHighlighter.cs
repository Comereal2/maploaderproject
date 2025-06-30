using UnityEngine;
using UnityEngine.EventSystems;

public class SpriteHighlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    GameObject panelChild;

    private void Awake()
    {
        if(transform.childCount < 1)
        {
            panelChild = new GameObject("FallbackObject");
        }
        else
        {
            panelChild = transform.GetChild(0).gameObject;
        }
        panelChild.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        panelChild.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        panelChild.SetActive(false);
    }
}

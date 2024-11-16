using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
Queria um botao levemente diferente (com glow e com mais de um grafico)
entao escrevi esse botao
*/
public class CompositeButton : MonoBehaviour
{
    [HideInInspector] bool isSelected = false, isHovering = false;
    public UnityEvent onClick;

    [ColorUsage(true,true)] public Color idle = Color.white;
    [ColorUsage(true,true)] public Color hover = new Color(0.7f,0.7f,0.7f,1f);
    [ColorUsage(true,true)] public Color selected = new Color(0.2f,0.2f,0.2f,1f);

    private Image mainImage;
    private TextMeshProUGUI tmp;

    void Update()
    {
        Color color = isSelected ? selected : (isHovering ? hover : idle);
        SetColor(color);
    }

    private void Click() => onClick.Invoke();

    public void Start()
    {
        mainImage = GetComponent<Image>();
        tmp = GetComponentInChildren<TextMeshProUGUI>();

        EventTrigger eventTrigger = GetComponent<EventTrigger>();

        EventTrigger.Entry onMouseDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        onMouseDown.callback.AddListener((eventData) => 
        { 
            isSelected = true; 
            Click(); 
        });

        EventTrigger.Entry onMouseUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        onMouseUp.callback.AddListener((eventData) => isSelected = false);

        EventTrigger.Entry onMouseEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        onMouseEnter.callback.AddListener((eventData) => isHovering = true);

        EventTrigger.Entry onMouseExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        onMouseExit.callback.AddListener((eventData) => isHovering = false);

        eventTrigger.triggers.Add(onMouseDown);
        eventTrigger.triggers.Add(onMouseUp);
        eventTrigger.triggers.Add(onMouseEnter);
        eventTrigger.triggers.Add(onMouseExit);
    }

    public void SetColor(Color color)
    {
        mainImage.material.SetColor("_Color", color);
        tmp.color = color;
    }
}

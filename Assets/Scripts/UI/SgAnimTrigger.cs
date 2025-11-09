using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SgAnimTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private GameObject otherText;
    [SerializeField] private Text text;
    [SerializeField] public SandGlassAnim sandglass;
    [SerializeField] private int animValue;
    [SerializeField] private float normalOpacity;
    [SerializeField] private float OverButtonOpacity;
    [SerializeField] private float fadeTime = 0.15f;
    private Coroutine fade;


    private void Start()
    {
        if( text == null)
        text = GetComponentInChildren<Text>();
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        sandglass.SetState(animValue);
        ChangeOpacity(OverButtonOpacity);
        otherText.SetActive(false);
        
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        sandglass.SetState(0);
        ChangeOpacity(normalOpacity);
        otherText.SetActive(true);
    }

    private void ChangeOpacity(float targetedOpacity)
    {
        if(fade != null)
        {
            StopCoroutine(fade);
        }
        fade = StartCoroutine(OpacityChanger(targetedOpacity));
    }

    private IEnumerator OpacityChanger(float targetedOpacity)
    {
        Color normalColor = text.color;
        Color OverBColor = new Color(text.color.r, text.color.g, text.color.b, targetedOpacity);
        float time = 0f;

        while (time < fadeTime)
        {
            time += Time.deltaTime;
            text.color = Color.Lerp(normalColor, OverBColor, time / fadeTime);
            yield return null;
        }

        text.color = OverBColor;
    }
}

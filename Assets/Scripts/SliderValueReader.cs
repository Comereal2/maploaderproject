using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueReader : MonoBehaviour
{
    private Slider slider;
    private string startingString;

    private void Awake()
    {
        slider = GetComponentInParent<Slider>();
        startingString = GetComponent<TMP_Text>().text;
    }

    private void Update()
    {
        GetComponent<TMP_Text>().text = startingString + slider.value.ToString();
    }
}

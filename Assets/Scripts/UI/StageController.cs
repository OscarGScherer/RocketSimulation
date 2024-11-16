using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/*
Classe que lida com a ligacao entre os controles do UI com o foguete
*/
public class StageController : MonoBehaviour
{
    private RocketController stage;
    private Slider fuelSlider, thrustSlider;
    private RectTransform sliders;
    [HideInInspector] public Button progressButton;


    public void HookUp(RocketController rc)
    {
        stage = rc;
        
        sliders = transform.GetChild(0).GetComponent<RectTransform>();
        progressButton = transform.GetChild(1).GetComponent<Button>();

        fuelSlider = sliders.GetChild(0).GetComponent<Slider>();
        thrustSlider = sliders.GetChild(1).GetComponent<Slider>();

        transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = rc.name;

        thrustSlider.onValueChanged.AddListener((value) => SetThrust(value));
    }

    public void UpdateAltitudeSlider(Slider slider) => slider.value = stage.transform.position.y;

    private void SetThrust(float value) => stage.thrustLevel = value;
    public void HookThrustSlider() => stage.thrustLevel = thrustSlider.value;

    public void DetachNextStage()
    {
        stage.DetachNextStage();
    }

    public void ToggleSliders(bool state) => sliders.gameObject.SetActive(state);

    void Update()
    {
        if(stage != null) fuelSlider.value = stage.currentFuelMass / stage.maxFuelMass;
    }
}

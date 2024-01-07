using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private Light sun;

    [SerializeField, Range(0,24)] private float timeOfDay; 

    [SerializeField] private float sunRotationSpeed;

    [Header("LightingPreset")]
    [SerializeField] private Gradient skyColor;
    [SerializeField] private Gradient equatorColor;
    [SerializeField] private Gradient sunColor;

    private void Update(){
        timeOfDay += Time.deltaTime * sunRotationSpeed;
        if (timeOfDay > 24)
            timeOfDay = 0;
        UpdateSunRotationAndLighting();
    }

    private void onValidate(){
        UpdateSunRotationAndLighting();
    }

    /// <summary>
    /// Updates the sun rotation and lighting around the skybox
    /// </summary>
    private void UpdateSunRotationAndLighting(){
        
        var sunRotation = Mathf.Lerp(-200, 160, timeOfDay / 24);
        sun.transform.rotation = Quaternion.Euler(sunRotation, sun.transform.rotation.y, sun.transform.rotation.z);

        var timeFraction = timeOfDay / 24;
        RenderSettings.ambientEquatorColor = equatorColor.Evaluate(timeFraction);
        RenderSettings.ambientSkyColor = skyColor.Evaluate(timeFraction);
        sun.color = sunColor.Evaluate(timeFraction);
    }
}

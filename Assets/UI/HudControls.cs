using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

[SuppressMessage("ReSharper", "CheckNamespace")]
public class HudControls : MonoBehaviour
{
	private GameObject spawnManager;
	private AirfieldManager airfieldManager;
	private static AirplaneController airplaneController;
	private static WeaponController weaponController;
	private Dictionary<string, GameObject> uiElements;
	public List<GameObject> controls;

	private bool _isPlayerReady;
    // Start is called before the first frame update
    void Awake()
    {
	    // uiElements = GetComponents<GameObject>();
	    Debug.Log($"DEBUG: uiELEMENTS: {uiElements}");
	    _isPlayerReady = false;
    }

    private void Start()
    {
	    // uiElements = GetComponents<GameObject>().ToList()
	    foreach (var VARIABLE in GetComponents<GameObject>())
	    {
		    uiElements.Add(VARIABLE.name, VARIABLE);
	    }
    }

    // Update is called once per frame

    public void OnValueChanged(float value)
    {
	    airplaneController.distanceToCamera = value;
    }

    public void Bomb()
    {
        weaponController.Bomb();
    }

    public void Gun()
    {
        weaponController.Shoot();
    }

    public void Begin()
    {
	    Debug.Log("Start Button Pressed");
	    _isPlayerReady = true;
	    // GameObject.Find("SpawnManager").GetComponent<AirfieldManager>().setGameState(3);
    }
    public bool isPlayerReady() {return _isPlayerReady;}

    public void Show(string element) { uiElements[element].SetActive(true); }
    public bool isShown(string element) {return uiElements[element].activeSelf;}

    public void Hide(string element)
    {
	    uiElements[element].SetActive(false);
	    Debug.Log($"Hid element: {element}");
    }

    public static void getNewAirplane()
    {
	    GameObject airplane = GameObject.Find("PlayerPlane");
	    airplaneController = airplane.GetComponent<AirplaneController>();
	    weaponController = airplane.GetComponent<WeaponController>();
    }
}

using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[SuppressMessage("ReSharper", "CheckNamespace")]
public class HudControls : MonoBehaviour
{
	private GameObject spawnManager;
	private AirfieldManager airfieldManager;
	private static AirplaneController airplaneController;
	private static WeaponController weaponController;
	private GameObject uiElement;

	private bool _isPlayerReady;
    // Start is called before the first frame update
    void Start()
    {
	    uiElement = GetComponent<GameObject>();
	    _isPlayerReady = false;
	    Hide();
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

    private void Begin() { _isPlayerReady = true; }
    public bool isPlayerReady() {return _isPlayerReady;}

    public void Show() { uiElement.SetActive(true); }
    public bool isShown() {return uiElement.activeSelf;}
    public void Hide() { uiElement.SetActive(false); }

    public static void getNewAirplane()
    {
	    GameObject airplane = GameObject.Find("PlayerPlane");
	    airplaneController = airplane.GetComponent<AirplaneController>();
	    weaponController = airplane.GetComponent<WeaponController>();
    }
}

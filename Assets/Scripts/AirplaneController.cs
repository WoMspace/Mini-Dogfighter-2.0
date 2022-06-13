using System;
using UnityEngine;
// ReSharper disable StringLiteralTypo

public class AirplaneController : MonoBehaviour
{
    public bool UseLookControls = true;

    public float maxSpeed;
    private GameObject airplane;
    public float distanceToCamera;
    private Camera phoneCamera;
    private bool _isDestroyed;

    private int hp;

    // Start is called before the first frame update

    void Start()
    {
        airplane = GetComponent<GameObject>();
        phoneCamera = GetComponent<Camera>();
        distanceToCamera = 1.0f;
        hp = 100;
    }

    // Update is called once per frame
    void Update()
    {
        if (!airplane || !phoneCamera) GetComponents(); // Idk how to make this faster. Probably start from scratch.
        
        Debug.Log("WOM:AIRPLANECONTROLLER:UPDATE: BEGIN");
        // if (!AirfieldManager.Paused())
        // {
            if (UseLookControls) LookControls();
            else RemoteControls();
        // }

        _isDestroyed = hp <= 0;
    }
    // TODO: Add speed controller for propeller
    void LookControls()
    {
        Debug.Log("WOM:AIRPLANECONTROLLER:LookControls: Getting screen-space camera target");
        Vector3 cameraTarget = new(phoneCamera.pixelWidth / 2f, phoneCamera.pixelHeight / 2f, distanceToCamera);
        Debug.Log("WOM:AIRPLANECONTROLLER:LookControls: Getting world-space camera target");
        Vector3 targetLocation = phoneCamera.ScreenToWorldPoint(cameraTarget);
        Debug.Log("WOM:AIRPLANECONTROLLER:LookControls: Getting Airplane position");
        Vector3 airplanePosition = airplane.transform.position;
        // Vector3 targetRotation = targetLocation - position;
        
        Debug.Log("WOM:AIRPLANECONTROLLER:LookControls: Calculating updated Airplane position");
        airplanePosition += airplane.transform.forward * (maxSpeed * Time.deltaTime);
        Debug.Log("WOM:AIRPLANECONTROLLER:LookControls: Setting Airplane rotation");
        airplane.transform.LookAt(targetLocation);
        Debug.Log("WOM:AIRPLANECONTROLLER:LookControls: Setting updated Airplane position");
        airplane.transform.position = airplanePosition;
    }

    void RemoteControls()
    {
        throw new NotImplementedException("This isn't implemented. Please use Look Controls.");
    }
    public void GetComponents(GameObject Airplane = null, Camera phoneCam = null)
    {
        airplane = Airplane == null ? GameObject.FindWithTag("Player") : Airplane;
        phoneCamera = phoneCam == null ? GameObject.FindWithTag("ARCamera").GetComponent<Camera>() : phoneCam;
    }

    public bool isDestroyed()
    {
        return _isDestroyed;
    }
}

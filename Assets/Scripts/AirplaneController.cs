using System;
using UnityEngine;

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
    void Awake()
    {
        airplane = GetComponent<GameObject>();
        phoneCamera = GetComponent<Camera>();
        distanceToCamera = 1.0f;
    }

    void Start()
    {
        hp = 100;
    }

    // Update is called once per frame
    void Update()
    {
        if (!AirfieldManager.Paused())
        {
            if (UseLookControls) LookControls();
            else RemoteControls();
        }

        _isDestroyed = hp <= 0;
    }
    // TODO: Add speed controller for propeller
    void LookControls()
    {
        Vector3 cameraTarget = new Vector3(phoneCamera.pixelWidth / 2f, phoneCamera.pixelHeight / 2f, distanceToCamera);
        Vector3 targetLocation = phoneCamera.ScreenToWorldPoint(cameraTarget);
        Vector3 airplanePosition = airplane.transform.position;
        // Vector3 targetRotation = targetLocation - position;
        
        airplanePosition += airplane.transform.forward * (maxSpeed * Time.deltaTime);
        airplane.transform.position = airplanePosition;
        airplane.transform.LookAt(targetLocation);
    }

    void RemoteControls()
    {
        throw new NotImplementedException("This isn't implemented. Please use Look Controls.");
    }

    public bool isDestroyed()
    {
        return _isDestroyed;
    }
}

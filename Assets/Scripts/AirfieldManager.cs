using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
[RequireComponent(typeof(ARAnchorManager))]

public class AirfieldManager : MonoBehaviour
{
    // Setup stuff, ngl I don't know how it all works.
    private ARPlaneManager _planeManager;
    private ARRaycastManager _raycastManager;
    private ARAnchorManager _anchorManager;
    private static List<ARRaycastHit> rayHits = new();
    private List<ARAnchor> anchors;
    public GameObject ARCamera;
    
    // Airfield
    public GameObject AirfieldPrefab;
    private bool airfieldExists = false;
    private bool airfieldPlaceMode = true;
    
    // Airplane
    private bool readyToFly = false;
    private GameObject airplaneSpawner;
    public GameObject AirplanePrefab;
    void Awake()
    {
        _planeManager = GetComponent<ARPlaneManager>();
        _raycastManager = GetComponent<ARRaycastManager>();
        _anchorManager = GetComponent<ARAnchorManager>();

        airplaneSpawner = GameObject.Find("AirplaneSpawner");
    }

    // Update is called once per frame
    void Update()
    {
        if (!airfieldExists && airfieldPlaceMode) PlaceAirfield();
        if (airfieldExists && readyToFly) SpawnAirplane();
    }

    void PlaceAirfield()
    {
        // If not touching screen
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;
        
        if (_raycastManager.Raycast(touch.position, rayHits, TrackableType.PlaneWithinPolygon))
        {
            // Raycast hits are sorted by distance, so the first one
            // will be the closest hit.
            var hitPose = rayHits[0].pose;
            var hitTrackableId = rayHits[0].trackableId;
            var hitPlane = _planeManager.GetPlane(hitTrackableId);

            // This attaches an anchor to the area on the plane corresponding to the raycast hit,
            // and afterwards instantiates an instance of your chosen prefab at that point.
            // This prefab instance is parented to the anchor to make sure the position of the prefab is consistent
            // with the anchor, since an anchor attached to an ARPlane will be updated automatically by the ARAnchorManager as the ARPlane's exact position is refined.
            var anchor = _anchorManager.AttachAnchor(hitPlane, hitPose);
            Instantiate(AirfieldPrefab, anchor.transform);
            airfieldExists = true;
            airfieldPlaceMode = false;

            if (anchor == null)
            {
                Debug.Log("Error creating anchor.");
            }
            else
            {
                // Stores the anchor so that it may be removed later.
                anchors.Add(anchor);
            }
        }
    }

    void SpawnAirplane()
    {
        GameObject Airplane = Instantiate(AirplanePrefab);
    }
}

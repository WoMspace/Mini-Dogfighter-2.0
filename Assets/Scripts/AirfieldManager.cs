using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    // ReSharper disable once CollectionNeverQueried.Local
    private List<ARAnchor> anchors;
    public GameObject ARCamera;
    
    // The "Play" Button
    public GameObject HUD_Prefab;
    private GameObject HUD;
    private HudControls playButton;
    
    // Game state stuff
    private int gameState; // See Update() for meanings.
    public int GameState() {return gameState; }
    private static bool stateChanged;
    private static bool _paused;
    public static bool Paused() {return _paused;}
    public static bool StateChanged() {return stateChanged;}
    
    // Airfield
    public GameObject AirfieldPrefab;
    private bool _airfieldExists;
    
    // Airplane
    private GameObject airplaneSpawner;
    public GameObject AirplanePrefab;
    private GameObject airplane;
    private AirplaneController _airplaneController;
    void Awake()
    {
        _planeManager = GetComponent<ARPlaneManager>();
        _raycastManager = GetComponent<ARRaycastManager>();
        _anchorManager = GetComponent<ARAnchorManager>();

        airplaneSpawner = GameObject.Find("AirplaneSpawner");
        _airplaneController = airplane.GetComponent<AirplaneController>();
    }

    // Update is called once per frame
    void Update()
    {
        int oldState = gameState;
        switch (gameState)
        {
            case 0: //Not started, only tracking.
                break;
            case 1: // Ready to place airfield.
                if (PlaceAirfield()) gameState = 2;
                break;
            case 2: // Waiting for player to be ready
                if (stateChanged)
                {
                    HUD = Instantiate(HUD_Prefab);
                    playButton = GameObject.Find("PlayButton").GetComponent<HudControls>();
                    playButton.Show();
                }
                if (playButton.isPlayerReady()) gameState = 3;
                break;
            case 3: // Player ready. Starting game.
                if (stateChanged)
                { // should only last one frame. Could foreseeably get stuck here.
                    playButton.Hide();
                    SpawnAirplane();
                    gameState = 4;
                }
                break;
            case 4: // Flight in progress.
                if (_paused) gameState = 5;
                break;
            case 5: // Game in progress, paused.
                if (!_paused) {gameState = 4; return;}
                break;
            case 6: // Aircraft destroyed.
                break;
            case 7: // Game ended. Show Menu.
                break;
        }

        stateChanged = oldState != gameState;
        if (stateChanged)
        {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (_paused) Time.timeScale = 0;
            else Time.timeScale = 1;
        }
    }

    bool PlaceAirfield()
    {
        // If not touching screen
        if (Input.touchCount == 0) return false;
        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return false;
        
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
        return true;
    }

    void SpawnAirplane()
    {
        if (_airplaneController.IsDestroyed()) Destroy(airplane);
        airplane = Instantiate(AirplanePrefab, airplaneSpawner.transform);
        airplane.name = "PlayerPlane";
        HudControls.getNewAirplane();
    }

    public void isPlayerReady() { gameState = 3; }
    public bool AirfieldIsPlaced() { return _airfieldExists; }
}

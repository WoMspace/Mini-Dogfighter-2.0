using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
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
    // ReSharper disable once CollectionNeverQueried.Local
    private List<ARAnchor> anchors;
    public GameObject ARCamera;
    
    // The "Play" Button
    public GameObject HUD_Prefab;
    private GameObject HUD;
    private HudControls HUD_Script;
    public GameObject StartButton;
    
    // Game state stuff
    private int gameState; // See Update() for meanings.
    public int GameState() {return gameState; }
    private static bool stateChanged;
    private static bool _paused;
    public static bool Paused() {return _paused;}
    public static bool StateChanged() {return stateChanged;}

    public TextMeshProUGUI debugText;
    private int debugNumber = 0;
    
    // Airfield
    public GameObject AirfieldPrefab;
    private bool _airfieldExists;
    private GameObject Airfield;
    
    // Airplane
    private GameObject airplaneSpawner;
    public GameObject AirplanePrefab;
    [CanBeNull] private GameObject airplane;
    [CanBeNull] private AirplaneController _airplaneController;
    
    // Debug
    private const bool verbose = true;
    private bool warnedAboutCase3 = false;
    private bool updateRunning = false;

    void EnforceConsecutive(bool stillRunning)
    {
        if (stillRunning)
        {
            Debug.LogError("WOM: AIRFIELDMANAGER: UPDATE: Never finished but is running again!");
        }
    }

    void Awake()
    {
        _planeManager = GetComponent<ARPlaneManager>();
        _raycastManager = GetComponent<ARRaycastManager>();
        _anchorManager = GetComponent<ARAnchorManager>();

        airplaneSpawner = GameObject.Find("AirplaneSpawner");
        // _airplaneController = airplane.GetComponent<AirplaneController>();
        debugGameState();
        Time.timeScale = 1f;
    }
    void Start()
    {
        gameState = 1;
        debugGameState();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("WOM: AIRFIELDMANAGER: UPDATE: BEGIN");
        EnforceConsecutive(updateRunning);
        updateRunning = true;
        debugGameState();
        int oldState = gameState;
        switch (gameState)
        {
            case 0: //Not started, only tracking.
                if(verbose) Debug.Log("WOM: AIRFIELDMANAGER: UPDATE: Case 0");
                break;
            case 1: // Ready to place airfield.
                // if (PlaceAirfield()) gameState = 2;
                if(verbose) Debug.Log("WOM: AIRFIELDMANAGER: UPDATE: Case 1");
                PlaceAirfield();
                break;
            case 2: // Waiting for player to be ready
                if(verbose) Debug.Log("WOM: AIRFIELDMANAGER: UPDATE: Case 2");
                if (stateChanged)
                {
                    if(verbose) Debug.Log("WOM: AIRFIELDMANAGER: UPDATE: CASE 2: State changed; First Run");
                    if(!HUD) HUD = Instantiate(HUD_Prefab);
                    if (!StartButton) StartButton = GameObject.FindWithTag("PlayButton");
                    // playButton = GameObject.Find("PlayButton").GetComponent<HudControls>();
                    HUD_Script = HUD.GetComponent<HudControls>();
                    Debug.Log(HUD_Script);

                    if(verbose) Debug.Log("WOM: AIRFIELDMANAGER: UPDATE: CASE 2: Got HudControls");
                    break;
                }
                if (HUD_Script.isPlayerReady())
                {
                    if (verbose) Debug.Log("WOM: AIRFIELDMANAGER: UPDATE: CASE 2: Player is ready. Switching to gamestate 3.");
                    gameState = 3;
                }
                else
                {
                    if(verbose) Debug.Log("WOM: AIRFIELDMANAGER: UPDATE: CASE 2: Player is not ready. Exiting Switch.");
                }
                break;
            case 3: // Player ready. Starting game.
                if(verbose) Debug.Log("WOM: AIRFIELDMANAGER: UPDATE: Case 3");
                if (stateChanged)
                { // should only last one frame. Could foreseeably get stuck here.
                    // HUD_Script.Hide("PlayButton");
                    HUD_Script.Hide(StartButton);
                    Debug.Log("WOM: Hid play button.\nGoing to spawn airplane...");
                    SpawnAirplane();
                
                    Debug.Log("WOM: Spawned Airplane");
                    gameState = 4;
                    Debug.Log("WOM: Changed to gamestate 4");
                }
                if (!stateChanged)
                { // This should never happen <_<
                    if(!warnedAboutCase3)
                    {
                        Debug.LogError("WOM: Update Ran case:3 more than once.");
                        Debug.LogError($"WOM: Airplane : {airplane}");
                        Debug.LogError($"WOM: HUD : {HUD}");
                        Debug.LogError($"WOM: Airfield : {Airfield}");
                        Debug.LogError($"WOM: StartButton : {StartButton}");
                        // Environment.Exit(0);
                        gameState = 4;
                        warnedAboutCase3 = true;
                        Debug.LogError("Overrode gamestate to 4.");
                    }
                    else
                    {
                        Debug.LogError("WOM: Update Ran case:3 more than twice after explicitly setting gameState to 4.");
                        Debug.LogError("Gamestate: " + gameState);
                        Environment.FailFast("Forcefully exiting.");
                    }
                }
                break;
            case 4: // Flight in progress.
                if(verbose) Debug.Log("WOM: AIRFIELDMANAGER: UPDATE: Case 4");
                if (_paused) gameState = 5;
                break;
            case 5: // Game in progress, paused.
                if(verbose) Debug.Log("WOM: AIRFIELDMANAGER: UPDATE: Case 5");
                if (!_paused) {gameState = 4; return;}
                break;
            case 6: // Aircraft destroyed.
                if(verbose) Debug.Log("WOM: AIRFIELDMANAGER: UPDATE: Case 6");
                break;
            case 7: // Game ended. Show Menu.
                if(verbose) Debug.Log("WOM: AIRFIELDMANAGER: UPDATE: Case 7");
                break;
            default: // This shouldn't be possible.
                Debug.LogError($"WOM: UPDATE: Game State reached impossible value: {gameState}");
                Debug.LogError($"WOM: UPDATE: Ligma Balls.");
                throw new Exception("WOM: UPDATE: Game State reached impossible value.");
        }
        if(verbose) Debug.Log("WOM: AIRFIELDMANAGER: UPDATE: Switch Finished.");
        stateChanged = oldState != gameState;
        if (stateChanged)
        {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (_paused) Time.timeScale = 0f;
            else Time.timeScale = 1f;
            Debug.Log("WOM: AIRFIELDMANAGER: UPDATE: State changed from " + oldState + " => " + gameState);
        }
        else
        {
            Debug.Log("WOM: AIRFIELDMANAGER: UPDATE: State did not change: " + gameState);
        }

        debugNumber++;
        updateRunning = false;
        Debug.Log("WOM: AIRFIELDMANAGER: UPDATE: END");
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
            Airfield = Instantiate(AirfieldPrefab, anchor.transform);
            gameState = 2;
            stateChanged = true;

            if (anchor == null)
            {
                Debug.Log("WOM: Error creating anchor.");
            }
            else
            {
                // Stores the anchor so that it may be removed later.
                anchors.Add(anchor);
            }

        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    void SpawnAirplane()
    {
        if (_airplaneController.IsDestroyed()) { Destroy(airplane); }
        airplane = Instantiate(AirplanePrefab, airplaneSpawner.transform);
        Debug.Log("Spawned Airplane");
        System.Diagnostics.Debug.Assert(airplane != null, nameof(airplane) + " != null");
        airplane.name = "PlayerPlane";
        _airplaneController = airplane.GetComponent<AirplaneController>();
        HudControls.getNewAirplane();
        Debug.Log($"New Airplane name: {airplane.name}");
    }

    // public void isPlayerReady() { gameState = 3; }
    public bool AirfieldIsPlaced() { return _airfieldExists; }

    void debugGameState()
    {
        debugText.text = $"{debugNumber}\n" +
                         $"gameState: {gameState}\n" +
                         $"Timescale: {Time.timeScale}\n" +
                         $"Paused: {_paused}";
    }

    public void setGameState(int state)
    {
        gameState = state;
    }
}

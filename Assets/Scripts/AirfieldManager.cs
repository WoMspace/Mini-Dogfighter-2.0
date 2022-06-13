using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Object = UnityEngine.Object;

// ReSharper disable StringLiteralTypo

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
    private int oldGameState;
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
    public bool verbose;
    private bool warnedAboutCase3 = false;
    private int case3Runs = 0;
    private bool updateRunning = false;

    void EnforceConsecutive(bool stillRunning)
    {
        if (stillRunning)
        {
            Debug.LogError("WOM:AIRFIELDMANAGER:UPDATE: Never finished but is running again!");
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
        
        verbose = Debug.isDebugBuild;
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
        if (verbose)
        {
            Debug.Log("WOM:AIRFIELDMANAGER:UPDATE: BEGIN");
            EnforceConsecutive(updateRunning);
            debugGameState();
        }
        updateRunning = true;
        stateChanged = oldGameState != gameState;
        oldGameState = gameState;
        switch (gameState)
        {
            case 0: //Not started, only tracking.
                if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE: Case 0");
                break;
            case 1: // Ready to place airfield.
                if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE: Case 1");
                PlaceAirfield();
                if (Airfield != null) gameState = 2;
                break;
            case 2: // Waiting for player to be ready
                if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE: Case 2");
                if (stateChanged)
                {
                    if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE:CASE2: State changed; First Run");
                    // /* if(!HUD) */ HUD = Instantiate(HUD_Prefab);
                    // /* if (!StartButton) */ StartButton = GameObject.FindWithTag("PlayButton");
                    // playButton = GameObject.Find("PlayButton").GetComponent<HudControls>();

                    HUD = Instantiate(HUD_Prefab);
                    if(verbose) Debug.Log($"WOM:AIRFIELDMANAGER:UPDATE:CASE2: HUD : {HUD}");
                    HUD_Script = HUD.GetComponent<HudControls>();
                    HUD_Script = GameObject.FindWithTag("PlayButton").GetComponent<HudControls>();
                    if(verbose) Debug.Log($"WOM:AIRFIELDMANAGER:UPDATE:CASE2: HUD_Script : {HUD_Script}");
                    StartButton = GameObject.FindWithTag("PlayButton");
                    if(verbose) Debug.Log($"WOM:AIRFIELDMANAGER:UPDATE:CASE2: Got StartButton : {StartButton}");
                    
                    if (HUD_Script == null)
                    {
                        Debug.LogError("WOM:AIRFIELDMANAGER:UPDATE:CASE2: HudControls is null!");
                        Environment.FailFast("WOM: Forcefully Exiting");
                    }

                    if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE:CASE2: Got HudControls");
                    break;
                }
                if (HUD_Script.isPlayerReady())
                {
                    if (verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE:CASE2: Player is ready. Switching to gamestate 3.");
                    gameState = 3;
                }
                else
                {
                    if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE:CASE2: Player is not ready. Exiting Switch.");
                }
                break;
            case 3: // Player ready. Starting game.
                if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE: CASE 3");
                // if (stateChanged)
                // { // should only last one frame. Could foreseeably get stuck here.
                    // HUD_Script.Hide("PlayButton");
                if(verbose) Debug.Log($"WOM:AIRFIELDMANAGER:UPDATE:CASE3: Hiding StartButton : {StartButton}");
                HUD_Script.Hide(StartButton);
                if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE:CASE3: Hid StartButton.\nGoing to spawn airplane...");
                // try
                // {
                    GameObject tmp = SpawnAirplane();
                    if(verbose) Debug.Log($"WOM:AIRFIELDMANAGER:UPDATE:CASE3: Spawned airplane : {tmp}");
                // }
                // catch(Exception e){Debug.LogError($"WOM:AIRFIELDMANAGER:UPDATE:CASE3: {e}");}
            
                if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE:CASE3: Spawned Airplane");
                gameState = 4;
                if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE:CASE3: Changed to gamestate 4");
                // }
                if (case3Runs > 1)
                { // This should never happen <_<
                    if(case3Runs > 2 || warnedAboutCase3)
                    {
                        Debug.LogError("WOM:AIRFIELDMANAGER:UPDATE:CASE3: Update Ran case:3 more than once.");
                        Debug.LogError($"WOM:AIRFIELDMANAGER:UPDATE:CASE3: Airplane : {airplane}");
                        Debug.LogError($"WOM:AIRFIELDMANAGER:UPDATE:CASE3: HUD : {HUD}");
                        Debug.LogError($"WOM:AIRFIELDMANAGER:UPDATE:CASE3: Airfield : {Airfield}");
                        Debug.LogError($"WOM:AIRFIELDMANAGER:UPDATE:CASE3: StartButton : {StartButton}");
                        // Environment.Exit(0);
                        gameState = 4;
                        warnedAboutCase3 = true;
                        Debug.LogError("WOM:AIRFIELDMANAGER:UPDATE:CASE3: Overrode gamestate to 4.");
                    }
                    else
                    {
                        Debug.LogError("WOM:AIRFIELDMANAGER:UPDATE:CASE3: Update Ran case:3 more than twice after explicitly setting gameState to 4.");
                        Debug.LogError("WOM:AIRFIELDMANAGER:UPDATE:CASE3: Gamestate: " + gameState);
                        Environment.FailFast("WOM:AIRFIELDMANAGER:UPDATE:CASE3: Forcefully exiting.");
                    }
                }
                break;
            case 4: // Flight in progress.
                if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE: CASE 4");
                if (_paused) gameState = 5;
                break;
            case 5: // Game in progress, paused.
                if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE: CASE 5");
                if (!_paused) {gameState = 4; return;}
                break;
            case 6: // Aircraft destroyed.
                if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE: CASE 6");
                break;
            case 7: // Game ended. Show Menu.
                if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE: CASE 7");
                break;
            default: // This shouldn't be possible.
                Debug.LogError($"WOM:AIRFIELDMANAGER:UPDATE: Game State reached impossible value: {gameState}");
                Debug.LogError($"WOM:AIRFIELDMANAGER:UPDATE: Ligma Balls.");
                throw new Exception("WOM:AIRFIELDMANAGER:UPDATE: Game State reached impossible value.");
        }
        if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE: Switch Finished.");
        stateChanged = oldGameState != gameState;
        if (stateChanged)
        {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (_paused) Time.timeScale = 0f;
            else Time.timeScale = 1f;
            if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE: State changed from " + oldGameState + " => " + gameState);
        }
        else
        {
            if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE: State did not change: " + gameState);
        }

        debugNumber++;
        updateRunning = false;
        if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:UPDATE: END");
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
            airplaneSpawner = GameObject.FindWithTag("Respawn");

            if (anchor == null)
            {
                if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:PlaceAirfield: Error creating anchor.");
            }
            else
            {
                // Stores the anchor so that it may be removed later.
                anchors.Add(anchor);
            }

        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    GameObject SpawnAirplane()
    {
        if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:SpawnAirplane: Beginning");
        // Debug.Log($"WOM:AIRFIELDMANAGER:SpawnAirplane: AirplanePrefab : {AirplanePrefab}");
        debugRuntimeObject(AirplanePrefab, "AirplanePrefab", true, true, "WOM:AIRFIELDMANAGER:SpawnAirPlane:");
        // Debug.Log($"WOM:AIRFIELDMANAGER:SpawnAirplane: _airplaneController : {_airplaneController}");
        debugRuntimeObject(_airplaneController, "_airplaneController", false, false, "WOM:AIRFIELDMANAGER:SpawnAirPlane:");
        // Debug.Log($"WOM:AIRFIELDMANAGER:SpawnAirplane: airplaneSpawner : {airplaneSpawner}");
        debugRuntimeObject(airplaneSpawner, "airplaneSpawner", true, true, "WOM:AIRFIELDMANAGER:SpawnAirPlane:");
        // if (_airplaneController != null)
        // {
        //     if (_airplaneController.IsDestroyed()) { Destroy(airplane); }
        // }
        // Debug.Log($"WOM:AIRFIELDMANAGER:SpawnAirplane AirplanePrefab : " + AirplanePrefab == null ? "null" : AirplanePrefab.ToString());
        // Debug.Log($"WOM:AIRFIELDMANAGER:SpawnAirplane airplaneSpawner : " + airplaneSpawner == null ? "null" : airplaneSpawner.ToString());
        
        if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:SpawnAirplane: Going for airplane instantiate!");
        try
        {
            airplane = Instantiate(AirplanePrefab, airplaneSpawner.transform);
            if(verbose) Debug.Log($"WOM:AIRFIELDMANAGER:SpawnAirplane: Successfully spawned airplane!");
        }
        catch (Exception e)
        {
            Debug.LogError($"WOM:AIRFIELDMANAGER:SpawnAirplane: {e}");
            Environment.FailFast($"WOM:AIRFIELDMANAGER:SpawnAirplane: Couldn't instantiate {AirplanePrefab} at location of {airplaneSpawner}");
        }
        
        if(verbose) Debug.Log("WOM:AIRFIELDMANAGER:SpawnAirplane: Spawned Airplane");
        System.Diagnostics.Debug.Assert(airplane != null, nameof(airplane) + " != null"); // This is for Rider linting
        airplane.name = "PlayerPlane";
        airplane.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        _airplaneController = airplane.GetComponent<AirplaneController>();
        _airplaneController.GetComponents(airplane);
        HudControls.getNewAirplane();
        if(verbose) Debug.Log($"WOM:AIRFIELDMANAGER:SpawnAirplane: Complete. New Airplane name: {airplane.name}");
        return airplane;
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

    void debugRuntimeObject([CanBeNull] Object item, string name, bool errorOnNull = false, bool exceptionOnNull = false, string context = "WOM:AIRFIELDMANAGER:")
    {
        bool isNull = item == null;
        var message = isNull ? $"{name} is null!" : $"{name} : {item}";

        string log = $"{context} {message}";

        if (verbose)
        {
            if (isNull && errorOnNull) Debug.LogError(log);
            if (isNull && exceptionOnNull) throw new NullReferenceException($"{name} is null!");
            if (!isNull) Debug.Log(log);
        }
    }

    public void setGameState(int state)
    {
        gameState = state;
    }
}

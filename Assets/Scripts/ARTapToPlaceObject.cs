using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Services;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARTapToPlaceObject : MonoBehaviour
{
    private ARRaycastManager _raycastManager;
    private ARPlaneManager _planeManager;

    /// <summary>
    /// Store placement indicator position and rotation.
    /// </summary>
    private Pose _placementPose;

    /// <summary>
    /// Shows if placement indicator is able to be placed.
    /// </summary>
    private bool _placementPoseIsValid;

    /// <summary>
    /// Placement indicator prefab.
    /// </summary>
    [SerializeField] private GameObject _placementIndicator;
   
    /// <summary>
    /// Prop indicator prefab.
    /// </summary>
    [SerializeField] private GameObject _propPrefab;

    /// <summary>
    /// Line object that is used to draw walls edges.
    /// </summary>
    [SerializeField] private LineRenderer _perimeterLine;
    
    /// <summary>
    /// Floor height value.
    /// </summary>
    private float _floorLevel;
    
    /// <summary>
    /// Indicates if floor level was calibrated.
    /// </summary>
    private bool _isFloorLevel;

    /// <summary>
    /// Indicates if room corners have been added.
    /// </summary>
    private bool _isPerimeterReady;

    /// <summary>
    /// Indicator object.
    /// </summary>
    private GameObject _indicator;
    
    /// <summary>
    /// Added props list.
    /// </summary>
    private List<GameObject> _props;

    /// <summary>
    /// Room prefab to be instantiated.
    /// </summary>
    [SerializeField] private RoomData _roomPrefab;
    
    /// <summary>
    /// Instantiated room.
    /// </summary>
    private RoomData _room;

    /// <summary>
    /// Text field to show distance between corners.
    /// </summary>
    [SerializeField] private Text _distanceText;
    
    /// <summary>
    /// Service to sync AR room and room from JSON.
    /// </summary>
    private IRoomSyncService _roomSyncService;

    /// <summary>
    /// Main camera object.
    /// </summary>
    private Camera _camera;
    
    private void Awake()
    {
        _roomSyncService = ServiceLocator.GetService<IRoomSyncService>();
    }

    void Start()
    {        
        _camera = Camera.main;
        _raycastManager = FindObjectOfType<ARRaycastManager>();
        _planeManager = FindObjectOfType<ARPlaneManager>();
        _perimeterLine.positionCount = 0;
        _room = Instantiate(_roomPrefab);
        _props = new List<GameObject>();
        SetPlacementIndicator();
        RoomPlanVisualizer.Instance.OnRoomConfig += SetPerimeter;
    }

    void Update()
    {
        UpdatePlacementPoint();
        UpdateIndicator();
        if (!_room) return;
        UpdateDistanceValue();
    }   

    /// <summary>
    /// Calculates current wall length.
    /// </summary>
    private void UpdateDistanceValue()
    {
        if (_room.corners.Count == 0) return;

        _distanceText.text =$"Length: {Math.Round(Vector3.Distance(_room.corners.Last().transform.position, _placementPose.position), 2)} m";
    }

    /// <summary>
    /// Creates AR room corner key point.
    /// </summary>
    public void CreateCornerPoint() 
    {
        _room.SetCorner(_placementPose.position);
        var positionCount = _perimeterLine.positionCount;
        positionCount += 1;
        _perimeterLine.positionCount = positionCount;
        _perimeterLine.SetPosition(positionCount - 1, _placementPose.position);        
    }
    
    /// <summary>
    /// For the proof of concept it generates test hardcoded room.
    /// </summary>
    public void GenerateRoomFromJson()
    {
        _roomSyncService.GenerateTestRoom();
    }

    /// <summary>
    /// Disables plane manager to stop defining new surfaces and synchronizing AR room and room generated from JSON.
    /// </summary>
    private void SetPerimeter() 
    {
        _perimeterLine.loop = true;
        _isPerimeterReady = true;
        _planeManager.enabled = false;
        
        _roomSyncService.SynchronizeRooms(_room);
    }

    /// <summary>
    /// Resets all managers, rooms and placed points and props.
    /// </summary>
    public void ResetPerimeter()
    {
        _perimeterLine.positionCount = 0;
        _planeManager.enabled = true;
        SetPlacementIndicator();
        _perimeterLine.loop = false;
        _isFloorLevel = false;
        _isPerimeterReady = false;
        _props.ForEach(Destroy);
        _props.Clear();
        Destroy(_room.gameObject);
        _room = Instantiate(_roomPrefab);
    }

    /// <summary>
    /// Calibrates the floor level in app world coordinates.
    /// </summary>
    public void SetFloorLevel() 
    {
        _floorLevel = _placementPose.position.y;
        _isFloorLevel = true;
    }

    /// <summary>
    /// Applies placement indicator prefab.
    /// </summary>
    private void SetPlacementIndicator() 
    {       
        _indicator = _placementIndicator;
        _placementIndicator.SetActive(true);                
        _propPrefab.SetActive(false);
    }

    /// <summary>
    /// Applies prop indicator prefab.
    /// </summary>
    public void SetPropIndicator() 
    {
        _indicator = _propPrefab;
        _placementIndicator.SetActive(false);
        _propPrefab.SetActive(true);
    }

    /// <summary>
    /// Adding prop to the scene.
    /// </summary>
    public void PlaceProp() 
    {
        SetPlacementIndicator();
        if (_placementPoseIsValid)
        {
            var minDistance = _room.corners.Min(c => Vector3.Distance(c.transform.position, _placementPose.position));
            
            var prop = Instantiate(_propPrefab, _room.corners.First(c => Math.Abs(Vector3.Distance(c.transform.position, _placementPose.position) - minDistance) < 0.01f).transform.position, _placementPose.rotation);
            prop.SetActive(true);
            _props.Add(prop);
        }
    }

    /// <summary>
    /// Updates indicator state.
    /// </summary>
    private void UpdateIndicator()
    {
        if (_placementPoseIsValid)
        {
            _indicator.SetActive(true);
            _indicator.transform.SetPositionAndRotation(_placementPose.position, _placementPose.rotation);
        }
        else 
        {
            _indicator.SetActive(false);
        }
    }

    /// <summary>
    /// Calculates placement indicator position and rotation.
    /// </summary>
    private void UpdatePlacementPoint() 
    {
        var screenCenter = _camera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0.5f));
        
        if (_isPerimeterReady)
        {
            RaycastHit hit;
            Physics.Raycast(_camera.ScreenPointToRay(screenCenter), out hit);

            if (hit.collider != null)
            {
                _placementPoseIsValid = true;
                _placementPose.position = hit.point;
                _placementPose.rotation = Quaternion.LookRotation(hit.normal);
            }
            else 
            {
                _placementPoseIsValid = false;
            }
        }
        else 
        {
            var hits = new List<ARRaycastHit>();
            
            _raycastManager.Raycast(screenCenter, hits, TrackableType.Planes);

            _placementPoseIsValid = hits.Count > 0;

            if (_placementPoseIsValid)
            {
                _placementPose = hits[0].pose;
                if (_isFloorLevel)
                {
                    _placementPose.position.y = _floorLevel;
                }

                var cameraForward = _camera.transform.forward;
                var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
                _placementPose.rotation = Quaternion.LookRotation(cameraBearing);
            }
            else 
            {
                var planeHits = _planeManager.Raycast(_camera.ScreenPointToRay(screenCenter), TrackableType.Planes, Allocator.Temp);
                if (planeHits.Length > 0) 
                {
                    _placementPose = planeHits[0].pose;

                    if (_isFloorLevel)
                    {
                        _placementPose.position.y = _floorLevel;
                    }

                    var cameraForward = _camera.transform.forward;
                    var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
                    _placementPose.rotation = Quaternion.LookRotation(cameraBearing);
                }        
                
            }
        }
        
    }
}

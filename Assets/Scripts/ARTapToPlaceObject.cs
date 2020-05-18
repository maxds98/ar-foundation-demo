using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

    private Pose _placementPose;

    private bool _placementPoseIsValid = false;

    [SerializeField] private GameObject _placementIndicator;
   
    [SerializeField] private GameObject _propPrefab;

    [SerializeField] private LineRenderer _perimeterLine;
    private float _floorLevel;
    private bool _isFloorLevel;

    private bool _isPerimeterReady = false;

    private GameObject _indicator;
    
    private List<GameObject> _props;
    private Camera _camera;

    [SerializeField] private RoomData _roomPrefab;
    
    private RoomData _room;

    [SerializeField] private Text _distanceText;
    [SerializeField] private Text _angleText;

    void Start()
    {        
        _camera = Camera.main;
        _raycastManager = FindObjectOfType<ARRaycastManager>();
        _planeManager = FindObjectOfType<ARPlaneManager>();
        _perimeterLine.positionCount = 0;
        _room = Instantiate(_roomPrefab);
        _props = new List<GameObject>();
        SetPlacementIndicator();
    }

    void Update()
    {
        UpdatePlacementPoint();
        UpdateIndicator();
        if (!_room) return;
        UpdateDistanceValue();
        UpdateCornerAngle();
    }

    private void UpdateCornerAngle()
    {
        if (_room.corners.Count < 2) return;

        var previousCornerPos = _room.corners[_room.corners.Count - 2].Position;
        var currentCornerPos = _room.corners.Last().Position;
        
        _angleText.text =$"Corner: {Math.Round(Utils.GetSignedAngle(previousCornerPos - currentCornerPos, _placementPose.position - currentCornerPos), 2)}°";
    }

    private void UpdateDistanceValue()
    {
        if (_room.corners.Count == 0) return;

        _distanceText.text =$"Length: {Math.Round(Vector3.Distance(_room.corners.Last().transform.position, _placementPose.position), 2)} m";
    }

    public void CreateCornerPoint() 
    {
        _room.SetCorner(_placementPose.position);
        _perimeterLine.positionCount += 1;
        _perimeterLine.SetPosition(_perimeterLine.positionCount - 1, _placementPose.position);        
    }

    public void SetPerimeter() 
    {
        _perimeterLine.loop = true;
        _isPerimeterReady = true;
        _planeManager.enabled = false;
        
        _room.SetPerimeter();
        
        RoomSynchronizer.Instance.SynchronizeRooms(_room);
    }

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

    public void SetFloorLevel() 
    {
        _floorLevel = _placementPose.position.y;
        _isFloorLevel = true;
    }

    private void SetPlacementIndicator() 
    {       
        _indicator = _placementIndicator;
        _placementIndicator.SetActive(true);                
        _propPrefab.SetActive(false);
    }

    public void SetPropIndicator() 
    {
        _indicator = _propPrefab;
        _placementIndicator.SetActive(false);
        _propPrefab.SetActive(true);
    }

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

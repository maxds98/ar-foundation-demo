using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARTapToPlaceObject : MonoBehaviour
{
    private ARRaycastManager _raycastManager;
    private ARPlaneManager _planeManager;

    private Pose _placementPose;

    private bool _placementPoseIsValid = false;

    [SerializeField] private GameObject _placementIndicator;

    [SerializeField] private GameObject _cornerPointPrefab;
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private GameObject _propPrefab;

    [SerializeField] private LineRenderer _perimeterLine;
    private float _floorLevel;
    private bool _isFloorLevel;

    private bool _isPerimeterReady = false;

    private GameObject _indicator;

    private List<GameObject> _corners;
    private List<GameObject> _walls;
    private List<GameObject> _props;
    private Camera _camera;

    void Start()
    {        
        _camera = Camera.main;
        _raycastManager = FindObjectOfType<ARRaycastManager>();
        _planeManager = FindObjectOfType<ARPlaneManager>();
        _perimeterLine.positionCount = 0;
        _corners = new List<GameObject>();
        _walls = new List<GameObject>();
        _props = new List<GameObject>();
        SetPlacementIndicator();
    }

    void Update()
    {
        UpdatePlacementPoint();
        UpdateIndicator();
    }       

    public void CreateCornerPoint() 
    {
        var cornerPoint = Instantiate(_cornerPointPrefab);
        _corners.Add(cornerPoint);
        cornerPoint.transform.position = _placementPose.position;
        _perimeterLine.positionCount += 1;
        _perimeterLine.SetPosition(_perimeterLine.positionCount - 1, _placementPose.position);        
    }

    public void SetPerimeter() 
    {
        _perimeterLine.loop = true;
        _isPerimeterReady = true;
        _planeManager.enabled = false;
        
        for (int i = 0; i < _corners.Count; i++)
        {
            var currentCornerPos = _corners[i].transform.position;
            var nextCurnerPos = _corners[i < _corners.Count - 1 ? i + 1 : 0].transform.position;
            var wall = Instantiate(_wallPrefab);
            _walls.Add(wall);
            var wallMeshFilter = wall.GetComponent<MeshFilter>();

            var triangles = new int[] 
            {
                0, 3, 1,
                3, 0, 2,
                1, 3, 0,
                2, 0, 3
            };
            
            var normals = wallMeshFilter.mesh.normals;

            wallMeshFilter.mesh = new Mesh();

            var vertices = new Vector3[4];

            vertices[0] = currentCornerPos;
            vertices[1] = currentCornerPos + Vector3.up * 2.5f;
            vertices[2] = nextCurnerPos;
            vertices[3] = nextCurnerPos + Vector3.up * 2.5f;

            wallMeshFilter.mesh.vertices = vertices;
            wallMeshFilter.mesh.triangles = triangles;
            wallMeshFilter.mesh.normals = normals;

            wall.AddComponent<MeshCollider>();
        }
    }

    public void ResetPerimeter()
    {
        _perimeterLine.positionCount = 0;
        _planeManager.enabled = true;
        SetPlacementIndicator();
        _perimeterLine.loop = false;
        _isFloorLevel = false;
        _isPerimeterReady = false;
        _corners.ForEach(Destroy);
        _walls.ForEach(Destroy);
        _props.ForEach(Destroy);
        _walls.Clear();
        _props.Clear();
        _corners.Clear();
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
            var prop = Instantiate(_propPrefab, _placementPose.position, _placementPose.rotation);
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
                var planeHits = _planeManager.Raycast(_camera.ScreenPointToRay(screenCenter), TrackableType.Planes, new Allocator());
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

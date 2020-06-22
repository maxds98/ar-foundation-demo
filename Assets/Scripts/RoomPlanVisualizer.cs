using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomPlanVisualizer : MonoBehaviour
{
    public static RoomPlanVisualizer Instance;

    /// <summary>
    /// Line object to draw 2D plan walls.
    /// </summary>
    private LineRenderer _walls;

    /// <summary>
    /// List of plan corners.
    /// </summary>
    private List<GameObject> _cornerObjects = new List<GameObject>();
    
    /// <summary>
    /// List of plan props.
    /// </summary>
    private List<GameObject> _propObjects = new List<GameObject>();

    /// <summary>
    /// List of corner indexes to sync rooms.
    /// </summary>
    public List<int> cornerIndexes = new List<int>();

    /// <summary>
    /// Action that invoked when the sync corners ware marked on 2D plan visualisation.
    /// </summary>
    public event Action OnRoomConfig;

    /// <summary>
    /// Indicates if corners ware marked.
    /// </summary>
    private bool _isMarked;

    /// <summary>
    /// corner prefab to instantiate on 2D plan.
    /// </summary>
    [SerializeField] private GameObject cornerPref;
    
    /// <summary>
    /// Main camera object.
    /// </summary>
    private Camera _camera;
    
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        _walls = GetComponentInChildren<LineRenderer>();
        _camera = Camera.main;
    }

    void Update()
    {
        if(_isMarked) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            
            Physics.Raycast(ray, out hit);
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Corner"))
                {
                    hit.collider.GetComponent<MeshRenderer>().material.color = Color.red;
                    cornerIndexes.Add(_cornerObjects.IndexOf(hit.collider.gameObject));
                }
            }
        }
        
        if (cornerIndexes.Count >= 2)
        {
            _walls.enabled = false;
            _cornerObjects.ForEach(c => c.SetActive(false));
            _propObjects.ForEach(p => p.SetActive(false));
            OnRoomConfig?.Invoke();
            _isMarked = true;
        }
    }

    /// <summary>
    /// Generates 2D plan of the room with the corners, props and walls on it.
    /// </summary>
    /// <param name="room">Room data to be built.</param>
    public void Build2DPlan(RoomData room)
    {
        _walls.enabled = true;
        _walls.positionCount = room.corners.Count;
        var positions = from corner in room.corners select corner.Position;
        _walls.SetPositions(positions.ToArray());
        foreach (var corner in room.corners)
        {
            var c = Instantiate(cornerPref, transform);
            c.transform.localPosition = corner.Position;
            c.tag = "Corner";
            _cornerObjects.Add(c);
        }

        foreach (var prop in room.props)
        {
            var p = Instantiate(prop, transform);
            p.transform.localPosition = prop.transform.localPosition;
            p.transform.localRotation = prop.transform.localRotation;
            _propObjects.Add(p);
        }
    }

}

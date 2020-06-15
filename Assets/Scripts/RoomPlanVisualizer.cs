using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomPlanVisualizer : MonoBehaviour
{
    public static RoomPlanVisualizer Instance;

    private LineRenderer _walls;

    public List<GameObject> cornerObjects = new List<GameObject>();

    public List<int> cornerIndexes = new List<int>();

    private Camera _camera;

    public event Action OnRoomConfig;

    private bool _isMarked = false;

    [SerializeField] private GameObject cornerPref;
    
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
            Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit);
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Corner"))
                {
                    hit.collider.GetComponent<MeshRenderer>().material.color = Color.red;
                    cornerIndexes.Add(cornerObjects.IndexOf(hit.collider.gameObject));
                }
            }
        }
        
        if (cornerIndexes.Count >= 2)
        {
            _walls.enabled = false;
            cornerObjects.ForEach(c => c.SetActive(false));
            OnRoomConfig?.Invoke();
            _isMarked = true;
        }
    }

    public void BuildWalls(RoomData room)
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
            cornerObjects.Add(c);
        }
    }

}

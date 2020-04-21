using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalWallDetector : MonoBehaviour
{
    public bool overlappingWall = false;
    public bool overlappingEdgeTop = false;
    public bool overlappingEdgeBottom = false;

    public WallDetector topCollider;
    public WallDetector bottomCollider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

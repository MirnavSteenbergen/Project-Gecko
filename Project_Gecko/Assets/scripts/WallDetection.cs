using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDetection : MonoBehaviour
{
    BoxCollider2D coll;

    public LayerMask wallMask;

    public float wallCheckDistance = 1f;
    public float skinWidth = 0.015f;

    float verRaySpacing;
    float horRaySpacing;

    [HideInInspector] public bool wallLeftTop;
    [HideInInspector] public bool wallLeftBottom;
    [HideInInspector] public bool wallRightTop;
    [HideInInspector] public bool wallRightBottom;
    [HideInInspector] public bool wallTopLeft;
    [HideInInspector] public bool wallTopRight;
    [HideInInspector] public bool wallBottomLeft;
    [HideInInspector] public bool wallBottomRight;

    [HideInInspector] public bool wallTop;
    [HideInInspector] public bool wallBottom;
    [HideInInspector] public bool wallLeft;
    [HideInInspector] public bool wallRight;

    //public WallInfo walls;
    
    private void Awake()
    {
        coll = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        // on start, set the appropiate rayspacing for the collider size
        UpdateRaySpacing();
    }

    public void UpdateWallInfo()
    {
        Bounds bounds = coll.bounds;
        bounds.Expand(skinWidth * -2);

        wallTopLeft     = DetectWall(new Vector2(bounds.min.x, bounds.max.y), Vector2.up, Vector2.right);
        wallTopRight    = DetectWall(new Vector2(bounds.max.x, bounds.max.y), Vector2.up, Vector2.left);
        wallBottomLeft  = DetectWall(new Vector2(bounds.min.x, bounds.min.y), Vector2.down, Vector2.right);
        wallBottomRight = DetectWall(new Vector2(bounds.max.x, bounds.min.y), Vector2.down, Vector2.left);
        wallLeftTop     = DetectWall(new Vector2(bounds.min.x, bounds.max.y), Vector2.left, Vector2.down);
        wallLeftBottom  = DetectWall(new Vector2(bounds.min.x, bounds.min.y), Vector2.left, Vector2.up);
        wallRightTop    = DetectWall(new Vector2(bounds.max.x, bounds.max.y), Vector2.right, Vector2.down);
        wallRightBottom = DetectWall(new Vector2(bounds.max.x, bounds.min.y), Vector2.right, Vector2.up);

        wallTop = wallTopLeft || wallTopRight;
        wallBottom = wallBottomLeft || wallBottomRight;
        wallLeft = wallLeftTop || wallLeftBottom;
        wallRight = wallRightTop || wallRightBottom;
    }

    bool DetectWall(Vector2 rayOrigin, Vector2 rayDirection, Vector2 rayTranslateDirecrion)
    {
        bool wallDetected = false;

        //Vector2 transformDirection = -Vector2.Perpendicular(rayDirection);

        for (int i = 0; i < 2; i++)
        {
            rayOrigin += rayTranslateDirecrion * i * (rayTranslateDirecrion.x == 0 ? verRaySpacing : horRaySpacing);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, wallCheckDistance, wallMask);

            Debug.DrawRay(rayOrigin, rayDirection * wallCheckDistance, Color.cyan);

            if (hit)
            {
                wallDetected = true;
            }
        }

        return wallDetected;
    }

    void UpdateRaySpacing()
    {
        Bounds bounds = coll.bounds;
        bounds.Expand(skinWidth * -2);

        verRaySpacing = bounds.size.y * 0.5f;
        horRaySpacing = bounds.size.x * 0.5f;
    }

    //public struct WallInfo
    //{
    //    public bool left, right, top, down;

    //    public bool topLeft, topRight;
    //    public bool bottomLeft, bottomRight;
    //    public bool leftTop, leftBottom;
    //    public bool rightTop, rightBottom;

    //    public void Reset()
    //    {
    //        topLeft = topRight = false;
    //        bottomLeft = bottomRight = false;
    //        leftTop = leftBottom = false;
    //        rightTop = rightBottom = false;
    //    }
    //}
}

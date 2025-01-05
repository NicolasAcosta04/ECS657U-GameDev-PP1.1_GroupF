using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class InPlayerCamera : MonoBehaviour
{
    Camera playerCamera;
    Transform player;
    MeshRenderer renderer;
    Plane[] cameraFrustum;
    Collider collider;
    public Boolean inCamera;
    public LayerMask Wall;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        playerCamera = GameObject.FindWithTag("PlayerCam").GetComponent<Camera>();
        renderer = GetComponent<MeshRenderer>();
        collider = GetComponent<Collider>();
        inCamera = false;
        //StartCoroutine(WallCheckRoutine());
    }

    /*
    private IEnumerator WallCheckRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;
            WallCheck();
        }
    }
    */

    private void WallCheck()
    {
        Vector3 directionToTarget = (player.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, player.position);
        if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, Wall))
        {
            inCamera = true;
        }
        else
        {
            inCamera = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        var bounds = collider.bounds;
        cameraFrustum = GeometryUtility.CalculateFrustumPlanes(playerCamera);
        var inCameraRange = GeometryUtility.TestPlanesAABB(cameraFrustum, bounds);
        if (inCameraRange)
        {
            WallCheck();

        }
        else
        {
            inCamera = false;
        }
    }
    private void OnDrawGizmos()
    {
        if (playerCamera == null || collider == null) return;

        Gizmos.color = Color.green;

        // Arrays to store the frustum corners
        Vector3[] nearCorners = new Vector3[4];
        Vector3[] farCorners = new Vector3[4];

        // Calculate the near and far plane corners in local space
        playerCamera.CalculateFrustumCorners(
            new Rect(0, 0, 1, 1),
            playerCamera.nearClipPlane,
            Camera.MonoOrStereoscopicEye.Mono,
            nearCorners
        );

        playerCamera.CalculateFrustumCorners(
            new Rect(0, 0, 1, 1),
            playerCamera.farClipPlane,
            Camera.MonoOrStereoscopicEye.Mono,
            farCorners
        );

        // Transform corners to world space
        for (int i = 0; i < 4; i++)
        {
            nearCorners[i] = playerCamera.transform.TransformPoint(nearCorners[i]);
            farCorners[i] = playerCamera.transform.TransformPoint(farCorners[i]);
        }

        // Draw near plane
        Gizmos.DrawLine(nearCorners[0], nearCorners[1]);
        Gizmos.DrawLine(nearCorners[1], nearCorners[2]);
        Gizmos.DrawLine(nearCorners[2], nearCorners[3]);
        Gizmos.DrawLine(nearCorners[3], nearCorners[0]);

        // Draw far plane
        Gizmos.DrawLine(farCorners[0], farCorners[1]);
        Gizmos.DrawLine(farCorners[1], farCorners[2]);
        Gizmos.DrawLine(farCorners[2], farCorners[3]);
        Gizmos.DrawLine(farCorners[3], farCorners[0]);

        // Draw lines connecting near and far planes
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(nearCorners[i], farCorners[i]);
        }

        // Draw the collider bounds
        Gizmos.color = Color.red; // Use red color for collider bounds
        Bounds bounds = collider.bounds;

        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        // Calculate the corners of the bounds
        Vector3[] corners = new Vector3[8];
        corners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z); // Bottom-back-left
        corners[1] = center + new Vector3(extents.x, -extents.y, -extents.z);  // Bottom-back-right
        corners[2] = center + new Vector3(-extents.x, -extents.y, extents.z);  // Bottom-front-left
        corners[3] = center + new Vector3(extents.x, -extents.y, extents.z);   // Bottom-front-right
        corners[4] = center + new Vector3(-extents.x, extents.y, -extents.z);  // Top-back-left
        corners[5] = center + new Vector3(extents.x, extents.y, -extents.z);   // Top-back-right
        corners[6] = center + new Vector3(-extents.x, extents.y, extents.z);   // Top-front-left
        corners[7] = center + new Vector3(extents.x, extents.y, extents.z);    // Top-front-right

        // Draw the edges of the bounds
        // Bottom face
        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[1], corners[3]);
        Gizmos.DrawLine(corners[3], corners[2]);
        Gizmos.DrawLine(corners[2], corners[0]);

        // Top face
        Gizmos.DrawLine(corners[4], corners[5]);
        Gizmos.DrawLine(corners[5], corners[7]);
        Gizmos.DrawLine(corners[7], corners[6]);
        Gizmos.DrawLine(corners[6], corners[4]);

        // Connect top and bottom faces
        Gizmos.DrawLine(corners[0], corners[4]);
        Gizmos.DrawLine(corners[1], corners[5]);
        Gizmos.DrawLine(corners[2], corners[6]);
        Gizmos.DrawLine(corners[3], corners[7]);
    }
}
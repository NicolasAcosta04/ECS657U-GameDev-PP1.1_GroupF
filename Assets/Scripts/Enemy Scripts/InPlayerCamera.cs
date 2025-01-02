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
        StartCoroutine(WallCheckRoutine());
    }

    private IEnumerator WallCheckRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;
            WallCheck();
        }
    }

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
            
        } else
        {
            inCamera = false;
        }
    }
}

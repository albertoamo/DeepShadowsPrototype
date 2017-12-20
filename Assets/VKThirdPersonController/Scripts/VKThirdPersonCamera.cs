using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VKThirdPersonCamera : MonoBehaviour
{
    private static VKThirdPersonCamera _instance;
    public static VKThirdPersonCamera instance // Singleton pattern
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<VKThirdPersonCamera>();

            return _instance;
        }
    }

    public GameObject targetLookAt;
    public GameObject dummyTarget;
    public float height = 1.4f;
    public float rightOffset = 0f;
    public float clippingDistance = 2.5f;
    public float maxClippingDistance = 2.5f;
    public float minClippingDistance = 0.1f;

    public Vector2 xLimit = new Vector2(-360, 360);
    public Vector2 yLimit = new Vector2(-40, 80);
    public Vector2 sensitivity = new Vector2(3, 3);
    public float smoothCameraRotation = 12f;

    [HideInInspector] public Vector2 input;

    private Camera _camera;
    private float distance = 5f;
    private float mouseY = 0f;
    private float mouseX = 0f;
    private float currentHeight;


    // Use this for initialization
    void Start ()
    {
        _camera = GetComponent<Camera>();

        // Create a dummy target to look at.
        dummyTarget = new GameObject("VKDummyTarget");
        dummyTarget.transform.parent = this.transform.parent;

        if (targetLookAt != null) SetTarget(targetLookAt);
    }
	
	// Update is called once per frame
	void Update ()
    {

    }

    private void FixedUpdate()
    {
        CameraMove();
        CameraRotate();
    }

    public void SetTarget(GameObject newTarget)
    {
        targetLookAt = newTarget;
        mouseY = targetLookAt.transform.eulerAngles.x;
        mouseX = targetLookAt.transform.eulerAngles.y;
        dummyTarget.transform.position = targetLookAt.transform.position;
        dummyTarget.transform.rotation = targetLookAt.transform.rotation;
    }

    public void CameraRotate()
    {
        // free rotation 
        mouseX += input.x * sensitivity.x;
        mouseY -= input.y * sensitivity.y;

        mouseX = VKUtils.ClampAngle(mouseX, xLimit.x, xLimit.y);
        mouseY = VKUtils.ClampAngle(mouseY, yLimit.x, yLimit.y);
    }

    public void CameraMove()
    {
        if (targetLookAt == null) return;

        // Get director vector to target and offset position
        Vector3 camDir = (-dummyTarget.transform.forward) + (rightOffset * dummyTarget.transform.right).normalized;
        Vector3 targetPos = new Vector3(dummyTarget.transform.position.x, dummyTarget.transform.position.y, dummyTarget.transform.position.z);
        targetPos += new Vector3(0, height, 0);

        var lookPoint = targetPos + dummyTarget.transform.forward * 2f;
        lookPoint += (dummyTarget.transform.right * Vector3.Dot(camDir * (clippingDistance), dummyTarget.transform.right));

        // Apply transforms to the camera
        transform.position = targetPos + (camDir * (clippingDistance));
        transform.rotation = Quaternion.LookRotation((lookPoint) - transform.position);

        // Apply transforms to the target
        Quaternion newRot = Quaternion.Euler(mouseY, mouseX, 0);
        dummyTarget.transform.position = targetLookAt.transform.position;
        dummyTarget.transform.rotation = Quaternion.Slerp(dummyTarget.transform.rotation, newRot, smoothCameraRotation * Time.deltaTime);

    }

    public void CameraClip()
    {
        // TO-DO
        // Clip camera using raycasts
    }
}

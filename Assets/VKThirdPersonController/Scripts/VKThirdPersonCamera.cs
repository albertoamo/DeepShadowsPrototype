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

    [HideInInspector] public GameObject targetLookAt;
    [HideInInspector] public GameObject dummyTarget;
    [HideInInspector] public Vector2 input;
    [HideInInspector] public VKUtils.VKTimer timer;

    [Header("--- Limit/Sensitivity values ---")]
    public Vector2 xLimit = new Vector2(-360, 360);
    public Vector2 yLimit = new Vector2(-40, 80);
    public Vector2 sensitivity = new Vector2(3, 3);

    [Header("--- Offset values ---")]
    public float height = 1.4f;
    public float rightOffset = 0f;
    public float clippingDistance = 2.5f;
    public float maxClippingDistance = 2.5f;
    public float minClippingDistance = 0.1f;
    public float smoothCameraRotation = 12f;
    public const float transitionSpeed = 1f;

    private float mouseY = 0f;
    private float mouseX = 0f;
    private Vector3 targetPos;
    private float currentHeight;

    // Use this for initialization
    public void Init (GameObject target)
    {
        // Create a dummy target to look at.
        dummyTarget = new GameObject("VKDummyTarget");
        dummyTarget.transform.parent = this.transform.parent;
        // Create a timer to control lerps
        timer = new VKUtils.VKTimer(transitionSpeed);

        SetTarget(target);
        timer.accumTime = timer.reachTime; // Move this in future patches
        transform.position = targetLookAt.transform.position;
    }
	
    private void FixedUpdate()
    {
        CameraClip();
        CameraMove();
        CameraRotate();
    }

    public void SetTarget(GameObject newTarget, float time = 0)
    {
        timer.Reset();

        targetLookAt = newTarget;
        mouseY = targetLookAt.transform.eulerAngles.x;
        mouseX = targetLookAt.transform.eulerAngles.y;
        dummyTarget.transform.position = targetLookAt.transform.position;
        dummyTarget.transform.rotation = targetLookAt.transform.rotation;
    }

    public Vector3 GetCameraDirection(Vector2 input)
    {
        if(height == .5f)
            return (input.x * transform.right + input.y * transform.up).normalized;
        else
            return (input.x * transform.right + input.y * transform.forward).normalized;
    }

    public void CameraRotate()
    {
        // free rotation 
        mouseX += input.x * sensitivity.x;
        mouseY -= input.y * sensitivity.y;

        mouseX = VKUtils.ClampAngle(mouseX, xLimit.x, xLimit.y); //Edit this when shadow-diving mode.
        mouseY = VKUtils.ClampAngle(mouseY, yLimit.x, yLimit.y);
    }

    public void CameraMove()
    {
        if (targetLookAt == null) return;

        // Get director vector to target and offset position
        Vector3 camDir = (-dummyTarget.transform.forward) + (rightOffset * dummyTarget.transform.right).normalized;
        targetPos = new Vector3(dummyTarget.transform.position.x, dummyTarget.transform.position.y, dummyTarget.transform.position.z);
        targetPos += height * targetLookAt.transform.up;

        Vector3 lookPoint = targetPos + dummyTarget.transform.forward * 2f;
        //lookPoint += (dummyTarget.transform.right * Vector3.Dot(camDir * (clippingDistance), dummyTarget.transform.right));

        // Sort of lerp timer.
        if (timer.IsAlive())
        {
            Vector3 position = targetPos + (camDir * (clippingDistance));
            Quaternion rotation = Quaternion.LookRotation((lookPoint) - transform.position);
            transform.position = Vector3.Lerp(transform.position, position, timer.accumTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, timer.accumTime);
        }
        else
        {
            transform.position = targetPos + (camDir * (clippingDistance));
            transform.rotation = Quaternion.LookRotation((lookPoint) - transform.position);
        }

        // Apply transforms to the target
        Quaternion newRot = Quaternion.Euler(mouseY, mouseX, 0);
        dummyTarget.transform.position = targetLookAt.transform.position;
        dummyTarget.transform.rotation = Quaternion.Slerp(dummyTarget.transform.rotation, newRot, smoothCameraRotation * Time.deltaTime);
    }

    public void CameraClip()
    {
        // Clip camera using raycasts
        RaycastHit cameraHit;
        Vector3 direction = (transform.position - targetPos).normalized;

        if(Physics.Raycast(new Ray(targetPos, direction), out cameraHit, 5, 5) && !cameraHit.collider.isTrigger)
            clippingDistance = Mathf.Clamp(cameraHit.distance - 0.2f, minClippingDistance, maxClippingDistance);
    }
}

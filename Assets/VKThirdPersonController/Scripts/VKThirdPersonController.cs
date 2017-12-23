using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VKThirdPersonController : MonoBehaviour {

    #region Variables
    public enum StatusType
    {
        IsWalking,
        IsRunning,
        IsCrouching,
        isShadow
    }
    [Header("--- GameObjects ---")]
    public GameObject bodyPlayer;

    [Header("--- Offset values ---")]
    protected float gravity = -9.8f;
    protected float slopeLimit = 45f;
    protected float groundDistance;
    protected float jumpHeight = 4f;
    protected const float maxAirDistance = 0.05f;

    [Header("--- Rotation values ---")]
    public float rotationSpeed = 10f;

    [Header("--- Movement values ---")]
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public float inputSpeed;
    [HideInInspector] public float playerSpeed;
    public const float walkSpeed = 9f;
    public const float runSpeed = 10f;
    public const float crouchSpeed = 7f;
    #endregion

    [HideInInspector] public Vector2 input;
    [HideInInspector] public Vector3 lookDirection;
    [HideInInspector] public StatusType characterStatus;

    #region Components               
    [HideInInspector] public Animator _animator;     
    [HideInInspector] public Rigidbody _rigidbody; 
    [HideInInspector] public CapsuleCollider _capsuleCollider;
    #endregion

    // Use this for initialization
    public void Init()
    {
        playerSpeed = walkSpeed;
        characterStatus = StatusType.IsWalking;

        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    public void UpdateController ()
    {
        CheckGround(); // Gravity physical force
        ControlMotion(); // Movement WASD
        ControlShadow();
    }

    public void UpdateAnimator()
    {
        _animator.SetBool("IsGrounded", isGrounded);
        _animator.SetFloat("InputVertical", inputSpeed * .75f, 0.1f, Time.deltaTime);
    }

    // Function to control player gravity and vertical downforces
    public void CheckGround()
    {
        // Check distance to ground
        RaycastHit groundHit;
        Physics.Raycast(new Ray(transform.position, -transform.up), out groundHit);
        groundDistance = groundHit.distance;

        // Detect wether we are on air or grounded
        if (groundDistance >= maxAirDistance)
        {
            isGrounded = false;
            //_rigidbody.AddForce(transform.up * gravity * Time.deltaTime, ForceMode.Acceleration);
        }
        else
        {
            isGrounded = true;
            //_rigidbody.AddForce(transform.up * (gravity * 2 * Time.deltaTime), ForceMode.Acceleration);
        }
    }

    // Function to control player motion, WASD
    public void ControlMotion()
    {
        // Obtain input strength
        inputSpeed = Mathf.Abs(input.x) + Mathf.Abs(input.y);
        inputSpeed = Mathf.Clamp(inputSpeed, 0, 1f);

        if (input != Vector2.zero)
        {
            // Apply body rotation depending on looking direction
            Quaternion freeRotation = Quaternion.LookRotation(lookDirection, transform.up);
            Quaternion euler = Quaternion.Euler(transform.eulerAngles.x, freeRotation.eulerAngles.y, transform.eulerAngles.z);
            transform.rotation = Quaternion.Lerp(transform.rotation, euler, rotationSpeed * Time.deltaTime);
        }

        Vector3 velY = transform.forward * playerSpeed * inputSpeed;
        velY += Physics.gravity;

        // Apply rigibody force on given direction
        _rigidbody.velocity = velY;
    }

    // ShadowDive control methods

    public void ControlShadow()
    {
        //if (characterStatus == StatusType.isShadow)
        //{
        //    RaycastHit groundHit;
        //    Physics.Raycast(new Ray(transform.position, transform.forward), out groundHit);
        //    Debug.Log(groundHit.distance);
        //    if (groundHit.distance < 0.4f)
        //    {
        //        Debug.Log("Triying to walk on wall");
        //        VKThirdPersonCamera charCamera = FindObjectOfType<VKThirdPersonCamera>();
        //        charCamera.dummyTarget.transform.right = groundHit.normal;
        //        charCamera.targetLookAt.transform.right = groundHit.normal;
        //        this.transform.right = groundHit.normal;
        //        Physics.gravity = -groundHit.normal.normalized * 9.8f;
        //        characterStatus = StatusType.IsCrouching;
        //    }
        //}
    }

    public void UpdatePlayerStatus(GameObject target, bool isShadow)
    {
        if (isShadow)
        {
            bodyPlayer.GetComponent<SkinnedMeshRenderer>().material.color = Color.yellow;
            target.GetComponent<MeshRenderer>().material.color = Color.yellow;
        }
        else
        {
            ExitShadowMode(target);
            bodyPlayer.GetComponent<SkinnedMeshRenderer>().material.color = Color.red;
            target.GetComponent<MeshRenderer>().material.color = Color.green;
        }
    }

    public bool EnterShadowMode(GameObject target)
    {
        if (characterStatus != StatusType.IsWalking) return false;

        characterStatus = StatusType.isShadow;
        this.transform.GetChild(1).gameObject.SetActive(false);
        _capsuleCollider.height = _capsuleCollider.radius;
        _capsuleCollider.center = new Vector3(0, _capsuleCollider.radius, 0);

        VKThirdPersonCamera charCamera = FindObjectOfType<VKThirdPersonCamera>();
        charCamera.SetTarget(target);
        charCamera.height = 0;

        return true;
    }

    public bool ExitShadowMode(GameObject target)
    {
        if (characterStatus != StatusType.isShadow) return false;

        characterStatus = StatusType.IsWalking;
        this.transform.GetChild(1).gameObject.SetActive(true);
        _capsuleCollider.height = 1.9f;
        _capsuleCollider.center = new Vector3(0, 0.96f, 0);

        VKThirdPersonCamera charCamera = FindObjectOfType<VKThirdPersonCamera>();
        charCamera.SetTarget(this.gameObject);
        charCamera.height = 1.4f;

        return true;
    }
}

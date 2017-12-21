using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VKThirdPersonController : MonoBehaviour {

    #region Variables
    public enum StatusType
    {
        IsGrounded,
        IsRunning,
        IsCrouching,
        isShadow
    }

    [Header("--- Offset values ---")]
    protected float gravity = -9.8f;
    protected float slopeLimit = 45f;
    protected float groundDistance;
    protected float jumpHeight = 4f;
    protected float playerHeight = 1.8f;

    [Header("--- Rotation values ---")]
    public float rotationSpeed = 10f;
    public float freeRotationSpeed = 10f;

    [Header("--- Movement values ---")]
    public const float walkSpeed = 2.5f;
    public const float runSpeed = 2.5f;
    public const float crouchSpeed = 2.5f;
    public float playerSpeed;
    public float inputSpeed;
    public bool isGrounded;
    #endregion

    [HideInInspector] public Vector2 input;

    #region Components               
    [HideInInspector] public Animator _animator;     
    [HideInInspector] public Rigidbody _rigidbody; 
    [HideInInspector] public CapsuleCollider _capsuleCollider;
    #endregion

    // Use this for initialization
    public void Init()
    {
        playerSpeed = walkSpeed;

        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    public void UpdateController ()
    {
        CheckGround(); // Gravity physical force
        ControlMotion(); // Movement WASD
    }

    public void UpdateAnimator()
    {
        _animator.SetBool("IsGrounded", isGrounded);
        _animator.SetFloat("InputVertical", inputSpeed * .5f, 0.1f, Time.deltaTime);
    }

    // Function to control player gravity and vertical downforces
    public void CheckGround()
    {
        // Check distance to ground
        RaycastHit groundHit;
        Physics.Raycast(new Ray(transform.position, -Vector3.up), out groundHit);
        groundDistance = groundHit.distance;

        // Detect wether we are on air or grounded
        if (groundDistance >= 0.5f)
        {
            isGrounded = false;
            _rigidbody.AddForce(transform.up * gravity * Time.deltaTime, ForceMode.VelocityChange);
        }
        else
        {
            isGrounded = true;
            _rigidbody.AddForce(transform.up * (gravity * 2 * Time.deltaTime), ForceMode.VelocityChange);
        }
    }

    // Function to control player motion, WASD
    public void ControlMotion()
    {
        // Obtain input strength
        inputSpeed = Mathf.Abs(input.x) + Mathf.Abs(input.y);
        inputSpeed = Mathf.Clamp(inputSpeed, 0, 1f);

        Vector3 velY = transform.forward * playerSpeed * inputSpeed;
        velY.y = _rigidbody.velocity.y;

        if (input != Vector2.zero)
        {
            // Apply body rotation depending on vectors
            Vector3 lookDirection = (input.x * Camera.main.transform.right + input.y * Camera.main.transform.forward).normalized;
            Quaternion freeRotation = Quaternion.LookRotation(lookDirection, transform.up);

            Vector3 euler = new Vector3(transform.eulerAngles.x, freeRotation.eulerAngles.y, transform.eulerAngles.z);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(euler), freeRotationSpeed * Time.deltaTime);
        }

        // Apply rigibody force on given direction
        _rigidbody.velocity = velY; // Block if no input given
        _rigidbody.AddForce(transform.forward * (playerSpeed * inputSpeed) * Time.deltaTime, ForceMode.VelocityChange);
    }
}

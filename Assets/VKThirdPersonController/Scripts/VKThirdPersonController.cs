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
    public float walkSpeed = 2.5f;
    public float runSpeed = 2.5f;
    public float crouchSpeed = 2.5f;
    public bool isGrounded;
    #endregion

    [HideInInspector] public Vector2 input;

    #region Components               
    [HideInInspector] public Animator _animator;     
    [HideInInspector] public Rigidbody _rigidbody; 
    [HideInInspector] public PhysicMaterial maxFrictionPhysics, frictionPhysics, slippyPhysics; 
    [HideInInspector] public CapsuleCollider _capsuleCollider;
    #endregion

    // Use this for initialization
    public void Init()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        // slides the character through walls and edges
        frictionPhysics = new PhysicMaterial();
        frictionPhysics.name = "frictionPhysics";
        frictionPhysics.staticFriction = .25f;
        frictionPhysics.dynamicFriction = .25f;
        frictionPhysics.frictionCombine = PhysicMaterialCombine.Multiply;

        // prevents the collider from slipping on ramps
        maxFrictionPhysics = new PhysicMaterial();
        maxFrictionPhysics.name = "maxFrictionPhysics";
        maxFrictionPhysics.staticFriction = 1f;
        maxFrictionPhysics.dynamicFriction = 1f;
        maxFrictionPhysics.frictionCombine = PhysicMaterialCombine.Maximum;

        // air physics 
        slippyPhysics = new PhysicMaterial();
        slippyPhysics.name = "slippyPhysics";
        slippyPhysics.staticFriction = 0f;
        slippyPhysics.dynamicFriction = 0f;
        slippyPhysics.frictionCombine = PhysicMaterialCombine.Minimum;
    }

    // Update is called once per frame
    public void UpdateController ()
    {
        CheckGround(); // Gravity physical force
        ControlMotion(); // Movement WASD
    }

    public void UpdateAnimator()
    {

    }

    public void CheckGround()
    {
        // Check distance to ground
        RaycastHit groundHit;
        Physics.Raycast(new Ray(transform.position, -Vector3.up), out groundHit);
        groundDistance = groundHit.distance;

        if (isGrounded && input == Vector2.zero) _capsuleCollider.material = maxFrictionPhysics;
        else if (isGrounded && input != Vector2.zero) _capsuleCollider.material = frictionPhysics;
        else _capsuleCollider.material = slippyPhysics;

        if (groundDistance >= 0.5f)
        {
            isGrounded = false;
            //verticalVelocity = _rigidbody.velocity.y;
            _rigidbody.AddForce(transform.up * gravity * Time.deltaTime, ForceMode.VelocityChange);
        }
        else
        {
            _rigidbody.AddForce(transform.up * (gravity * 2 * Time.deltaTime), ForceMode.VelocityChange);
        }
    }

    public void ControlMotion()
    {
        float speed = Mathf.Abs(input.x) + Mathf.Abs(input.y);
        speed = Mathf.Clamp(speed, 0, 1f);

        // Apply body rotation first
        Vector3 lookDirection = (input.x * Camera.main.transform.right + input.y * Camera.main.transform.forward).normalized;
        Quaternion freeRotation = Quaternion.LookRotation(lookDirection, transform.up);
        float diferenceRotation = freeRotation.eulerAngles.y - transform.eulerAngles.y;
        float eulerY = transform.eulerAngles.y;

        eulerY = freeRotation.eulerAngles.y;
        var euler = new Vector3(transform.eulerAngles.x, eulerY, transform.eulerAngles.z);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(euler), freeRotationSpeed * Time.deltaTime);

        _rigidbody.AddForce(transform.forward * (walkSpeed * speed) * Time.deltaTime, ForceMode.VelocityChange);
    }
}

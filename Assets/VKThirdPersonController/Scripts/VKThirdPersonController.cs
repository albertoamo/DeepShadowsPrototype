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
    }

    // Update is called once per frame
    public void UpdateController ()
    {
        CheckGround();
        ControlMotion();
    }

    public void UpdateAnimator()
    {

    }

    public void CheckGround()
    {
        // change the physics material to very slip when not grounded or maxFriction when is
        if (isGrounded && input == Vector2.zero)
            _capsuleCollider.material = maxFrictionPhysics;
        else if (isGrounded && input != Vector2.zero)
            _capsuleCollider.material = frictionPhysics;
        else
            _capsuleCollider.material = slippyPhysics;

        // Apply gravity

    }

    public void ControlMotion()
    {

    }
}

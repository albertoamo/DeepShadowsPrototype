using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public GameObject staminaBar;
    public Image staminaValue;

    [Header("--- Offset values ---")]
    protected float gravity = 9.8f;
    protected float slopeLimit = 45f;
    protected float groundDistance;
    protected float jumpHeight = 4f;
    protected const float maxAirDistance = 0.2f;
    protected const float maxAirDropOutDistance = 0.6f;
    protected static Vector3 nGravity = new Vector3(0, -9.8f, 0);

    protected Vector3 previousPosition;

    [Header("--- Stamina values ---")]
    public float stamina = 100f;
    public float maxStamina = 100f;
    public float minStamina = 0f;
    public float dcrStaminaOnPlaceMultiplier = 0.5f;
    public float dcrStaminaGround = 0.02f;
    public float dcrStaminaWall = 0.08f;

    [Header("--- Movement values ---")]
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public float inputSpeed;
    [HideInInspector] public float playerSpeed;
    public const float walkSpeed = 9f;
    public const float runSpeed = 10f;
    public const float crouchSpeed = 7f;
    public const float rotationSpeed = 10f;
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
        staminaValue = staminaBar.GetComponent<Image>();
        staminaBar.transform.parent.gameObject.SetActive(false);

        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        Physics.gravity = nGravity;
    }

    // Update is called once per frame
    public void UpdateController()
    {
        CheckGround(); // Gravity physical force
        ControlShadow(); // Shadow first or it will force player down!
        ControlMotion(); // Movement WASD
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
        Physics.Raycast(new Ray(transform.position + transform.up * 0.1f, -transform.up), out groundHit);
        groundDistance = groundHit.distance;

        // Detect wether we are on air or grounded
        if (groundDistance >= maxAirDistance)
        {
            isGrounded = false;
        }
        else
        {
            isGrounded = true;
        }
    }

    // Function to control player motion, WASD
    public void ControlMotion()
    {
        if (isGrounded == false) return;

        // Obtain input strength
        inputSpeed = Mathf.Abs(input.x) + Mathf.Abs(input.y);
        inputSpeed = Mathf.Clamp(inputSpeed, 0, 1f);
        Vector3 lookDir = Vector3.ProjectOnPlane(lookDirection, Physics.gravity);

        if (input != Vector2.zero && lookDir != Vector3.zero)
        {
            // Apply body rotation depending on looking direction
            Quaternion euler = Quaternion.LookRotation(lookDir, transform.up);
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
        if (groundDistance > maxAirDropOutDistance)
        {
            ExitShadowMode();
            return;
        }

        if (characterStatus == StatusType.isShadow)
        {
            RaycastHit groundHit, groundHitConvex;
            Vector3 originPoint = (transform.position + 0.01f * transform.up) + _capsuleCollider.radius * transform.forward;
            bool hit = Physics.Raycast(new Ray((transform.position + 0.01f * transform.up), transform.forward), out groundHit, 0.45f, 5);
            bool hitConvex = Physics.Raycast(new Ray(originPoint, -(transform.forward + transform.up).normalized), out groundHitConvex, 1f, 5);

            if (hit && groundHit.distance < 0.4f)
            {
                if (Vector3.Dot(-groundHit.normal.normalized, nGravity) > -0.1f)
                {
                    Vector3 myForward = Vector3.Cross(transform.right, groundHit.normal);
                    transform.rotation = Quaternion.LookRotation(myForward, groundHit.normal);
                    transform.position = transform.position + _capsuleCollider.radius * transform.forward;
                    Physics.gravity = -groundHit.normal.normalized * gravity;
                }
            }

            if (!hit && hitConvex && groundHitConvex.distance > _capsuleCollider.radius && groundHitConvex.distance < 1f)
            {
                if (Vector3.Dot(-groundHitConvex.normal.normalized, nGravity) > -0.1f)
                {
                    Vector3 myForward = Vector3.Cross(transform.right, groundHitConvex.normal);
                    transform.rotation = Quaternion.LookRotation(myForward, groundHitConvex.normal);
                    transform.position = transform.position + _capsuleCollider.radius * transform.forward; // offset forward
                    Physics.gravity = -groundHitConvex.normal.normalized * gravity;
                }
            }

            UpdateStamina();
        }
    }

    public void UpdateStamina()
    {

        /* Determine decremental depending if we are in ground mode or wall mode */
        float currentDcrStamina = (Physics.gravity == nGravity) ? dcrStaminaGround : dcrStaminaWall;

        /* Determine decremental multiplier if we are in the same position or moving */
        float currentDcrOnPlaceMultiplier = (previousPosition == transform.position) ? dcrStaminaOnPlaceMultiplier : 1f;

        stamina = Mathf.Clamp(stamina - (currentDcrOnPlaceMultiplier * currentDcrStamina + Time.deltaTime), minStamina, maxStamina);

        staminaValue.fillAmount = stamina / maxStamina;

        previousPosition = transform.position;

        if (stamina == 0)
            ExitShadowMode();
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
        _capsuleCollider.height = _capsuleCollider.radius;
        _capsuleCollider.center = new Vector3(0, _capsuleCollider.radius, 0);
        transform.GetChild(1).gameObject.SetActive(false);

        VKThirdPersonCamera charCamera = FindObjectOfType<VKThirdPersonCamera>();
        charCamera.SetTarget(target);
        charCamera.height = 0;

        Physics.IgnoreLayerCollision(8, 8, true);
        staminaBar.transform.parent.gameObject.SetActive(true);

        return true;
    }

    public bool ExitShadowMode(GameObject target = null)
    {
        if (characterStatus != StatusType.isShadow) return false;

        characterStatus = StatusType.IsWalking;
        _capsuleCollider.height = 1.9f;
        _capsuleCollider.center = new Vector3(0, 0.96f, 0);

        transform.GetChild(1).gameObject.SetActive(true);
        staminaBar.transform.parent.gameObject.SetActive(false);

        VKThirdPersonCamera charCamera = FindObjectOfType<VKThirdPersonCamera>();
        charCamera.SetTarget(this.gameObject);
        charCamera.height = 1.4f;

        Physics.IgnoreLayerCollision(8, 8, false);
        Physics.gravity = nGravity;
        transform.up = Vector3.up;

        // FULL STAMINA REGEN ON EXIT
        stamina = maxStamina;

        return true;
    }
}

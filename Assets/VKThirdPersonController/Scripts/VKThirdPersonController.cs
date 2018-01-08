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
    public Material projectorMaterial;
    public Image staminaValue;

    [Header("--- Offset values ---")]
    protected float gravity = 9.8f;
    protected float slopeLimit = 45f;
    protected float groundAngle;
    protected float groundDistance;
    protected float jumpHeight = 4f;
    protected float stepOffsetEnd = 0.45f;
    protected float stepOffsetStart = 0.05f;
    protected float stepSmooth = 4f;
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

    private VKThirdPersonCamera charCamera;
    private GameObject targetProjector;
    private RaycastHit groundHit;
    #region Components               
    [HideInInspector] public Animator _animator;
    [HideInInspector] public Rigidbody _rigidbody;
    [HideInInspector] public CapsuleCollider _capsuleCollider;
    #endregion

    // Use this for initialization
    public void Init()
    {
        Physics.gravity = nGravity;

        playerSpeed = walkSpeed;
        characterStatus = StatusType.IsWalking;
        staminaValue = staminaBar.GetComponent<Image>();
        staminaBar.transform.parent.gameObject.SetActive(false);
        charCamera = FindObjectOfType<VKThirdPersonCamera>();

        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        // Create and set the projector for the shadow.
        targetProjector = new GameObject("VKProjectorShadow");
        targetProjector.transform.position = this.transform.position + 2 * this.transform.up;
        targetProjector.transform.SetParent(this.transform);
        targetProjector.AddComponent<Projector>();
        targetProjector.transform.eulerAngles = new Vector3(90, 0, 0);
        targetProjector.GetComponent<Projector>().material = projectorMaterial;
        targetProjector.GetComponent<Projector>().fieldOfView = 30;
        targetProjector.GetComponent<Projector>().nearClipPlane = 1.9f;
        targetProjector.GetComponent<Projector>().farClipPlane = 3f;
        targetProjector.SetActive(false);
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
        Physics.Raycast(new Ray(transform.position + transform.up * 0.1f, -transform.up), out groundHit);
        groundAngle = Vector3.Angle(groundHit.normal, Vector3.up);
        groundDistance = groundHit.distance;

        // Detect wether we are on air or grounded
        isGrounded = (groundDistance >= maxAirDistance) ? false : true;
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
        //ControlAngleMotion();
    }

    // Function to control motion on steps and sliding zones.
    public void ControlAngleMotion()
    {
        if (characterStatus == StatusType.isShadow) return;

        RaycastHit stepHit;
        Ray rayStep = new Ray((transform.position + new Vector3(0, stepOffsetEnd, 0) + transform.forward * ((_capsuleCollider).radius + 0.05f)), Vector3.down);

        if (Physics.Raycast(rayStep, out stepHit, stepOffsetEnd - stepOffsetStart) && !stepHit.collider.isTrigger)
        {
            if (stepHit.point.y >= (transform.position.y) && stepHit.point.y <= (transform.position.y + stepOffsetEnd))
            {
                var velocityDirection = (stepHit.point - transform.position).normalized;
                _rigidbody.velocity = velocityDirection * stepSmooth * 2;
            }
        }
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
            RaycastHit directHit, groundHitConvex;
            Vector3 originPoint = (transform.position + 0.1f * transform.up) + _capsuleCollider.radius * transform.forward;
            bool hit = Physics.Raycast(new Ray((transform.position + 0.05f * transform.up), transform.forward), out directHit, 0.45f, 5);
            bool hitConvex = Physics.Raycast(new Ray(originPoint, -(transform.forward + transform.up).normalized), out groundHitConvex, 1f, 5);

            Vector3 endpoint = originPoint + -2f * (transform.forward + transform.up).normalized;
            Debug.DrawLine(originPoint, endpoint);

            if (hit && directHit.distance < 0.4f)
            {
                if (Vector3.Dot(-directHit.normal.normalized, nGravity) > -0.1f)
                {
                    Vector3 myForward = Vector3.Cross(transform.right, directHit.normal);
                    transform.rotation = Quaternion.LookRotation(myForward, directHit.normal);
                    transform.position = transform.position + _capsuleCollider.radius * transform.forward;
                    Physics.gravity = -directHit.normal.normalized * gravity;
                    charCamera.timer.Reset();
                }
            }

            if (!hit && hitConvex && groundHitConvex.distance > .175f && groundHitConvex.distance < 2f)
            {
                if (groundHitConvex.normal != groundHit.normal && Vector3.Dot(-groundHitConvex.normal.normalized, nGravity) > -0.1f)
                {
                    Vector3 myForward = Vector3.Cross(transform.right, groundHitConvex.normal);
                    transform.rotation = Quaternion.LookRotation(myForward, groundHitConvex.normal);
                    transform.position = groundHitConvex.point + _capsuleCollider.radius * transform.forward; // offset forward
                    //transform.position += _capsuleCollider.radius * transform.up; // offset up
                    Physics.gravity = -groundHitConvex.normal.normalized * gravity;
                    charCamera.timer.Reset();
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

        // Adding a little fix to improve rotation, fix this in future patches

        characterStatus = StatusType.isShadow;
        _capsuleCollider.height = _capsuleCollider.radius;
        _capsuleCollider.center = new Vector3(0, _capsuleCollider.radius, 0);
        transform.forward = Vector3.Cross(-charCamera.transform.right, Physics.gravity).normalized;

        targetProjector.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(false);
        staminaBar.transform.parent.gameObject.SetActive(true);

        charCamera = FindObjectOfType<VKThirdPersonCamera>();
        charCamera.SetTarget(target, 1f);
        charCamera.height = .5f;

        Physics.IgnoreLayerCollision(8, 8, true);

        return true;
    }

    public bool ExitShadowMode(GameObject target = null)
    {
        if (characterStatus != StatusType.isShadow) return false;

        // Offset the player out of the wall, just in case
        transform.position += .5f * groundHit.normal; 

        characterStatus = StatusType.IsWalking;
        _capsuleCollider.height = 1.9f;
        _capsuleCollider.center = new Vector3(0, 0.96f, 0);

        targetProjector.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(true);
        staminaBar.transform.parent.gameObject.SetActive(false);

        charCamera = FindObjectOfType<VKThirdPersonCamera>();
        charCamera.SetTarget(this.gameObject, 1f);
        charCamera.height = 1.4f;

        transform.up = Vector3.up;
        transform.forward = Vector3.Cross(charCamera.transform.right, Vector3.up).normalized;
        Physics.IgnoreLayerCollision(8, 8, false);
        Physics.gravity = nGravity;

        _rigidbody.velocity = new Vector3(0,0,0);

        // FULL STAMINA REGEN ON EXIT
        stamina = maxStamina;
        return true;
    }
}

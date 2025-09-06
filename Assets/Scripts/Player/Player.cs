using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    public float moveSpeed = 5f;

    [Header("Roll Settings")]
    public float rollForce = 7f;         // ������ �̵� �ӵ�
    public float rollDuration = 0.6f;    // ������ ���� �ð�
    public float rollCooldown = 0.5f;    // ������ ��Ÿ��
    private bool isRolling = false;
    private float lastRollTime = -10f;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public Vector3 cameraOffset = new Vector3(0, 2, -5);
    public float mouseSensitivity = 150f;
    public float smoothTime = 0.05f;

    [Header("Animation Settings")]
    public Animator animator;

    private Rigidbody rb;
    private float xRotation = 0f;
    private Vector3 currentVelocity;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("Animator ������Ʈ�� �����ϴ�. T-Pose ������Ʈ�� Animator�� �߰��ϼ���.");

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        MovePlayer();
        RotatePlayer();
        UpdateCamera();
        UpdateAnimator();
    }

    public void MovePlayer()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // �̵�
        Vector3 move = transform.right * h + transform.forward * v;
        Vector3 velocity = move * moveSpeed;
        transform.position += velocity * Time.deltaTime;

        // ������ ����
        if (Input.GetButtonDown("Jump") && !isRolling && Time.time > lastRollTime + rollCooldown) // Jump ��ư�� Roll�� ��Ȱ��
        {
            StartCoroutine(Roll());
        }
    }

    private IEnumerator Roll()
    {
        isRolling = true;
        lastRollTime = Time.time;

        if (animator != null)
            animator.SetTrigger("Roll");

        Vector3 rollDirection = transform.forward;

        float elapsed = 0f;
        while (elapsed < rollDuration)
        {
            rb.MovePosition(transform.position + rollDirection * rollForce * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        isRolling = false;
    }

    void RotatePlayer()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -30f, 60f);
    }

    void UpdateCamera()
    {
        Vector3 targetPos = transform.position + Quaternion.Euler(xRotation, transform.eulerAngles.y, 0) * cameraOffset;
        cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, targetPos, ref currentVelocity, smoothTime);
        cameraTransform.LookAt(transform.position + Vector3.up * 1.5f);
    }

    void UpdateAnimator()
    {
        if (animator != null)
        {
            float speed = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude;
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsGrounded", isGrounded && !isRolling);
        }
    }
}

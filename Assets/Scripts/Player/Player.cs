using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    public float moveSpeed = 5f;
    public float jumpHeight = 2f; // ���� ����
    public float jumpDuration = 0.5f; // ���� �ð� (�պ�)

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public Vector3 cameraOffset = new Vector3(0, 2, -5);
    public float mouseSensitivity = 150f;
    public float smoothTime = 0.05f;

    [Header("Animation Settings")]
    public Animator animator;

    [Header("Debug Settings")]
    public LayerMask groundLayer; // ���� ���̾� (Plane)

    private float jumpTimer = 0f;
    private bool isJumping = false;
    private Vector3 jumpStartPosition;
    private Rigidbody rb;
    private float xRotation = 0f;
    private Vector3 currentVelocity;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.isKinematic = true;

        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("Animator ������Ʈ�� �����ϴ�. T-Pose ������Ʈ�� Animator�� �߰��ϼ���.");

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (groundLayer.value == 0)
            groundLayer = LayerMask.GetMask("Default"); // Plane�� �⺻������ Default ���̾�

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

    void MovePlayer()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;
        Vector3 velocity = move * moveSpeed;
        transform.position += velocity * Time.deltaTime;

        // ���� ó��
        isGrounded = IsGrounded();
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            Debug.Log("Jump �Է� ����, IsGrounded: " + isGrounded);
            isJumping = true;
            jumpStartPosition = transform.position;
            jumpTimer = 0f;
            animator.SetTrigger("Jump");
            Debug.Log("Jump Ʈ���� ȣ��");
        }

        // �񹰸� ���� ����
        if (isJumping)
        {
            jumpTimer += Time.deltaTime;
            float t = jumpTimer / jumpDuration;
            if (t <= 1f)
            {
                float height = jumpHeight * (1f - Mathf.Pow(2f * t - 1f, 2f));
                transform.position = new Vector3(
                    transform.position.x,
                    jumpStartPosition.y + height,
                    transform.position.z
                );
            }
            else
            {
                isJumping = false;
                transform.position = new Vector3(
                    transform.position.x,
                    jumpStartPosition.y,
                    transform.position.z
                );
                animator.SetBool("IsGrounded", true);
                Debug.Log("Jump ����, ����");
            }
        }
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
            Debug.Log($"Speed: {speed}, IsGrounded: {isGrounded}, IsJumping: {isJumping}");
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsGrounded", isGrounded && !isJumping);
        }
    }

    bool IsGrounded()
    {
        Vector3 rayStart = transform.position - new Vector3(0, 0.9f, 0); // Capsule Collider �ϴ�
        bool grounded = Physics.Raycast(rayStart, Vector3.down, 0.6f, groundLayer);
        Debug.DrawRay(rayStart, Vector3.down * 0.6f, grounded ? Color.green : Color.red, 0.1f);
        Debug.Log($"IsGrounded: {grounded}, RayStart: {rayStart}");
        return grounded;
    }
}
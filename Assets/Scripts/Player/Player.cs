using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public Vector3 cameraOffset = new Vector3(0, 2, -5);
    public float mouseSensitivity = 150f;
    public float smoothTime = 0.05f;

    [Header("Animation Settings")]
    public Animator animator; // Animator ������Ʈ

    private Rigidbody rb;
    private float xRotation = 0f;
    private Vector3 currentVelocity;
    private bool isGrounded;

    void Start()
    {
        // Rigidbody ����
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.isKinematic = true; // Kinematic���� �����Ͽ� �񺼷� MeshCollider ���� ����

        // Animator ����
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("Animator ������Ʈ�� �����ϴ�. T-Pose ������Ʈ�� Animator�� �߰��ϼ���.");

        // ī�޶� ����
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

    void MovePlayer()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // �̵� ���� ���
        Vector3 move = transform.right * h + transform.forward * v;
        Vector3 velocity = move * moveSpeed;

        // Transform���� �̵� (Kinematic�̹Ƿ� ���� ��ġ ������Ʈ)
        transform.position += velocity * Time.deltaTime;

        // ���� ó��
        isGrounded = IsGrounded();
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.isKinematic = false; // ���� �� ���� Ȱ��ȭ
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetTrigger("Jump"); // ���� �ִϸ��̼� Ʈ����
            StartCoroutine(ResetKinematicAfterJump());
        }
    }

    void RotatePlayer()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX); // �¿� ȸ��
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -30f, 60f); // ���� ����
    }

    void UpdateCamera()
    {
        // ��ǥ ��ġ: �÷��̾� �� + offset
        Vector3 targetPos = transform.position + Quaternion.Euler(xRotation, transform.eulerAngles.y, 0) * cameraOffset;

        // ������ �̵�
        cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, targetPos, ref currentVelocity, smoothTime);

        // �׻� �÷��̾ ���� ���� �ٶ󺸱�
        cameraTransform.LookAt(transform.position + Vector3.up * 1.5f);
    }

    void UpdateAnimator()
    {
        if (animator != null)
        {
            // �̵� �ӵ� ���
            float speed = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude;
            Debug.Log("Speed: " + speed); // ����� �α�

            // Speed�� 0���� 0 �̻����� �ٲ�� ��� Walk�� ��ȯ
            if (speed > 0 && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                animator.SetFloat("Speed", speed);
            }
            else
            {
                animator.SetFloat("Speed", speed);
            }

            // ���� ����
            animator.SetBool("IsGrounded", isGrounded);
        }
    }

    bool IsGrounded()
    {
        // Capsule Collider�� �ٴ� �浹 ����
        return Physics.Raycast(transform.position, Vector3.down, 1.5f); // �Ÿ� ����
    }

    IEnumerator ResetKinematicAfterJump()
    {
        // ���� �� 1�� �� Kinematic���� ����
        yield return new WaitForSeconds(1f);
        if (IsGrounded())
        {
            rb.isKinematic = true;
        }
    }
}
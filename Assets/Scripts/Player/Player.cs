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
    public Animator animator; // Animator 컴포넌트

    private Rigidbody rb;
    private float xRotation = 0f;
    private Vector3 currentVelocity;
    private bool isGrounded;

    void Start()
    {
        // Rigidbody 설정
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.isKinematic = true; // Kinematic으로 설정하여 비볼록 MeshCollider 오류 방지

        // Animator 설정
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("Animator 컴포넌트가 없습니다. T-Pose 오브젝트에 Animator를 추가하세요.");

        // 카메라 설정
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

        // 이동 방향 계산
        Vector3 move = transform.right * h + transform.forward * v;
        Vector3 velocity = move * moveSpeed;

        // Transform으로 이동 (Kinematic이므로 직접 위치 업데이트)
        transform.position += velocity * Time.deltaTime;

        // 점프 처리
        isGrounded = IsGrounded();
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.isKinematic = false; // 점프 시 물리 활성화
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetTrigger("Jump"); // 점프 애니메이션 트리거
            StartCoroutine(ResetKinematicAfterJump());
        }
    }

    void RotatePlayer()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX); // 좌우 회전
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -30f, 60f); // 상하 제한
    }

    void UpdateCamera()
    {
        // 목표 위치: 플레이어 뒤 + offset
        Vector3 targetPos = transform.position + Quaternion.Euler(xRotation, transform.eulerAngles.y, 0) * cameraOffset;

        // 스무스 이동
        cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, targetPos, ref currentVelocity, smoothTime);

        // 항상 플레이어가 보는 방향 바라보기
        cameraTransform.LookAt(transform.position + Vector3.up * 1.5f);
    }

    void UpdateAnimator()
    {
        if (animator != null)
        {
            // 이동 속도 계산
            float speed = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude;
            Debug.Log("Speed: " + speed); // 디버깅 로그

            // Speed가 0에서 0 이상으로 바뀌면 즉시 Walk로 전환
            if (speed > 0 && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                animator.SetFloat("Speed", speed);
            }
            else
            {
                animator.SetFloat("Speed", speed);
            }

            // 착지 여부
            animator.SetBool("IsGrounded", isGrounded);
        }
    }

    bool IsGrounded()
    {
        // Capsule Collider의 바닥 충돌 감지
        return Physics.Raycast(transform.position, Vector3.down, 1.5f); // 거리 조정
    }

    IEnumerator ResetKinematicAfterJump()
    {
        // 점프 후 1초 뒤 Kinematic으로 복귀
        yield return new WaitForSeconds(1f);
        if (IsGrounded())
        {
            rb.isKinematic = true;
        }
    }
}
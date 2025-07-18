using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 0.1f;
    [SerializeField] private float mouseSensitivity = 2f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayerMask = 1;
    
    [Header("Block Placement")]
    [SerializeField] private float placeRange = 5f;
    [SerializeField] private int blockTypeToPlace = 3; // 設置するブロックタイプ（木）
    
    private Camera playerCamera;
    private float xRotation = 0f;
    public bool isGrounded;
    
    // World参照
    private World world;
    
    // Input Actions
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction escapeAction;
    private InputAction placeAction;
    private InputAction breakAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // カメラを取得（子オブジェクトから探す）
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // カーソルをロック
        Cursor.lockState = CursorLockMode.Locked;

        // Rigidbodyの設定
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        // Ground Check Pointが設定されていない場合は作成
        if (groundCheckPoint == null)
        {
            GameObject groundCheck = new GameObject("GroundCheckPoint");
            groundCheck.transform.SetParent(transform);
            groundCheck.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheckPoint = groundCheck.transform;
        }
        
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");
        escapeAction = InputSystem.actions.FindAction("Escape");
        placeAction = InputSystem.actions.FindAction("Place");
        breakAction = InputSystem.actions.FindAction("Break");

        // ESCキーのコールバック設定
        escapeAction.performed += OnEscapePressed;
        
        // Placeキーのコールバック設定
        placeAction.performed += OnPlacePressed;
        
        // World参照を取得
        world = FindFirstObjectByType<World>();
    }


    // Update is called once per frame
    void Update()
    {
        HandleMouseLook();
    }
    
    private void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    
    private async void OnPlacePressed(InputAction.CallbackContext context)
    {
        if (world == null || playerCamera == null) return;
        
        // カメラからRaycastを飛ばす
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, placeRange, groundLayerMask))
        {
            // 着弾点の表面にブロックを設置（法線方向に少しずらす）
            Vector3 placePosition = hit.point + hit.normal * 0.5f;
            await world.SetBlockWorld(placePosition, blockTypeToPlace);
        }
    }
    
    void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
        CheckGrounded();
    }
    
    private void HandleMovement()
    {
        // Input Actionから移動入力を取得
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        
        // 移動方向を計算（プレイヤーの向きに基づく）
        Vector3 direction = transform.right * moveInput.x + transform.forward * moveInput.y;
        direction = direction.normalized;
        
        // Y軸の速度は保持して、XZ平面での移動のみ適用
        Vector3 moveVelocity = direction * moveSpeed;
        moveVelocity.y = rb.linearVelocity.y;
        
        // Rigidbodyに速度を適用
        rb.linearVelocity = moveVelocity;
    }
    
    private void HandleJump()
    {
        // Input Actionでジャンプ（地面にいる時のみ）
        if (jumpAction.IsPressed() && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    
    private void HandleMouseLook()
    {
        // カーソルがロックされている時のみマウス操作を受け付ける
        if (Cursor.lockState != CursorLockMode.Locked) return;
        
        // Input Actionからマウス入力を取得
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        
        // 感度とフレームレートに応じた調整
        lookInput *= mouseSensitivity * Time.deltaTime;
        
        // Y軸回転（左右を向く）
        transform.Rotate(Vector3.up * lookInput.x);
        
        // X軸回転（上下を見る）- カメラに適用
        xRotation -= lookInput.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }
    
    private void CheckGrounded()
    {
        // 地面チェック（spherecast）
        isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayerMask);
    }
    
    // デバッグ用：地面チェックポイントの可視化
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;


public class PhysicsMovement : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty moveAction;
    public InputActionProperty turnAction;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float turnSpeed = 60; 
  
    public float inputDeadzone = 0.15f;
    private Vector2 inputMoveAxis;
    private float inputTurnAxis;

    [Header("References")]
    public Transform directionSource;
    public LayerMask groundLayer;
    public CapsuleCollider bodyCollider;
    public Rigidbody rb;
    public Transform turnSource;


    void Start()
    {
       
        rb.useGravity = true;
     
        

        moveAction.action.Enable();
        turnAction.action.Enable();
        Debug.Log($"Turn Action Enabled? {turnAction.action.enabled}");
    }

    public void Update()
    {
        inputMoveAxis = moveAction.action.ReadValue<Vector2>();
        Debug.Log($"move Axis: {inputMoveAxis}");
        Vector2 turnInput = turnAction.action.ReadValue<Vector2>();
        inputTurnAxis = turnInput.x;
        Debug.Log($"[DEBUG] Full Turn Vector2: {turnInput}, X: {turnInput.x}");
    }
    void FixedUpdate()
    { 
       

        bool isGrounded = checkIfGrounded();
        
        if(isGrounded) 
        {
            Quaternion yaw = Quaternion.Euler(0, directionSource.eulerAngles.y, 0);
            Vector3 direction = yaw * new Vector3(inputMoveAxis.x, 0, inputMoveAxis.y);

            
            Vector3 targetMovePosition = rb.position + direction * Time.fixedDeltaTime * moveSpeed;

            Vector3 axis = Vector3.up;
            float angle = turnSpeed * Time.fixedDeltaTime * inputTurnAxis;

            Quaternion q =Quaternion.AngleAxis(angle, axis); 

            rb.MoveRotation(rb.rotation * q);

            Vector3 newPosition = q * (targetMovePosition - turnSource.position) + turnSource.position;

            rb.MovePosition(newPosition);
        }

      
    }

    public bool checkIfGrounded()
    {
        Vector3 start = bodyCollider.transform.TransformPoint(bodyCollider.center);
        float rayLength = bodyCollider.height / 2 - bodyCollider.radius + 0.05f;


        bool hasHit = Physics.SphereCast(start, bodyCollider.radius, Vector3.down, out RaycastHit hitinfo, rayLength, groundLayer);
         
        return hasHit;
    }
}

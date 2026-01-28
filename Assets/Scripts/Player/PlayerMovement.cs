using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private CharacterController controller;
    
    private float _moveInput;
    private float _turnInput;
    
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float jumpForce = 1.0f;
    [SerializeField] private float gravity = 9.81f;
    
    private float _verticalVelocity;

    private bool disableMovement = false;
    
    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (disableMovement) return;
        
        InputManagement();
        Movement();
    }

    public void SetDisableMovement(bool value)
    {
        if (!IsOwner) return;
    
        disableMovement = value;
        if (disableMovement)
        {
            Debug.Log("Disabled movement for: " + gameObject.name);
            controller.enabled = false;
        }
        else
        {
            controller.enabled = true;
        }
    }
    
    private void Movement()
    {
        GroundMovement();
    }

    private float CalcuateVerticalVelocity()
    {
        if (controller.isGrounded)
        {
            _verticalVelocity = -1f;
            
            if (Input.GetKey(KeyCode.Space))
            {
                _verticalVelocity = Mathf.Sqrt(jumpForce * 2f * gravity);
            }
        }
        else
        {
            _verticalVelocity -= gravity * Time.deltaTime;
        }
        return _verticalVelocity;
    }
    
    private void GroundMovement()
    {
        Vector3 move = new Vector3(_turnInput, 0.0f, _moveInput);
        move *= speed;
        move.y = CalcuateVerticalVelocity();
        controller.Move(move * Time.deltaTime);
    }

    private void InputManagement()
    {
        _moveInput = Input.GetAxis("Vertical");
        _turnInput = Input.GetAxis("Horizontal");
    }
}

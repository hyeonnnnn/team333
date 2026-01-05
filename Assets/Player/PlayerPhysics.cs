using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerPhysics : MonoBehaviour
{
    private Rigidbody _rigidbody;
    
    [SerializeField] private float _gravity = 9.81f;
    [SerializeField] private float _jumpForce = 20f;
    
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
            Jump();
    }

    private void Jump()
    {
        _rigidbody.linearVelocity = Vector3.up * _jumpForce;
    }
    
    private void FixedUpdate()
    {
        Gravity();
    }

    public void Gravity()
    {
        _rigidbody.linearVelocity -= Vector3.up * _gravity * Time.deltaTime;
    }
}

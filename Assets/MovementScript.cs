using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;

public class MovementScript : MonoBehaviour
{
    private Rigidbody2D _rb;

    [Header("Layer Masks")]
    [SerializeField]private LayerMask _groundLayer;


    [Header("Movement Variables")]
    [SerializeField] private float _movementAcceleration;
    [SerializeField] private float _maxMoveSpeed;
    [SerializeField] private float _linearDrag;
    private float _horizontalDirection;
    private bool _changingDirection => (_rb.velocity.x > 0f && _horizontalDirection < 0f) || (_rb.velocity.x < 0f && _horizontalDirection > 0f);


    [Header("Jump Variables")]
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private float _airLinearDrag = 10f;
    [SerializeField] private float _fallMultiplier = 8f;
    [SerializeField] private float _lowJumpFallMultiplier = 5f;
    [SerializeField] private int _extraJumps = 1;
    private int _extraJumpsValue;
    private bool _canJump => Input.GetButtonDown("Jump") && _onGround && (_onGround || _extraJumpsValue > 0);

    [Header("Ground Collision Variables")]
    [SerializeField] private float _groundRaycastLength;
    private bool _onGround;
    [SerializeField] private Vector3 _groundRaycastOffset;

    [Header("Shooting Knockback Variables")]
    [SerializeField] private float _knockbackForce;
    [SerializeField] private int _bullet;
    
    private bool _canKnockback => (_bullet == 1 && Input.GetMouseButtonDown(0));
    private bool _isKnockback = false;
    [SerializeField] private float _knockbackTime = 0.5f;




    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private Vector2 GetInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private Vector2 AngleOfKnockback(Vector3 knockbackVector)
    {
        float angle = Mathf.Atan2(knockbackVector.y, knockbackVector.x);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    private void ShootGun()
    {
        Debug.Log(_bullet);
        _bullet--;
        if (_bullet <= 0) {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            _rb.AddForce(-AngleOfKnockback(mousePosition) * _knockbackForce, ForceMode2D.Impulse);
            StartCoroutine(KnockbackStunTime(_knockbackTime));
        }
        
        
    }

    private void MoveCharacter()
    {
        _rb.AddForce(new Vector2(_horizontalDirection, 0f) * _movementAcceleration);

        if (Mathf.Abs(_rb.velocity.x) > _maxMoveSpeed)
        {
            _rb.velocity = new Vector2(Mathf.Sign(_rb.velocity.x) * _maxMoveSpeed, _rb.velocity.y);
        }
    }

    private void ApplyLinearDrag()
    {
        if (Mathf.Abs(_horizontalDirection) < 0.4f || _changingDirection)
        {
            _rb.drag = _linearDrag;
        }
        else
        {
            _rb.drag = 0f;
        }
    }

    private void Jump()
    {
        if (!_onGround)
        {
            _extraJumpsValue--;
        }
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);

         
    }

    private void CheckCollisions()
    {
        _onGround = Physics2D.Raycast(transform.position + _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer) ||
                               Physics2D.Raycast(transform.position - _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + _groundRaycastOffset, transform.position + _groundRaycastOffset + Vector3.down * _groundRaycastLength);
        Gizmos.DrawLine(transform.position - _groundRaycastOffset, transform.position - _groundRaycastOffset + Vector3.down * _groundRaycastLength);
        Gizmos.DrawLine(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }

    private void ApplyAirLinearDrag()
    {
        _rb.drag = _airLinearDrag;
    }
    private void FallMultiplier()
    {
        if (_rb.velocity.y < 0)
        {
            _rb.gravityScale = _fallMultiplier;
        }
        else if (_rb.velocity.y > 0 && Input.GetButtonDown("Jump"))
        {
            _rb.gravityScale = _lowJumpFallMultiplier;
        }
        else
        {
            _rb.gravityScale = 1f;
        }
    }
    IEnumerator KnockbackStunTime(float cooldown)
    {
        _isKnockback = true;
        
        yield return new WaitForSeconds(cooldown);
        _isKnockback = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        _horizontalDirection = GetInput().x;
        if (_canJump) Jump();
        if (_canKnockback) ShootGun();
        if (Input.GetMouseButton(1)) _bullet = 1;
        Debug.Log(_isKnockback);

    }

    private void FixedUpdate()
    {
        CheckCollisions();
        if (!_isKnockback) MoveCharacter();
        if (_onGround)
        {
            _extraJumpsValue = _extraJumps;
            ApplyLinearDrag();
        }
        else
        {
            ApplyAirLinearDrag();
        }
       
        FallMultiplier();
    }


}

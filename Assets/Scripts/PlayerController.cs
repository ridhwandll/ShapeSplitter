using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 35;
    public float counterMovement = 13;
    public float maxAimLineDistance = 5.0f;

    private int _health;
    private int _maxHealth = 100;
    private Vector2 _input;
    private Rigidbody2D _rigidBody2D;
    private LineRenderer _lineRenderer;
    private Camera _mainCamera;
    
    void Awake()
    {
        _mainCamera = Camera.main;
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        _health = _maxHealth;
    }
    
    void Update()
    {
        _input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        DrawAimLine();
    }

    private void FixedUpdate()
    {
        float velocity =  _rigidBody2D.linearVelocity.magnitude;
        float maxSpeed =  moveSpeed * 1.5f;

        if (_input.x > 0 && _rigidBody2D.linearVelocity.x > maxSpeed) _input.x = 0;
        if (_input.x > 0 && _rigidBody2D.linearVelocity.x < -maxSpeed)  _input.x = 0;
        if (_input.y > 0 && _rigidBody2D.linearVelocity.y > maxSpeed)  _input.y = 0;
        if (_input.y > 0 && _rigidBody2D.linearVelocity.y < -maxSpeed) _input.y = 0;

        float xBonus = 1.0f;
        float yBonus = 1.0f;
        
        if (_input.x < 0 && _rigidBody2D.linearVelocity.x > 0) xBonus = 2.0f;
        if (_input.x > 0 && _rigidBody2D.linearVelocity.x < 0)  xBonus = 2.0f;
        if (_input.y > 0 && _rigidBody2D.linearVelocity.y < 0)  yBonus = 2.0f;
        if (_input.y > 0 && _rigidBody2D.linearVelocity.y < 0) yBonus = 2.0f;
        
        float extraForce = maxSpeed - velocity;
        
        _rigidBody2D.AddForce(_input * moveSpeed * extraForce * xBonus * yBonus * 2 * Time.fixedDeltaTime);
        _rigidBody2D.AddForce(counterMovement * -_rigidBody2D.linearVelocity * moveSpeed * Time.fixedDeltaTime);
    }

    private void DrawAimLine()
    {
        Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 start = transform.position;
        Vector2 end = (mouseWorld - transform.position);
 
        if (end.magnitude > maxAimLineDistance)
            end = end.normalized * maxAimLineDistance;
        
        _lineRenderer.SetPosition(0, start);
        _lineRenderer.SetPosition(1, start + end);
    }

    public int GetHealth() 
    {
        return _health;
    }

    public void TakeDamage(int amount)
    {
        _health = Mathf.Max(0, _health - amount);
        // if (_health == 0) //TODO: LEVEL END KILL HERE
    }
    
    public void Heal(int amount)
    {
        _health = Mathf.Min(_maxHealth, _health + amount);
    }
}

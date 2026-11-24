using UnityEngine;

public class PaddleLogic : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed = 7f;
    [SerializeField] public float paddleHitReward =2f;

    private float moveInput = 0f;
    private bool aiControlled = false;

    void Start()
    {
        rb.position = new Vector2(0, -2.5f);
    }

    void Update()
    {
        if (!aiControlled)
        {
            moveInput = Input.GetAxisRaw("Horizontal");
        }
    }

    void FixedUpdate()
    {
        Vector2 newPosition = rb.position +
            Vector2.right * moveInput * speed * Time.fixedDeltaTime;

        rb.MovePosition(newPosition);
    }

    public void MoveLeft()
    {
        aiControlled = true;
        moveInput = -1f;
    }

    public void MoveRight()
    {
        aiControlled = true;
        moveInput = 1f;
    }

    public void Stay()
    {
        aiControlled = true;
        moveInput = 0f;
    }

    public void EnablePlayerControl()
    {
        aiControlled = false;
    }
    public void ResetPaddle()
    {
    rb.position = new Vector2(0f, -2.5f);
    rb.linearVelocity = Vector2.zero;
    Stay();
    }   
    private void OnCollisionEnter2D(Collision2D collision)
    {
    if (!collision.gameObject.CompareTag("Ball"))
        return;

    QLearningAgent agent = FindAnyObjectByType<QLearningAgent>();

    if (agent != null)
        agent.GiveReward(paddleHitReward);
    }
}
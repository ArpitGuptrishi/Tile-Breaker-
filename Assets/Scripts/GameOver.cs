using UnityEngine;

public class GameOver : MonoBehaviour

{
    public ScoreKeeper sk;
    public BallLogic ball;
    
    void OnTriggerEnter2D(Collider2D collision)
    {
    if (!collision.CompareTag("Ball"))
        return;

    QLearningAgent agent = FindAnyObjectByType<QLearningAgent>();

    if (agent != null)
    {
        agent.EndEpisode(-10f);
    }

    sk.decScore();
    ball.BallSpawn();

    PaddleLogic paddle = FindAnyObjectByType<PaddleLogic>();

    if (paddle != null)
    {
        paddle.EnablePlayerControl();
        paddle.ResetPaddle();
    }
    }
}

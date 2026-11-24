using UnityEngine;

public class BrickLogic : MonoBehaviour
{
    private ScoreKeeper sk;

    void Start()
    {
        sk = FindAnyObjectByType<ScoreKeeper>();
    }

    void OnCollisionEnter2D(Collision2D collision)
{
    sk.addScore();

    QLearningAgent agent = FindAnyObjectByType<QLearningAgent>();

    if (agent != null)
    {
        agent.GiveReward(5f);
    }

    Destroy(gameObject);
    sk.decBrickCount();
}
}
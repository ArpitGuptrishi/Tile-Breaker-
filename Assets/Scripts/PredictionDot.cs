using UnityEngine;

public class PredictionDot : MonoBehaviour
{
    public QLearningAgent agent;
    public float predictionHeight = -2.5f;

    void Update()
    {
        if (agent == null)
            return;

        transform.position = new Vector3(
            agent.predictedLandingX,
            predictionHeight,
            0f
        );
    }
}
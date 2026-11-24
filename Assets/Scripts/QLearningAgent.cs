using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class QLearningAgent : MonoBehaviour
{
    private string SavePath => Path.Combine(Application.persistentDataPath, "qtable_autonomous.json");
    [Serializable] public class QEntry
    {
    public string state;
    public float[] values;
    }

[Serializable]
public class QTableData
{
    public QEntry[] entries;
    public  float savedEpsilon =1f;
}
    [Header("Game References")]
    public BallLogic ball;
    public PaddleLogic paddle;

    [Header("State Information")]
    public int ballX;
    public int ballY;
    public int ballDirectionX;
    public int ballDirectionY;
    public int predictedLandingX;
    public int paddleX;
    public int landingDifference;
    public int relativeXBin;
    public int heightBin;
    public int velocityXBin;
    public int velocityYBin;
    

    [Header("Q-Learning Parameters")]
    public float learningRate = 0.1f;
    public float discountFactor = 0.95f;
    public float epsilon = 1f;
    

    // Each state stores 3 Q-values: Left, Stay, Right
    private const int X_BINS = 13;
    private const int Y_BINS = 5;
    private const int VX_BINS = 3;
    private const int VY_BINS = 2;
    private const int NUM_ACTIONS = 3;
    private const float MAX_RELATIVE_X = 8f;
    private const float MAX_BALL_HEIGHT = 10f;
    private Dictionary<string, float[]> qTable = new Dictionary<string, float[]>();
    private string previousState;
    private int previousAction;
    private bool hasPreviousAction = false;
    private float accumulatedReward = 0f;
    private float previousLandingDistance;
    private bool hasPreviousLandingDistance = false;
    private float stableLandingX;
    private Vector2 previousBallDirection;
    private bool hasPreviousBallDirection = false;
    [SerializeField] private float decisionInterval = 0.05f;

    private float decisionTimer = 0f;
    [Header("Exploration Settings")]
    [SerializeField] private float minEpsilon = 0.05f;
    [SerializeField] private float epsilonDecay = 0.98f;
    
    [Header("Training Control")]
    public bool trainingEnabled = true;
    [SerializeField] private float presentationEpsilon = 0f;

    [Header("Training Statistics")]
    public int episodeCount = 0;
    public int bricksDestroyed = 0;
    public float episodeReward = 0f;   
    public int totalBricksDestroyed = 0;
    public float totalEpisodeReward = 0f;
    
    void Start()
    {
    Debug.Log(SavePath);
    LoadQTable();

    if (!trainingEnabled)
    {
        epsilon = presentationEpsilon;
    }
    }
    void Update()
    {
    decisionTimer += Time.deltaTime;

    if (decisionTimer < decisionInterval)
        return;

    decisionTimer = 0f;

    ObserveState();
    ObserveDiscreteState();
    

    

    Vector2 currentBallDirection = ball.rb.linearVelocity.normalized;

    if (hasPreviousBallDirection && Vector2.Dot(previousBallDirection, currentBallDirection) < 0.95f)
    {
    // Ball direction changed significantly.
    if (currentBallDirection.y < 0f)
    {
        stableLandingX = PredictLandingX();
        //Debug.Log(
        // $"Direction changed! Ball: {ball.transform.position}, " +
        //$"Velocity: {ball.rb.linearVelocity}, " +
        //$"Predicted landing X: {stableLandingX:F2}"
        //);
    }
    }

    previousBallDirection = currentBallDirection;
    hasPreviousBallDirection = true;
    float currentLandingDistance = Mathf.Abs(predictedLandingX - paddle.transform.position.x);
    if (hasPreviousLandingDistance){
        float progressReward =(previousLandingDistance - currentLandingDistance) * 0.1f;

        accumulatedReward += progressReward;
        episodeReward += progressReward;
    }

    previousLandingDistance = currentLandingDistance;
    hasPreviousLandingDistance = true;

    string currentState = GetStateKey();

    if (!qTable.ContainsKey(currentState))
    {
        qTable[currentState] = new float[3];
    }
    // Small penalty for each decision step
    accumulatedReward -= 0.01f;
    episodeReward -= 0.01f;

    if (hasPreviousAction)
    {
        if (trainingEnabled)
        {
            Learn(
                previousState,
                previousAction,
                accumulatedReward,
                currentState
            );
        }

    accumulatedReward = 0f;
    }

    int action = ChooseAction(currentState);

    PerformAction(action);

    previousState = currentState;
    previousAction = action;
    hasPreviousAction = true;
    }

    void ObserveState()
    {
        Vector2 ballPosition = ball.transform.position;
        Vector2 ballVelocity = ball.rb.linearVelocity;
        Vector2 paddlePosition = paddle.transform.position;

        ballX = Mathf.RoundToInt(ballPosition.x);
        ballY = Mathf.RoundToInt(ballPosition.y);
        paddleX = Mathf.RoundToInt(paddlePosition.x);

        ballDirectionX = ballVelocity.x > 0 ? 1 : -1;
        ballDirectionY = ballVelocity.y > 0 ? 1 : -1;
        if (ball.rb.linearVelocity.y < 0f)
        {
         stableLandingX = PredictLandingX();
        }
        predictedLandingX = Mathf.RoundToInt(stableLandingX);

        landingDifference = Mathf.RoundToInt(
        predictedLandingX - paddle.transform.position.x);

    }

    string GetStateKey()
    {
    return $"{relativeXBin}_{heightBin}_{velocityXBin}_{velocityYBin}";
    }
    void DebugState()
    {
    Debug.Log(
        $"State: {GetStateKey()} | " +
        $"Relative X: {relativeXBin} | " +
        $"Height: {heightBin} | " +
        $"Velocity X: {velocityXBin} | " +
        $"Velocity Y: {velocityYBin}"
    );
    }

    int ChooseAction(string state)
{
    // Exploration: choose a random action
    if (UnityEngine.Random.value < epsilon)
    {
        return UnityEngine.Random.Range(0, 3);
    }

    // Exploitation: choose the action with the highest Q-value
    float[] qValues = qTable[state];

    int bestAction = 0;

    for (int i = 1; i < qValues.Length; i++)
    {
        if (qValues[i] > qValues[bestAction])
        {
            bestAction = i;
        }
    }

    return bestAction;
}

    void PerformAction(int action)
    {
    /**float paddleX = paddle.transform.position.x;
    float distanceToTarget = predictedLandingX - paddleX;

    // Stop correcting when sufficiently close to the target.
    if (Mathf.Abs(distanceToTarget) < 0.4f)
    {
        paddle.Stay();
        return;
    }**/

    switch (action)
    {
        case 0:
            paddle.MoveLeft();
            break;

        case 1:
            paddle.Stay();
            break;

        case 2:
            paddle.MoveRight();
            break;
    }
    }

    public void GiveReward(float reward)
    {
    accumulatedReward += reward;
    episodeReward += reward;

    if (reward > 0)
    {
        bricksDestroyed++;
    }

    Debug.Log("AI received reward: " + reward);
    }

    void Learn(string state, int action, float reward, string nextState)
    {
        if (!qTable.ContainsKey(state))
            qTable[state] = new float[3];

        if (!qTable.ContainsKey(nextState))
            qTable[nextState] = new float[3];

        float oldQ = qTable[state][action];

        float maxNextQ = Mathf.Max(qTable[nextState]);

        float newQ = oldQ + learningRate *
            (reward + discountFactor * maxNextQ - oldQ);

        qTable[state][action] = newQ;
    }

    public void EndEpisode(float terminalReward)
    {
    episodeCount++;
    totalBricksDestroyed += bricksDestroyed;
    totalEpisodeReward += episodeReward;

    Debug.Log(
        $"Episode: {episodeCount} | " +
        $"Bricks destroyed: {bricksDestroyed} | " +
        $"Episode reward: {episodeReward} | " +
        $"Epsilon: {epsilon:F3}"
    );
    if (trainingEnabled && hasPreviousAction)
    {
    float oldQ = qTable[previousState][previousAction];

    qTable[previousState][previousAction] =
        oldQ + learningRate * (terminalReward - oldQ);
    }

    hasPreviousAction = false;
    accumulatedReward = 0f;
    decisionTimer = 0f;
    
    if (trainingEnabled)
    {
    DecayEpsilon();
    }

    Debug.Log("Episode ended. Terminal reward: " + terminalReward);
    
    if (trainingEnabled)
    {
    SaveQTable();
    }

    bricksDestroyed = 0;
    episodeReward = 0f;
    }
    void DecayEpsilon()
    {
    epsilon = Mathf.Max(minEpsilon, epsilon * epsilonDecay);
    }
    public void SaveQTable()
    {
    QTableData data = new QTableData();
    data.entries = new QEntry[qTable.Count];
    data.savedEpsilon = epsilon;

    int index = 0;

    foreach (var entry in qTable)
    {
        data.entries[index] = new QEntry
        {
            state = entry.Key,
            values = entry.Value
        };

        index++;
    }

    string json = JsonUtility.ToJson(data, true);
    File.WriteAllText(SavePath, json);

    Debug.Log("Q-table saved to: " + SavePath);
    }

    public void LoadQTable()

    {
    if (!File.Exists(SavePath))
    {
        Debug.Log("No saved Q-table found. Starting fresh.");
        return;
    }

    string json = File.ReadAllText(SavePath);
    QTableData data = JsonUtility.FromJson<QTableData>(json);

    qTable.Clear();

    foreach (QEntry entry in data.entries)
    {
        qTable[entry.state] = entry.values;
    }
    epsilon = Mathf.Clamp(data.savedEpsilon, minEpsilon, 1f);

    Debug.Log("Q-table loaded. States: " + qTable.Count);
    }

    float PredictLandingX()
    {
    float ballX = ball.transform.position.x;
    float ballY = ball.transform.position.y;

    Vector2 velocity = ball.rb.linearVelocity;
    float paddleY = paddle.transform.position.y;

    // Only predict when the ball is moving downward.
    if (velocity.y >= 0f)
        return ballX;

    // Time until the ball reaches the paddle's height.
    float timeToPaddle = (paddleY - ballY) / velocity.y;

    // Predict horizontal position before wall bounces.
    float projectedX = ballX + velocity.x * timeToPaddle;

    // Actual ball-center boundaries.
    float leftWall = -8.38f;
    float rightWall = 8.38f;
    float width = rightWall - leftWall;

    // Reflect the projected position across the walls.
    float distance = Mathf.Repeat(projectedX - leftWall, 2f * width);

    if (distance > width)
        distance = 2f * width - distance;

    return leftWall + distance;
    }
    void ObserveDiscreteState()
{
    Vector2 ballPosition = ball.transform.position;
    Vector2 paddlePosition = paddle.transform.position;
    Vector2 velocity = ball.rb.linearVelocity;

    // 1. Relative horizontal position: -8 to +8
    float relativeX = ballPosition.x - paddlePosition.x;

    relativeXBin = Mathf.Clamp(
        Mathf.FloorToInt(
            (relativeX + MAX_RELATIVE_X) /
            (2f * MAX_RELATIVE_X) * X_BINS
        ),
        0,
        X_BINS - 1
    );

    // 2. Ball height above the paddle: 0 to 10
    float height = Mathf.Max(0f, ballPosition.y - paddlePosition.y);

    heightBin = Mathf.Clamp(
        Mathf.FloorToInt(height / MAX_BALL_HEIGHT * Y_BINS),
        0,
        Y_BINS - 1
    );

    // 3. Horizontal velocity: left, nearly vertical, right
    if (velocity.x < -0.5f)
        velocityXBin = 0;
    else if (velocity.x > 0.5f)
        velocityXBin = 2;
    else
        velocityXBin = 1;

    // 4. Vertical velocity: downward or upward
    velocityYBin = velocity.y > 0f ? 1 : 0;
}
}
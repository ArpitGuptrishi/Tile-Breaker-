using UnityEngine;

public class SpawnBricks : MonoBehaviour
{
    public GameObject Brick;

    void Start()
    {
        Debug.Log("The code ran");

        for (int i = 8; i >= 0; i--)
        {
            for (int j = 4; j >= 0; j--)
            {
                Instantiate(Brick, new Vector3(i, j, 0f), Quaternion.identity);

                if (i != 0)
                {
                    Instantiate(Brick, new Vector3(-i, j, 0f), Quaternion.identity);
                }
            }
        }
    }
}
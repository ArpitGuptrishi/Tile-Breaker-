using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreKeeper : MonoBehaviour
{
   
   public int startScore=100;
   private int BrickCount =84;
   [SerializeField]public int BrickBreakScore=5;
   [SerializeField]public int BallDropScore=-10;
    public Text text;
    void Start()
    {
        text.text= startScore.ToString();
    }
    public void addScore()
    {
        startScore+=BrickBreakScore;
        text.text = startScore.ToString();
    }
    public void decScore()
    {
        startScore+=BallDropScore;
        text.text = startScore.ToString();
    }
    public void decBrickCount()
    {
       
        if (BrickCount == 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        BrickCount--;
    }
    
}

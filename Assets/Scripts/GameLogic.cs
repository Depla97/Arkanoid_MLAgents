using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System;

public class GameLogic : MonoBehaviour
{
    public static event Action OnGameOver;
    public static event Action<int> OnLevelCleared;

    private GameObject currentLevel;

    [SerializeField]
    int lives;

    [SerializeField]
    int score;
    
    private int level;

    [SerializeField]
    GameObject levelContainer;

    [SerializeField]
    GameObject[] levelPrefabs;
    
    [SerializeField]
    GameObject PlayerPad;
    
    private void OnEnable()
    {
        Pad.OnLostLife += HandleOnLostLife;
        Bricks.OnAllBricksDestroyed += HandleNextLevel;
        Bricks.OnBrickDestroyed += HandleBrokenBrick;
    }

    private void OnDisable()
    {
        Pad.OnLostLife -= HandleOnLostLife;
        Bricks.OnAllBricksDestroyed -= HandleNextLevel;
        Bricks.OnBrickDestroyed -= HandleBrokenBrick;
    }

    private void Awake()
    {
        LoadLevelMap(level);
    }

    void HandleOnLostLife()
    {
        lives -= 1;
        // Debug.Log("Vita persa");
        if (lives == 0)
        {
            Debug.Log("Game over");
            lives = 3;
            //PlayerPad.GetComponent<PadAgent>().gameover();
            //OnGameOver?.Invoke();
            //StartCoroutine(nameof(GameOver));
        }
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSeconds(0.5f);

        //SceneManager.LoadScene("GameOver");
    }

    void Won()
    {
        SceneManager.LoadScene("Won");
    }

    public void HandleNextLevel()
    {
         OnLevelCleared?.Invoke(level);
         
         PlayerPad.GetComponent<PadAgent>().FinishLevel(level);
         LoadNewLevel();
    }

    public void LoadNewLevel()
    {
        level += 1;
        print("next level:"+level);
        LoadLevelMap(level);
        //SceneManager.LoadScene("Levels");
    }

    void LoadLevelMap(int levelNo)
    {
        if (levelNo >= 1 && levelNo <= levelPrefabs.Length)
        {
            Grid grid = levelContainer.GetComponent<Grid>();
            if (grid != null)
            {
                // TODO: add better config for levels,
                // with configurable grid cells size, backgrounds and possibly music.
                grid.cellSize = new Vector3(levelNo == 6 ? 1 : 3, 1, 0);
            }

            // Load a level prefab. Note that level numbers are 1-based.
            GameObject levelPrefab = levelPrefabs[levelNo - 1];
            currentLevel = Instantiate(levelPrefab, levelContainer.transform);
            currentLevel.GetComponent<Bricks>().AssignPad(PlayerPad.GetComponent<Pad>());
            currentLevel.GetComponent<Bricks>().AssignLogic(this);

        }
        else if (levelNo > levelPrefabs.Length)
        {
            PlayerPad.GetComponent<PadAgent>().Victory();
            //Won();
        }
    }

    public void ReloadLevel(int selectLevel)
    {
        print("reloading level");
        Destroy(currentLevel);
        print("loaded level:"+selectLevel);
        LoadLevelMap(selectLevel);
        level = selectLevel;
    }

    void HandleBrokenBrick(TileBase _brick)
    {
        score += 1;
    }
}

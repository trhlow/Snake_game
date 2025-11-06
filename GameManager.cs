using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("=== GRID SETTINGS ===")]
    public int gridWidth = 40;
    public int gridHeight = 30;
    public float cellSize = 0.5f;

    public enum Difficulty { Easy, Normal, Hard }

    [Header("=== DIFFICULTY SETTINGS ===")]
    public Difficulty currentDifficulty = Difficulty.Normal;

    private Dictionary<Difficulty, float> difficultySpeed = new Dictionary<Difficulty, float>()
    {
        { Difficulty.Easy, 0.15f },
        { Difficulty.Normal, 0.1f },
        { Difficulty.Hard, 0.06f }
    };

    [Header("=== PLAYER SETTINGS ===")]
    public Color player1Color = Color.green;
    public Color player2Color = Color.red;
    public Color aiColor = Color.red;

    private List<Snake> snakes = new List<Snake>();
    private List<Food> foods = new List<Food>();
    private bool gameOver = false;
    private bool gamePaused = false;
    private int foodsEatenThisCycle = 0;
    private List<GameObject> borderWalls = new List<GameObject>();
    private int currentGameMode = 0; // 0=none, 1=single, 2=multi, 3=ai

    public float moveDelay => difficultySpeed[currentDifficulty];

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log("=== SNAKE GAME START ===");
        SetupCamera();
        CreateBorders();
        LoadDifficulty();
    }

    void Update()
    {
        // Pause menu
        if (Input.GetKeyDown(KeyCode.Escape) && !gameOver && currentGameMode != 0)
        {
            TogglePause();
        }

        if (gamePaused) return;

        if (gameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartCurrentMode();
        }

        if (gameOver && Input.GetKeyDown(KeyCode.M))
        {
            ReturnToMenu();
        }
    }

    void LoadDifficulty()
    {
        int savedDiff = PlayerPrefs.GetInt("Difficulty", 1);
        currentDifficulty = (Difficulty)savedDiff;
    }

    public void SetDifficulty(Difficulty diff)
    {
        currentDifficulty = diff;
        PlayerPrefs.SetInt("Difficulty", (int)diff);
        PlayerPrefs.Save();

        Debug.Log($"Difficulty set to: {diff}");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("Click");
        }
    }

    public void TogglePause()
    {
        gamePaused = !gamePaused;

        if (UIManager.Instance != null)
        {
            if (gamePaused)
            {
                UIManager.Instance.ShowPauseMenu();
                Time.timeScale = 0f;
            }
            else
            {
                UIManager.Instance.ShowGameUI();
                Time.timeScale = 1f;
            }
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("Click");
        }

        Debug.Log(gamePaused ? "Game PAUSED" : "Game RESUMED");
    }

    public void ResumeGame()
    {
        gamePaused = false;
        Time.timeScale = 1f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameUI();
        }
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
        }

        float centerX = (gridWidth * cellSize) / 2f;
        float centerY = (gridHeight * cellSize) / 2f;

        cam.transform.position = new Vector3(centerX, centerY, -20f);
        cam.orthographic = true;
        cam.orthographicSize = Mathf.Max(gridHeight * cellSize / 2f + 2f, 10f);
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
    }

    void CreateBorders()
    {
        foreach (var wall in borderWalls)
        {
            if (wall != null) Destroy(wall);
        }
        borderWalls.Clear();

        Color borderColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);

        CreateBorderLine(new Vector3(0, gridHeight * cellSize, 0),
                        new Vector3(gridWidth * cellSize, gridHeight * cellSize, 0), borderColor);

        CreateBorderLine(new Vector3(0, 0, 0),
                        new Vector3(gridWidth * cellSize, 0, 0), borderColor);

        CreateBorderLine(new Vector3(0, 0, 0),
                        new Vector3(0, gridHeight * cellSize, 0), borderColor);

        CreateBorderLine(new Vector3(gridWidth * cellSize, 0, 0),
                        new Vector3(gridWidth * cellSize, gridHeight * cellSize, 0), borderColor);
    }

    void CreateBorderLine(Vector3 start, Vector3 end, Color color)
    {
        GameObject lineObj = new GameObject("Border");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.sortingOrder = 10;

        borderWalls.Add(lineObj);
    }

    void RestartCurrentMode()
    {
        Time.timeScale = 1f;
        gamePaused = false;

        MenuManager menu = FindFirstObjectByType<MenuManager>();
        if (menu != null)
        {
            menu.RestartLastMode();
        }
    }

    void ReturnToMenu()
    {
        Time.timeScale = 1f;
        gamePaused = false;

        MenuManager menu = FindObjectOfType<MenuManager>();
        if (menu != null)
        {
            menu.ReturnToMenu();
        }
    }

    void ClearGame()
    {
        foreach (var snake in snakes)
        {
            if (snake != null) Destroy(snake.gameObject);
        }
        snakes.Clear();

        foreach (var food in foods)
        {
            if (food != null) Destroy(food.gameObject);
        }
        foods.Clear();

        foodsEatenThisCycle = 0;
    }

    Snake CreateSnake(Vector2 gridPos, Color color, string name)
    {
        GameObject obj = new GameObject("Snake_" + name);
        Snake snake = obj.AddComponent<Snake>();
        snake.Initialize(gridPos, color, name);
        return snake;
    }

    public void StartSinglePlayer()
    {
        ClearGame();
        currentGameMode = 1;
        gameOver = false;
        gamePaused = false;
        Time.timeScale = 1f;

        Debug.Log("🎮 Single Player Mode Started!");
        Debug.Log($"Difficulty: {currentDifficulty}");

        Vector2 startPos = new Vector2(gridWidth / 2f, gridHeight / 2f);
        Snake snake = CreateSnake(startPos, player1Color, "Player");
        snakes.Add(snake);

        for (int i = 0; i < 5; i++)
        {
            SpawnFood(FoodType.Small);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("GameStart");
        }
    }

    public void StartMultiplayer()
    {
        ClearGame();
        currentGameMode = 2;
        gameOver = false;
        gamePaused = false;
        Time.timeScale = 1f;

        Debug.Log("🎮 Two Player Mode Started!");

        Vector2 startPos1 = new Vector2(gridWidth / 4f, gridHeight / 2f);
        GameObject obj1 = new GameObject("Snake_Player1");
        SnakeMultiplayer snake1 = obj1.AddComponent<SnakeMultiplayer>();
        snake1.Initialize(startPos1, player1Color, "Player 1");
        snake1.controlScheme = SnakeMultiplayer.ControlScheme.Arrows;
        snakes.Add(snake1);

        Vector2 startPos2 = new Vector2(3 * gridWidth / 4f, gridHeight / 2f);
        GameObject obj2 = new GameObject("Snake_Player2");
        SnakeMultiplayer snake2 = obj2.AddComponent<SnakeMultiplayer>();
        snake2.Initialize(startPos2, player2Color, "Player 2");
        snake2.controlScheme = SnakeMultiplayer.ControlScheme.WASD;
        snakes.Add(snake2);

        for (int i = 0; i < 8; i++)
        {
            SpawnFood(FoodType.Small);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("GameStart");
        }
    }

    public void StartAIMode()
    {
        ClearGame();
        currentGameMode = 3;
        gameOver = false;
        gamePaused = false;
        Time.timeScale = 1f;

        Debug.Log("🎮 VS AI Mode Started!");

        Vector2 startPos1 = new Vector2(gridWidth / 4f, gridHeight / 2f);
        Snake player = CreateSnake(startPos1, player1Color, "Player");
        snakes.Add(player);

        Vector2 startPos2 = new Vector2(3 * gridWidth / 4f, gridHeight / 2f);
        GameObject aiObj = new GameObject("Snake_AI");
        AISnake aiSnake = aiObj.AddComponent<AISnake>();
        aiSnake.Initialize(startPos2, aiColor, "AI");
        snakes.Add(aiSnake);

        for (int i = 0; i < 8; i++)
        {
            SpawnFood(FoodType.Small);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("GameStart");
        }
    }

    public void OnFoodEaten(Food food)
    {
        foods.Remove(food);
        Destroy(food.gameObject);

        foodsEatenThisCycle++;

        if (foodsEatenThisCycle >= 3)
        {
            foodsEatenThisCycle = 0;

            for (int i = 0; i < 5; i++)
            {
                SpawnFood(FoodType.Small);
            }

            SpawnFood(FoodType.Large);

            if (Random.value > 0.7f)
            {
                SpawnFood(FoodType.Super);
            }
        }
    }

    void SpawnFood(FoodType type)
    {
        Vector2Int pos = GetRandomFreePosition();
        if (pos.x >= 0)
        {
            GameObject obj = new GameObject("Food_" + type);
            Food food = obj.AddComponent<Food>();
            food.Initialize(type, pos);
            foods.Add(food);
        }
        else
        {
            Debug.LogWarning("Cannot spawn food - map is full!");
        }
    }

    public Vector2Int GetRandomFreePosition()
    {
        List<Vector2Int> occupied = new List<Vector2Int>();

        foreach (var snake in snakes)
        {
            if (snake != null)
            {
                occupied.AddRange(snake.body);
            }
        }

        foreach (var food in foods)
        {
            if (food != null)
            {
                occupied.Add(food.gridPosition);
            }
        }

        List<Vector2Int> available = new List<Vector2Int>();
        for (int x = 1; x < gridWidth - 1; x++)
        {
            for (int y = 1; y < gridHeight - 1; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!occupied.Contains(pos))
                {
                    available.Add(pos);
                }
            }
        }

        if (available.Count > 0)
        {
            return available[Random.Range(0, available.Count)];
        }

        return new Vector2Int(-1, -1);
    }

    public void OnSnakeDeath(Snake deadSnake)
    {
        int aliveCount = 0;
        Snake winner = null;

        foreach (var snake in snakes)
        {
            if (snake != null && snake.alive)
            {
                aliveCount++;
                winner = snake;
            }
        }

        if (aliveCount == 0 || (snakes.Count > 1 && aliveCount == 1))
        {
            gameOver = true;
            currentGameMode = 0;

            string winnerText = "GAME OVER!";
            int finalScore = 0;

            if (winner != null)
            {
                winnerText = $"🏆 {winner.snakeName} WINS!";
                finalScore = winner.score;
            }
            else if (snakes.Count > 0 && snakes[0] != null)
            {
                finalScore = snakes[0].score;
            }

            // Check and save highscore
            HighscoreManager.Instance?.CheckAndSaveHighscore(finalScore);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowGameOver(winnerText, finalScore);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("GameOver");
            }

            Debug.Log("╔═══════════════════════════════════╗");
            Debug.Log("║         GAME OVER!                ║");
            Debug.Log("╚═══════════════════════════════════╝");

            if (winner != null)
            {
                Debug.Log($"🏆 WINNER: {winner.snakeName}!");
                Debug.Log($"Final Score: {winner.score}");
            }
        }

        int foodCount = Mathf.Min(deadSnake.score / 10, 15);
        for (int i = 0; i < foodCount && i < deadSnake.body.Count; i++)
        {
            SpawnFood(FoodType.Small);
        }
    }

    public bool CheckCollisionWithOtherSnake(Snake currentSnake, Vector2Int pos)
    {
        foreach (var snake in snakes)
        {
            if (snake != null && snake != currentSnake && snake.alive)
            {
                for (int i = 1; i < snake.body.Count; i++)
                {
                    if (snake.body[i] == pos)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void CheckHeadToHeadCollision()
    {
        if (snakes.Count < 2) return;

        for (int i = 0; i < snakes.Count; i++)
        {
            for (int j = i + 1; j < snakes.Count; j++)
            {
                Snake snake1 = snakes[i];
                Snake snake2 = snakes[j];

                if (snake1 != null && snake2 != null &&
                    snake1.alive && snake2.alive &&
                    snake1.body.Count > 0 && snake2.body.Count > 0)
                {
                    if (snake1.body[0] == snake2.body[0])
                    {
                        Debug.Log("💥 HEAD TO HEAD COLLISION!");

                        if (Random.value < 0.5f)
                        {
                            snake1.Die();
                        }
                        else
                        {
                            snake2.Die();
                        }
                    }
                }
            }
        }
    }

    public List<Food> GetAllFoods()
    {
        return foods;
    }

    public List<Snake> GetAllSnakes()
    {
        return snakes;
    }

    public bool IsGamePaused()
    {
        return gamePaused;
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0);
    }
}
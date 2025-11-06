using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Up, Down, Left, Right
}

public class Snake : MonoBehaviour
{
    public List<Vector2Int> body = new List<Vector2Int>();
    public Direction currentDirection = Direction.Right;
    public int score = 0;
    public bool alive = true;
    public Color snakeColor = Color.green;
    public string snakeName = Player;

    protected Direction nextDirection;
    protected float lastMoveTime = 0f;
    protected int growQueue = 0;
    protected List<GameObject> bodySegments = new List<GameObject>();
    private int combo = 0;
    private float lastEatTime = 0f;
    
    // Input buffer
    protected Queue<Direction> inputBuffer = new Queue<Direction>();
    protected const int maxBufferSize = 3;

    public virtual void Initialize(Vector2 startPos, Color color, string name)
    {
        snakeColor = color;
        snakeName = name;

        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(startPos.x), Mathf.RoundToInt(startPos.y));
        body.Add(gridPos);
        body.Add(new Vector2Int(gridPos.x - 1, gridPos.y));
        body.Add(new Vector2Int(gridPos.x - 2, gridPos.y));

        currentDirection = Direction.Right;
        nextDirection = Direction.Right;

        CreateVisualBody();
    }

    void Update()
    {

        HandleInput();

        if (Time.time - lastEatTime > 2f && combo > 0)
        {
            combo = 0;
            UpdateComboUI();
        }

        if (Time.time - lastMoveTime >= GameManager.Instance.moveDelay)
        {
            Move();
            lastMoveTime = Time.time;
        }
    }

    protected virtual void HandleInput()
    {
        Direction? newDirection = null;
        
        if (Input.GetKeyDown(KeyCode.UpArrow) && currentDirection != Direction.Down)
            newDirection = Direction.Up;
        else if (Input.GetKeyDown(KeyCode.DownArrow) && currentDirection != Direction.Up)
            newDirection = Direction.Down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && currentDirection != Direction.Right)
            newDirection = Direction.Left;
        else if (Input.GetKeyDown(KeyCode.RightArrow) && currentDirection != Direction.Left)
            newDirection = Direction.Right;
        
        if (newDirection.HasValue && inputBuffer.Count < maxBufferSize)
        {
            // Only add if different from last buffered direction
            if (inputBuffer.Count == 0 || inputBuffer.ToArray()[inputBuffer.Count - 1] != newDirection.Value)
            {
                inputBuffer.Enqueue(newDirection.Value);
            }
        }
    }

    protected virtual void Move()
    {
        // Process input buffer
        if (inputBuffer.Count > 0)
        {
            Direction bufferedDir = inputBuffer.Dequeue();
            {
                nextDirection = bufferedDir;
            }
        }
        
        currentDirection = nextDirection;

        Vector2Int head = body[0];
        Vector2Int newHead = head + GetDirectionVector(currentDirection);

        if (CheckCollision(newHead))
        {
            Die();
            return;
        }

        body.Insert(0, newHead);
        CheckFoodCollision(newHead);

        if (growQueue > 0)
        {
            growQueue--;
        }
        else
        {
            body.RemoveAt(body.Count - 1);
        }

        UpdateVisualBody();

        GameManager.Instance.CheckHeadToHeadCollision();
    }

    bool IsOppositeDirection(Direction dir1, Direction dir2)
    {
        return (dir1 == Direction.Up && dir2 == Direction.Down) ||
               (dir1 == Direction.Down && dir2 == Direction.Up) ||
               (dir1 == Direction.Left && dir2 == Direction.Right) ||
               (dir1 == Direction.Right && dir2 == Direction.Left);
    }

    Vector2Int GetDirectionVector(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return Vector2Int.up;
            case Direction.Down: return Vector2Int.down;
            case Direction.Left: return Vector2Int.left;
            case Direction.Right: return Vector2Int.right;
            default: return Vector2Int.zero;
        }
    }

    bool CheckCollision(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= GameManager.Instance.gridWidth ||
            pos.y < 0 || pos.y >= GameManager.Instance.gridHeight)
        {
            return true;
        }

        for (int i = 1; i < body.Count; i++)
        {
            if (body[i] == pos) return true;
        }

        if (GameManager.Instance.CheckCollisionWithOtherSnake(this, pos))
        {
            return true;
        }

        return false;
    }

    void CheckFoodCollision(Vector2Int pos)
    {
        List<Food> foods = GameManager.Instance.GetAllFoods();
        foreach (var food in foods)
        {
            if (food.gridPosition == pos)
            {
                EatFood(food);
                break;
            }
        }
    }

    void EatFood(Food food)
    {
        if (Time.time - lastEatTime <= 2f)
        {
            combo++;
        }
        else
        {
            combo = 1;
        }
        lastEatTime = Time.time;

        int baseScore = 0;
        switch (food.type)
        {
            case FoodType.Small:
                growQueue += 1;
                baseScore = 10;
                break;
            case FoodType.Large:
                growQueue += 2;
                baseScore = 20;
                break;
            case FoodType.Super:
                growQueue += 3;
                baseScore = 50;
                break;
        }

        int multiplier = Mathf.Min(combo, 5);
        int earnedScore = baseScore * multiplier;
        score += earnedScore;

        if (combo > 1)
        {
            Debug.Log($🔥 {snakeName} COMBO x{combo}! +{earnedScore} points! Total: {score});
        }
        else
        {
            Debug.Log(${snakeName} ate {food.type}! +{earnedScore} points! Score: {score});
        }
        
        UpdateScoreUI();
        UpdateComboUI();

        if (AudioManager.Instance != null)
        {
            if (combo > 3)
                AudioManager.Instance.PlaySound(ComboHigh);
            else if (combo > 1)
                AudioManager.Instance.PlaySound(ComboLow);
            else
                AudioManager.Instance.PlaySound(Eat);
        }

        CreateEatParticles(food.transform.position, food.GetVisualColor());
        StartCoroutine(FlashEffect());

        GameManager.Instance.OnFoodEaten(food);
    }
    
    void UpdateScoreUI()
    {
        if (UIManager.Instance != null)
        {
            var allSnakes = GameManager.Instance.GetAllSnakes();
            if (allSnakes.Count > 1)
            {
                UIManager.Instance.UpdateMultiplayerScores(
                    allSnakes[0].score, 
                    allSnakes[1].score
                );
            }
            else
            {
                UIManager.Instance.UpdateScoreUI($Score: {score});
            }
        }
    }

    void UpdateComboUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateComboUI(combo);
        }
    }

    void CreateEatParticles(Vector3 position, Color color)
    {
        GameObject particleObj = ParticlePool.Instance?.GetParticle();
        
        if (particleObj == null)
        {
            particleObj = new GameObject(Particle);
            particleObj.AddComponent<ParticleSystem>();
        }
        
        particleObj.transform.position = position;

        ParticleSystem ps = particleObj.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = color;
        main.startSize = 0.2f;
        main.startSpeed = 3f;
        main.startLifetime = 0.5f;
        main.maxParticles = 20;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });

        ps.Play();
        
        StartCoroutine(ReturnParticleToPool(particleObj, 1f));
    }

    IEnumerator ReturnParticleToPool(GameObject particleObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (ParticlePool.Instance != null)
        {
            ParticlePool.Instance.ReturnParticle(particleObj);
        }
        else
        {
            Destroy(particleObj);
        }
    }

    IEnumerator FlashEffect()
    {
        foreach (var seg in bodySegments)
        {
            if (seg != null)
            {
                Renderer r = seg.GetComponent<Renderer>();
                if (r != null) r.material.color = Color.white;
            }
        }

        yield return new WaitForSeconds(0.1f);

        UpdateBodyColors();
    }

    public void Die()
    {
        
        alive = false;
        inputBuffer.Clear();
        
        Debug.Log($💀 {snakeName} died! Final Score: {score});

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(Death);
        }

        CreateDeathExplosion();
        StartCoroutine(ScreenShake());
        GameManager.Instance.OnSnakeDeath(this);

        foreach (var seg in bodySegments)
        {
            if (seg != null)
            {
                Renderer r = seg.GetComponent<Renderer>();
                if (r != null)
                {
                    Color c = r.material.color;
                    c.a = 0.3f;
                    r.material.color = c;
                }
            }
        }
    }

    void CreateDeathExplosion()
    {
        if (body.Count == 0) return;
        
        GameObject particleObj = new GameObject(DeathParticle);
        particleObj.transform.position = GameManager.Instance.GridToWorld(body[0]);

        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = snakeColor;
        main.startSize = 0.3f;
        main.startSpeed = 5f;
        main.startLifetime = 1f;
        main.maxParticles = 50;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 50) });

        Destroy(particleObj, 2f);
    }

    IEnumerator ScreenShake()
    {
        if (Camera.main == null) yield break;

        Vector3 originalPos = Camera.main.transform.position;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-0.2f, 0.2f);
            float y = Random.Range(-0.2f, 0.2f);
            Camera.main.transform.position = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.position = originalPos;
    }

    void CreateVisualBody()
    {
        foreach (var seg in bodySegments)
        {
            if (seg != null) Destroy(seg);
        }
        bodySegments.Clear();

        for (int i = 0; i < body.Count; i++)
        {
            CreateSegment(i);
        }
    }

    void CreateSegment(int index)
    {
        GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seg.transform.parent = transform;
        seg.transform.localScale = Vector3.one * GameManager.Instance.cellSize * 0.9f;

        Renderer r = seg.GetComponent<Renderer>();
        if (r != null)
        {
            float t = (float)index / Mathf.Max(1, body.Count);
            Color c = Color.Lerp(snakeColor, snakeColor * 0.5f, t);
            r.material.color = c;
        }

        Destroy(seg.GetComponent<Collider>());
        bodySegments.Add(seg);
    }

    void UpdateVisualBody()
    {
        while (bodySegments.Count < body.Count)
        {
            CreateSegment(bodySegments.Count);
        }

        while (bodySegments.Count > body.Count)
        {
            GameObject seg = bodySegments[bodySegments.Count - 1];
            bodySegments.RemoveAt(bodySegments.Count - 1);
            Destroy(seg);
        }

        for (int i = 0; i < body.Count; i++)
        {
            if (bodySegments[i] != null)
            {
                bodySegments[i].transform.position = GameManager.Instance.GridToWorld(body[i]);
            }
        }

        UpdateBodyColors();
    }

    void UpdateBodyColors()
    {
        for (int i = 0; i < bodySegments.Count; i++)
        {
            if (bodySegments[i] != null)
            {
                Renderer r = bodySegments[i].GetComponent<Renderer>();
                if (r != null)
                {
                    float t = (float)i / Mathf.Max(1, body.Count);
                    Color c = Color.Lerp(snakeColor, snakeColor * 0.5f, t);
                    r.material.color = c;
                }
            }
        }
    }
}

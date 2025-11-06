using System.Collections.Generic;
using UnityEngine;

public class AISnake : Snake
{
    private Vector2Int targetFoodPos;
    private float thinkDelay = 0.08f;
    private float lastThinkTime = 0f;
    private List<Vector2Int> currentPath = null;

    protected override void HandleInput()
    {
        if (Time.time - lastThinkTime >= thinkDelay)
        {
            FindPathToFood();
            lastThinkTime = Time.time;
        }
    }

    void FindPathToFood()
    {
        List<Food> foods = GameManager.Instance.GetAllFoods();
        if (foods.Count == 0) return;

        Food bestFood = null;
        List<Vector2Int> bestPath = null;
        float bestScore = float.MaxValue;

        Vector2Int head = body[0];
        
        foreach (var food in foods)
        {
            List<Vector2Int> path = FindSafePath(head, food.gridPosition);
            
            if (path != null && path.Count > 0)
            {
                float priority = path.Count;
                
                if (food.type == FoodType.Super)
                    priority *= 0.4f;
                else if (food.type == FoodType.Large)
                    priority *= 0.6f;

                if (priority < bestScore)
                {
                    bestScore = priority;
                    bestFood = food;
                    bestPath = path;
                }
            }
        }

        if (bestPath != null && bestPath.Count > 1)
        {
            currentPath = bestPath;
            targetFoodPos = bestFood.gridPosition;
            
            Vector2Int nextPos = bestPath[1];
            DecideDirectionToTarget(nextPos);
        }
        else
        {
            FindSafeDirection();
        }
    }

    List<Vector2Int> FindSafePath(Vector2Int start, Vector2Int target)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        queue.Enqueue(start);
        visited.Add(start);
        
        int maxIterations = 500;
        int iterations = 0;
        
        while (queue.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            Vector2Int current = queue.Dequeue();
            
            if (current == target)
            {
                return ReconstructPath(cameFrom, current);
            }
            
            Direction[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
            
            foreach (Direction dir in directions)
            {
                Vector2Int next = current + GetDirectionVector(dir);
                
                {
                    visited.Add(next);
                    cameFrom[next] = current;
                    queue.Enqueue(next);
                }
            }
        }
        
        return null;
    }

    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        path.Add(current);
        
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        
        return path;
    }

    bool IsValidPosition(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= GameManager.Instance.gridWidth ||
            pos.y < 0 || pos.y >= GameManager.Instance.gridHeight)
            return false;
        
        if (body.Contains(pos))
            return false;
        
        if (GameManager.Instance.CheckCollisionWithOtherSnake(this, pos))
            return false;
        
        return true;
    }

    void DecideDirectionToTarget(Vector2Int target)
    {
        Vector2Int head = body[0];
        int dx = target.x - head.x;
        int dy = target.y - head.y;
        
        Direction desiredDir = Direction.Right;
        
        if (Mathf.Abs(dx) > Mathf.Abs(dy))
        {
            desiredDir = dx > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            desiredDir = dy > 0 ? Direction.Up : Direction.Down;
        }
        
        if (IsValidDirection(desiredDir))
        {
            nextDirection = desiredDir;
        }
        else
        {
            FindSafeDirection();
        }
    }

    void FindSafeDirection()
    {
        Direction[] allDirections = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        
        List<Direction> safeDirections = new List<Direction>();
        
        foreach (Direction dir in allDirections)
        {
            if (IsValidDirection(dir))
            {
                safeDirections.Add(dir);
            }
        }

        if (safeDirections.Count > 0)
        {
            Direction bestDir = safeDirections[0];
            int maxSpace = 0;

            foreach (Direction dir in safeDirections)
            {
                int space = CountSpaceInDirection(dir);
                if (space > maxSpace)
                {
                    maxSpace = space;
                    bestDir = dir;
                }
            }

            nextDirection = bestDir;
        }
    }

    int CountSpaceInDirection(Direction dir)
    {
        Vector2Int pos = body[0] + GetDirectionVector(dir);
        int count = 0;
        int maxCheck = 5;

        while (count < maxCheck && IsValidPosition(pos))
        {
            count++;
            pos += GetDirectionVector(dir);
        }

        return count;
    }

    bool IsValidDirection(Direction dir)
    {
        if (IsOppositeDirection(dir, currentDirection))
            return false;

        Vector2Int head = body[0];
        Vector2Int nextPos = head + GetDirectionVector(dir);

        if (nextPos.x < 0 || nextPos.x >= GameManager.Instance.gridWidth ||
            nextPos.y < 0 || nextPos.y >= GameManager.Instance.gridHeight)
        {
            return false;
        }

        if (body.Contains(nextPos))
        {
            return false;
        }

        if (GameManager.Instance.CheckCollisionWithOtherSnake(this, nextPos))
        {
            return false;
        }

        return true;
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
}

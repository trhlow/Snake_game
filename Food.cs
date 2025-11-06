using UnityEngine;

public enum FoodType
{
    Small, Large, Super
}

public class Food : MonoBehaviour
{
    public FoodType type;
    public Vector2Int gridPosition;
    private GameObject visualObj;
    private Vector3 originalScale;

    public void Initialize(FoodType foodType, Vector2Int pos)
    {
        type = foodType;
        gridPosition = pos;
        transform.position = GameManager.Instance.GridToWorld(pos);
        CreateVisual();
    }

    void CreateVisual()
    {
        visualObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visualObj.transform.parent = transform;
        visualObj.transform.localPosition = Vector3.zero;
        float size = GetSize();
        visualObj.transform.localScale = new Vector3(size, size, size);
        originalScale = visualObj.transform.localScale;
        Renderer r = visualObj.GetComponent<Renderer>();
        if (r != null) r.material.color = GetVisualColor();
        Destroy(visualObj.GetComponent<Collider>());
    }

    float GetSize()
    {
        float cellSize = GameManager.Instance.cellSize;
        switch (type)
        {
            case FoodType.Small: return cellSize * 0.5f;
            case FoodType.Large: return cellSize * 0.7f;
            case FoodType.Super: return cellSize * 0.9f;
            default: return cellSize * 0.5f;
        }
    }

    public Color GetVisualColor()
    {
        switch (type)
        {
            case FoodType.Small: return new Color(0.3f, 1f, 0.3f);
            case FoodType.Large: return new Color(1f, 1f, 0.3f);
            case FoodType.Super: return new Color(1f, 0.5f, 0f);
            default: return Color.green;
        }
    }

    void Update()
    {
        if (visualObj != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.15f;
            visualObj.transform.localScale = originalScale * pulse;
            float floatY = Mathf.Sin(Time.time * 2f) * 0.1f;
            visualObj.transform.localPosition = new Vector3(0, floatY, 0);
            if (type == FoodType.Super)
                visualObj.transform.Rotate(Vector3.up, 120f * Time.deltaTime);
        }
    }
}
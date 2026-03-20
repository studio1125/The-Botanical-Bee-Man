using UnityEngine;

public class GameManager : MonoBehaviour {

    [Header("References")]
    [SerializeField] private BoxCollider2D mapBounds;

    public BoxCollider2D GetMapBounds() => mapBounds;

}

using UnityEngine;

public class CameraView : MonoBehaviour
{
    private GameSystem _gameSystem;

    private void Awake()
    {
        _gameSystem = FindObjectOfType<GameSystem>();
    }

    private void Start()
    {
        SetCameraPosition();
    }

    private void SetCameraPosition()
    {
        if (_gameSystem == null) return;

        transform.position = new Vector3(_gameSystem._width / 2f, _gameSystem._height / 2f, -10f);
    }
}

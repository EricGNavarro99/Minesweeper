using UnityEngine;

public class CameraView : MonoBehaviour
{
    private GameSystem _gameSystem;
    private Camera _cameraComponent;

    private void Awake()
    {
        _gameSystem ??= FindObjectOfType<GameSystem>();
        _cameraComponent ??= GetComponent<Camera>();

    }

    private void Start()
    {
        SetCameraPosition();
        CentrateBoardOnScreen();
    }

    private void SetCameraPosition()
    {
        if (_gameSystem == null) return;

        transform.position = new Vector3(_gameSystem._width / 2f, _gameSystem._height / 2f, -10f);
    }

    private void CentrateBoardOnScreen()
    {
        if (_cameraComponent == null && _gameSystem == null) return;

        float cellsOnScreen = _gameSystem._width * _gameSystem._height;
        float cameraOrtographicSize = Mathf.Clamp(cellsOnScreen / 10, 5, 18);

        _cameraComponent.orthographicSize = cameraOrtographicSize;
    }
}

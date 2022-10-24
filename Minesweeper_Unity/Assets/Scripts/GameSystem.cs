using System.Collections;
using UnityEngine;

public enum Difficulty { _easy, _normal, _difficult }

public class GameSystem : MonoBehaviour
{
    [Space, Header("Sizes:")]
    [Range(9, 32)] public int _width = 9;
    [Range(9, 32)] public int _height = 9;

    [Space] public Difficulty _difficulty;

    [Space] public bool _revealAllCells = false;

    private int _mines = 0;

    private Board _board;
    private Cell[,] _state;

    private bool _gameOver = false;
    private bool _createdMap = false;

    private void Awake()
    {
        _board = GetComponentInChildren<Board>();
    }

    private void Start()
    {
        SetNewGame();

        Debug.Log(_mines);
    }

    private void Update()
    {
        if (!_gameOver && _createdMap)
        {
            if (Input.GetMouseButtonDown(0)) RevealCell();
            if (Input.GetMouseButtonDown(1)) FlagCell();
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            _gameOver = false;
            SetNewGame();
        }
    }

    private void SetNewGame()
    {
        _gameOver = false;
        _createdMap = false;
        GenerateTemporalCells();
        StartCoroutine(GenerateMap());
    }

    private IEnumerator GenerateMap()
    {
        yield return new WaitUntil(() => Input.GetMouseButtonUp(0));

        if (_state != null)
            _state = new Cell[_width, _height];

        GenerateCells();
        GenerateMines();
        SetNumbersInMap();

        _board.Draw(_state);

        _createdMap = true;
    }

    private void GenerateCells()
    {
        for (byte x = 0; x < _width; x++)
        {
            for (byte y = 0; y < _height; y++)
            {
                Cell cell = new Cell();

                cell._position = new Vector3Int(x, y, 0);
                cell._type = Cell.Type._empty;

                _state[x, y] = cell;
                if (_revealAllCells) _state[x, y]._revealed = true;
            }
        }
    }

    private void GenerateTemporalCells()
    {
        _state = new Cell[_width, _height];

        GenerateCells();
        _board.Draw(_state);
    }

    private void GenerateMines()
    {
        Vector3Int selectedPosition = GetSelectedCellPosition();

        for (byte a = 0; a < _mines; a++)
        {
            int x = GenerateSafeSpace(selectedPosition.x, _width);
            int y = GenerateSafeSpace(selectedPosition.y, _height);

            while (_state[x, y]._type == Cell.Type._mine)
            {
                x = GenerateSafeSpace(selectedPosition.x, _width);
                y = GenerateSafeSpace(selectedPosition.y, _height);
            }

            _state[x, y]._type = Cell.Type._mine;
        }
    }

    private void SetNumbersInMap()
    {
        for (byte x = 0; x < _width; x++)
        {
            for (byte y = 0; y < _height; y++)
            {
                Cell cell = _state[x, y];

                if (cell._type == Cell.Type._mine) continue;

                cell._number = CountNearbyMines(x, y);

                if (cell._number > 0) cell._type = Cell.Type._number;

                _state[x, y] = cell;
            }
        }
    }

    private void RevealCell()
    {
        Vector3Int selectedCell = GetSelectedCellPosition();
        Cell cell = GetCell(selectedCell.x, selectedCell.y);

        if (cell._type == Cell.Type._invalid || cell._revealed || cell._flagged) return;

        switch (cell._type)
        {
            case Cell.Type._mine: Explode(cell);
                break;

            case Cell.Type._empty:
            case Cell.Type._number:
                Flood(cell);
                CheckWinCondition();
                break;

            default:
                cell._revealed = true;
                _state[cell._position.x, cell._position.y] = cell;
                CheckWinCondition();
                break;
        }
        
        _board.Draw(_state);

    }

    private void FlagCell()
    {
        Vector3Int selectedCell = GetSelectedCellPosition();

        Cell cell = GetCell(selectedCell.x, selectedCell.y);

        if (cell._type == Cell.Type._invalid || cell._revealed) return;

        cell._flagged = !cell._flagged;
        _state[selectedCell.x, selectedCell.y] = cell;
        _board.Draw(_state);
    }

    private int GenerateSafeSpace(int selectedCell, int maxCellsInRow)
    {
        int randomCell = 0;
        maxCellsInRow -= 1;

        if (selectedCell == 0) randomCell = Random.Range(selectedCell + 1, maxCellsInRow);
        else if (selectedCell == maxCellsInRow) randomCell = Random.Range(0, selectedCell - 1);
        else if (selectedCell > 0 && selectedCell < maxCellsInRow)
        {
            int downLeft = Random.Range(0, selectedCell - 1);
            int topRight = Random.Range(selectedCell + 1, maxCellsInRow);
            randomCell = (Random.Range(0, 10) < 5) ? downLeft : topRight;
        }


        return randomCell;
    }

    private Vector3Int GetSelectedCellPosition()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = _board._tilemap.WorldToCell(worldPosition);

        if (cellPosition.x < 0 || cellPosition.y < 0 || cellPosition.x > _width || cellPosition.y > _height)
        {
            Debug.Log($"Selected position is outside the tilemap > {cellPosition.x}, {cellPosition.y}");

            cellPosition = new Vector3Int(Random.Range(0, _width), Random.Range(0, _height), 0);
            Debug.Log($"New current selected position > {cellPosition.x}, {cellPosition.y}");
        }

        return cellPosition;
    }

    private byte CountNearbyMines(byte cellX, byte cellY)
    {
        byte count = 0;

        for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if (adjacentX == 0 && adjacentY == 0) continue;

                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

                if (x < 0 || x >= _width || y < 0 || y >= _height) continue;

                if (GetCell(x, y)._type == Cell.Type._mine) count++;
            }
        }

        return count;
    }

    private Cell GetCell(int x, int y)
    {
        if (ValidateCell(x, y)) return _state[x, y];
        else return new Cell();
    }

    private bool ValidateCell(int x, int y)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height;
    }

    private void Explode(Cell cell)
    {
        Debug.Log("Game over!");
        _gameOver = true;

        cell._revealed = true;
        cell._exploded = true;
        _state[cell._position.x, cell._position.y] = cell;

        for (byte x = 0; x < _width; x++)
        {
            for (byte y = 0; y < _height; y++)
            {
                cell = _state[x, y];

                if (cell._type != Cell.Type._mine && cell._flagged)
                {
                    cell._flagged = false;
                    cell._failed = true;
                    _state[x, y] = cell;
                }
                else if (cell._type == Cell.Type._mine && !cell._flagged)
                {
                    cell._revealed = true;
                    _state[x, y] = cell;
                }
            }
        }
    }

    private void Flood(Cell cell)
    {
        if (cell._revealed) return;
        if (cell._type == Cell.Type._mine || cell._type == Cell.Type._invalid) return;

        cell._revealed = true;
        _state[cell._position.x, cell._position.y] = cell;

        if (cell._type == Cell.Type._empty || cell._number == 1)
        {
            Flood(GetCell(cell._position.x - 1, cell._position.y));
            Flood(GetCell(cell._position.x + 1, cell._position.y));
            Flood(GetCell(cell._position.x, cell._position.y - 1));
            Flood(GetCell(cell._position.x, cell._position.y + 1));
        }
    }

    private void CheckWinCondition()
    {
        if (!_createdMap) return;

        for (byte x = 0; x < _width; x++)
        {
            for (byte y = 0; y < _height; y++)
            {
                Cell cell = _state[x, y];

                if (cell._type != Cell.Type._mine && !cell._revealed) return;
            }
        }

        Debug.Log("Winner!");
        _gameOver = true;

        for (byte x = 0; x < _width; x++)
        {
            for (byte y = 0; y < _height; y++)
            {
                Cell cell = _state[x, y];

                if (cell._type == Cell.Type._mine)
                {
                    cell._flagged = true;
                    _state[x, y] = cell;
                }
            }
        }
    }
}
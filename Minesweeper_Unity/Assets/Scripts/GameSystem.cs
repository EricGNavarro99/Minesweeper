using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    [Range(9, 32)] public int _width = 9;
    [Range(9, 32)] public int _height = 9;
    private int _mines = 12;

    private Board _board;
    private Cell[,] _state;

    private bool _gameOver = false;

    private void Awake()
    {
        _board = GetComponentInChildren<Board>();
    }

    private void Start()
    {
        SetNewGame();
    }
    /*
    private void Update()
    {
        if (!_gameOver)
        {
            if (Input.GetMouseButtonUp(0)) RevealCell();
            if (Input.GetMouseButtonUp(1)) FlagCell();
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            _gameOver = false;
            SetNewGame();
        }
    }*/

    private void SetNewGame()
    {

        _state = new Cell[_width, _height];

        GenerateCells();
        StartCoroutine(GenerateMap());

        _board.Draw(_state);

        RevealAllCells();
    } //!
    
    private IEnumerator GenerateMap()
    {
        yield return new WaitUntil(() => Input.GetMouseButtonUp(0));

        GenerateMines();
        SetNumbersInMap();
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
            }
        }
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
                x++;
                y++;

                if (x > _width || y > _height)
                {
                    x = 0;
                    y = 0;

                    if (x == selectedPosition.x) x += 2;
                    if (y == selectedPosition.y) x += 2;
                }
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
            case Cell.Type._mine: ExplodeCell(cell); 
                break;

            case Cell.Type._empty:
                FloodCell(cell);
                CheckWinCondition();
                break;

            default:
                cell._revealed = true;
                _state[selectedCell.x, selectedCell.y] = cell;
                CheckWinCondition();
                break;
        }
    }

    private void RevealAllCells()
    {
        for (byte x = 0; x < _width; x++)
        {
            for (byte y = 0; y < _height; y++)
            {
                Cell cell = _state[x, y];

                cell._revealed = true;

                _state[x, y] = cell;
            }
        }
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

        if (selectedCell == 0) randomCell = Random.Range(selectedCell + 2, maxCellsInRow);
        else if (selectedCell == maxCellsInRow) randomCell = Random.Range(0, selectedCell - 2);
        else if (selectedCell > 0 && selectedCell < maxCellsInRow) randomCell = Random.Range(Random.Range(0, selectedCell - 2), Random.Range(selectedCell + 2, maxCellsInRow));
        else randomCell = 0;


        return randomCell;
    }

    private Vector3Int GetSelectedCellPosition()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = _board._tilemap.WorldToCell(worldPosition);

        if (cellPosition.x < 0 || cellPosition.y < 0 || cellPosition.x > _width || cellPosition.y > _height)
        {
            Debug.Log($"Selected position is outside the tilemap > {cellPosition.x}, {cellPosition.y}");
            return new Vector3Int(0, 0);
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

    private void ExplodeCell(Cell cell)
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

    private void FloodCell(Cell cell)
    {
        if (cell._revealed) return;
        if (cell._type == Cell.Type._mine || cell._type == Cell.Type._invalid) return;

        cell._revealed = true;
        _state[cell._position.x, cell._position.y] = cell;

        if (cell._type == Cell.Type._empty || cell._number == 1)
        {
            FloodCell(GetCell(cell._position.x - 1, cell._position.y));
            FloodCell(GetCell(cell._position.x + 1, cell._position.y));
            FloodCell(GetCell(cell._position.x, cell._position.y - 1));
            FloodCell(GetCell(cell._position.x, cell._position.y + 1));
        }
    }

    private void CheckWinCondition()
    {
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

                if (cell._type != Cell.Type._mine)
                {
                    cell._flagged = true;
                    _state[x, y] = cell;
                }
            }
        }
    }
}
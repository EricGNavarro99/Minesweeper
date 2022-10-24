using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap _tilemap { get; private set; }

    #region Tiles:

    public Tile _empty;
    public Tile _unknown;
    public Tile _num1;
    public Tile _num2;
    public Tile _num3;
    public Tile _num4;
    public Tile _num5;
    public Tile _num6;
    public Tile _num7;
    public Tile _num8;
    public Tile _flag;
    public Tile _exploded;
    public Tile _mine;
    public Tile _failed;

    #endregion

    private void Awake()
    {
        _tilemap = GetComponent<Tilemap>();
    }

    public void Draw(Cell[,] state)
    {
        int widht = state.GetLength(0);
        int height = state.GetLength(1);

        for (byte x = 0; x < widht; x++)
        {
            for (byte y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                _tilemap.SetTile(cell._position, GetTile(cell));
            }
        }
    }

    private Tile GetTile(Cell cell)
    {
        if (cell._revealed) return GetRevealedTile(cell);
        else if (cell._flagged) return _flag;
        else if (cell._failed) return _failed;
        else return _unknown;
    }

    private Tile GetRevealedTile(Cell cell)
    {
        switch (cell._type)
        {
            case Cell.Type._empty: return _empty;
            case Cell.Type._number: return GetNumberTile(cell);
            case Cell.Type._mine: return cell._exploded ? _exploded : _mine;
            case Cell.Type._invalid: return null;
            default: return null;
        }
    }

    private Tile GetNumberTile(Cell cell) 
    {
        switch (cell._number)
        {
            case 1: return _num1;
            case 2: return _num2;
            case 3: return _num3;
            case 4: return _num4;
            case 5: return _num5;
            case 6: return _num6;
            case 7: return _num7;
            case 8: return _num8;
            default: return null;
        }
    }
}

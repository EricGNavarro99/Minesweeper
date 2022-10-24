using UnityEngine;

public struct Cell
{
    public enum Type { _invalid, _empty, _number, _mine }

    public Type _type;
    public Vector3Int _position;
    public byte _number;

    public bool _revealed;
    public bool _flagged;
    public bool _exploded;
    public bool _failed;
}

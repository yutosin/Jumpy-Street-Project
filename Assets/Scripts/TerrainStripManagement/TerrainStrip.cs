using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainType
{
    Grass,
    River,
    Road,
    Train
}

public struct Cell
{
    public bool accessible;
    public Vector3 gridPos;
}

[System.Serializable]
public struct TerrainInfo
{
    public TerrainType type;
    public Material mat;
}

/*TerrainStrips are part of the grid system used for movement. Considering a grid, a TerrainStrip is essentially an
  an entire row of a grid. TerrainStrips contain Cells all along the x axis for its width. These Cells are used to 
  assign the player a position when moving and for prop placement.*/

public class TerrainStrip : MonoBehaviour
{
    private List<Cell> _cells;
    private TerrainInfo _terrainInfo;
    private List<GameObject> _props;
    private List<Prop> _testProps;
    private MovableStripManager _movableStripManager;
    private static float _lastLogDirection = -1.0f;
    
    private static int _currentCell = 11;

    private static int CurrentCell
    {
        get { return _currentCell; }
        set { _currentCell =  Mathf.Clamp(value, 6, 13); }
    }

    public int zPosKey;
    public bool IsMovable = false;
    
    public delegate void OnStripInactive(TerrainStrip strip);
    
    public static event OnStripInactive StripInactive;

    public TerrainType Type
    {
        get { return _terrainInfo.type; }
    }

    public void SetupTerrainStrip(TerrainInfo terrainInfo, Prop prop = new Prop(), bool movable = false)
    {
        _cells = new List<Cell>(20); //All strips contain 20 cells corresponding to their width
        _testProps = new List<Prop>(20);
        zPosKey = (int)transform.position.z;
        SetTerrainInfo(terrainInfo);
        _movableStripManager = new MovableStripManager();
        IsMovable = movable;
        if (movable)
        {
            if (Type == TerrainType.River)
            {
                _movableStripManager.SetupManager(Type, zPosKey, _lastLogDirection);
                _lastLogDirection = _lastLogDirection * -1;
            }
            else
                _movableStripManager.SetupManager(Type, zPosKey);
        }

        if (prop.propPrefab != null)
        {
            CreateCells(prop);
            return;
        }
        CreateCells();
    }

    public void ReassignTerrainStrip(TerrainInfo terrainInfo, Prop prop = new Prop(), bool movable = false)
    {
        zPosKey = (int)transform.position.z;
        SetTerrainInfo(terrainInfo);
        IsMovable = movable;
        if (movable)
        {
            if (Type == TerrainType.River)
            {
                _movableStripManager.SetupManager(Type, zPosKey, _lastLogDirection);
                _lastLogDirection = _lastLogDirection * -1;
            }
            else
                _movableStripManager.SetupManager(Type, zPosKey);
        }
        if (prop.propPrefab != null)
            CreateCells(prop);
        else
            CreateCells();
        gameObject.SetActive(true);
    }

    private void SetTerrainInfo(TerrainInfo terrainInfo)
    {
        Renderer rend = gameObject.GetComponent<Renderer>();
        if (rend != null)
        {
            _terrainInfo = terrainInfo;
            rend.material = _terrainInfo.mat;
        }
    }

    private Prop PlaceProp(Prop prop, Vector3 placement)
    {
        Prop cellProp = prop;
        cellProp.gameObject = TerrainStripFactory.GetUsablePropFromPool(prop);
        cellProp.gameObject.transform.position = new Vector3(placement.x, 1 + prop.yOffset, placement.z);
        cellProp.gameObject.SetActive(true);
        return cellProp;
    }

    private void CreateCells(Prop prop = new Prop())
    {
        float xPosBase = -9.5f; //Left most x position on TerrainStrip; offset by .5 to get center
        float xPosIncrement = 1;
        float zPos = transform.position.z;
        for (int i = 0; i < _cells.Capacity; i++)
        {
            // Chance of prop beign spawned in a cell
            int objectChance;
            
            //Also very ugly code; need a better way of setting prop rules for strip
            if (i < 6 || i > 13)
                objectChance = (Type == TerrainType.River) ? 0 : Random.Range(1, 3); //balance this
            else
                objectChance = Random.Range(1, 7);
            
            Cell tempCell = new Cell();
            tempCell.gridPos = new Vector3(xPosBase + (i * xPosIncrement), 0, zPos);
            
            if (!IsMovable && objectChance == 1 && prop.propPrefab != null)
            {
                Prop cellProp = PlaceProp(prop, tempCell.gridPos);
                
                _testProps.Add(cellProp);
                
                tempCell.accessible = cellProp.propAccessibility;
            }
            else if (Type == TerrainType.River)
                tempCell.accessible = false;
            else
                tempCell.accessible = true;

            _cells.Add(tempCell);
        }

        if (!IsMovable && _testProps.Count < 1 && prop.propPrefab != null)
        {
            int randomCellNum = Random.Range(6, 13);
            Cell tempCell = _cells[randomCellNum];

            Prop cellProp = PlaceProp(prop, tempCell.gridPos);
            _testProps.Add(cellProp);
                
            tempCell.accessible = cellProp.propAccessibility;

            _cells[randomCellNum] = tempCell;
        }
    }

    public Cell GetNearestCell(Vector3 point)
    {
        Cell nearestCell = default(Cell);
        for (int i = 0; i < _cells.Count; i++)
        {
            if (point.x > _cells[i].gridPos.x && point.x < _cells[i].gridPos.x + .5f
                || point.x < _cells[i].gridPos.x && point.x > _cells[i].gridPos.x - .5f
                || point.x == _cells[i].gridPos.x)
            {
                nearestCell = _cells[i];
                if (_cells[i].accessible)
                {
                    CurrentCell = i;
                }
            }
        }

        return nearestCell;
    }

    public Cell GetCell(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.UP:
            case MoveDirection.DOWN:
                break;
            case MoveDirection.LEFT:
                --CurrentCell;
                if (!_cells[CurrentCell].accessible)
                    ++CurrentCell;
                break;
            case MoveDirection.RIGHT:
                ++CurrentCell;
                if (!_cells[CurrentCell].accessible)
                    --CurrentCell;
                break;
            default:
                break;
        }
        return _cells[_currentCell];
    }

    private void BoundsCheck()
    {
        Vector3 viewPos = CameraManager.Cam.WorldToViewportPoint(transform.position);
        if (viewPos.x < -0.2 || viewPos.y < -0.2)
        {
            for (int i = 0; i < _testProps.Count; i++)
            {
                _testProps[i].gameObject.SetActive(false);
            }
            gameObject.SetActive(false);
            _testProps.Clear();
            _cells.Clear();
            if (IsMovable)
                _movableStripManager.ReturnToInactive();
            
            if (StripInactive != null)
                StripInactive.Invoke(this);
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < _cells.Count; i++)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_cells[i].gridPos, Vector3.one);
        }
    }

    private void Update()
    {
        BoundsCheck();
    }
}

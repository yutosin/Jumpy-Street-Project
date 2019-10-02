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
    
    private static int _currentCell = 11;

    private static int CurrentCell
    {
        get { return _currentCell; }
        set { _currentCell =  Mathf.Clamp(value, 0, 19); }
    }

    public delegate void OnStripDestroy(TerrainStrip strip);
    
    //Event used by TerrainStripFactory to know when to remove a TerrainStrip from the TerrainStrip master list
    public static event OnStripDestroy StripDestroyed;

    public TerrainType Type
    {
        get { return _terrainInfo.type; }
    }

    public void SetupTerrainStrip(TerrainInfo terrainInfo, Prop prop = new Prop())
    {
        _cells = new List<Cell>(20); //All strips contain 20 cells corresponding to their width
        _props = new List<GameObject>(20); 
        SetTerrainInfo(terrainInfo);
        if (prop.propPrefab != null)
        {
            CreateCells(prop);
            return;
        }
        CreateCells();
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

    private void CreateCells(Prop prop = new Prop())
    {
        float xPosBase = -9.5f; //Left most x position on TerrainStrip; offset by .5 to get center
        float xPosIncrement = 1;
        float zPos = transform.position.z;
        for (int i = 0; i < _cells.Capacity; i++)
        {
            // Chance of prop beign spawned in a cell
            int objectChance = Random.Range(1, 4); //balance this
            
            Cell tempCell = new Cell();
            tempCell.gridPos = new Vector3(xPosBase + (i * xPosIncrement), 0, zPos);
            
            if (objectChance == 1 && prop.propPrefab != null)
            {
                GameObject newProp = Instantiate(prop.propPrefab, 
                    new Vector3(tempCell.gridPos.x, 1, tempCell.gridPos.z), Quaternion.identity);
                _props.Add(newProp);
                tempCell.accessible = prop.propAccessibility;
            }
            else
            {
                tempCell.accessible = true;
            }
            
            _cells.Add(tempCell);
        }
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
            if (StripDestroyed != null)
                StripDestroyed.Invoke(this);
            foreach (var prop in _props)
            {
                Destroy(prop.gameObject);
            }
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        BoundsCheck();
    }
}

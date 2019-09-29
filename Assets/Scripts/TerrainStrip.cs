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

public class TerrainStrip : MonoBehaviour
{
    private List<Cell> _cells;
    private TerrainInfo _terrainInfo;
    
    public TerrainType Type;

    public void SetupTerrainStrip(TerrainInfo terrainInfo, Prop prop)
    {
        _cells = new List<Cell>(20);
        SetTerrainInfo(terrainInfo);
        CreateCells(transform.position.z, prop);
    }

    private void SetTerrainInfo(TerrainInfo terrainInfo)
    {
        Renderer rend = gameObject.GetComponent<Renderer>();
        if (rend != null)
        {
            _terrainInfo = terrainInfo;
            rend.material = _terrainInfo.mat;
            Type = _terrainInfo.type;
        }
    }

    public void CreateCells(float zPos, Prop prop = new Prop())
    {
        float xBase = -9.5f;
        float xInc = 1;
        for (int i = 0; i < _cells.Capacity; i++)
        {
            int objectChance = Random.Range(1, 4);
            
            Cell tempCell = new Cell();
            tempCell.gridPos = new Vector3(xBase + (i * xInc), 0, zPos);
            
            if (objectChance == 1 && prop.propPrefab != null)
            {
                Instantiate(prop.propPrefab, new Vector3(tempCell.gridPos.x, 1, tempCell.gridPos.z), Quaternion.identity);
                tempCell.accessible = prop.propAccessibility;
            }
            
            _cells.Add(tempCell);
        }
    }
}

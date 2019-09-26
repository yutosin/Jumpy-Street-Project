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

public class TerrainStrip : MonoBehaviour
{
    private List<Cell> _cells;

    public Material TerrainMat;
    public TerrainType Type;
    
    // Start is called before the first frame update
    void Start()
    {
        _cells = new List<Cell>(20);
    }

    public void CreateCells(float zPos, Prop prop = new Prop())
    {
        float xBase = -9.5f;
        float xInc = 0.5f;
        for (int i = 0; i < _cells.Count; i++)
        {
            int objectChance = Random.Range(1, 4);
            
            Cell tempCell = new Cell();
            tempCell.gridPos = new Vector3(xBase + (i * xInc), 0, zPos);
            
            if (objectChance == 1 && prop.propPrefab != null)
            {
                Instantiate(prop.propPrefab, tempCell.gridPos, Quaternion.identity);
                tempCell.accessible = prop.propAccessibility;
            }
            
            _cells[i] = tempCell;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Prop
{
    public GameObject propPrefab;
    public bool propAccessibility;
    public TerrainType propTerrain;
}

public class TerrainStripFactory : MonoBehaviour
{
    private List<TerrainStrip> _strips;
    private int _currentStrip;

    public List<TerrainInfo> TerrainInfos;
    public List<Prop> TerrainProps;
    public GameObject TerrainStripPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        _strips = new List<TerrainStrip>(10);
        for (int i = 0; i < _strips.Capacity; i++)
        {
            GameObject tempStrip = Instantiate(TerrainStripPrefab, new Vector3(0, 0, i), Quaternion.identity);
            _strips.Add(tempStrip.GetComponent<TerrainStrip>());
            int randMat = Random.Range(0, TerrainInfos.Count);
            int randProp = Random.Range(0, TerrainProps.Count);
            
            _strips[i].SetupTerrainStrip(TerrainInfos[randMat], TerrainProps[randProp]);
        }

        _currentStrip = 0;
    }

    public Vector3 GetNextPosition(MoveDirection direction)
    {
        Cell temp;
        switch (direction)
        {
            //Clean this up, too much repeated code; might wanna redo accesible check logic
            case MoveDirection.UP:
                _currentStrip++;
                temp = _strips[_currentStrip].GetCell(direction);
                if (!temp.accessible && (_currentStrip - 1) > 0)
                {
                    temp = _strips[_currentStrip - 1].GetCell(direction);
                    _currentStrip--;
                }

                return temp.gridPos;
            case MoveDirection.DOWN:
                _currentStrip--;
                temp = _strips[_currentStrip].GetCell(direction);
                if (!temp.accessible && (_currentStrip + 1) > 0)
                {
                    temp = _strips[_currentStrip + 1].GetCell(direction);
                    _currentStrip++;
                }

                return temp.gridPos;
            case MoveDirection.LEFT:
            case MoveDirection.RIGHT:
                temp = _strips[_currentStrip].GetCell(direction);
                return temp.gridPos;
            default:
                break;
        }
        return Vector3.zero;
    }
    
    /*TO DO:
        -continuous strip generation
        -create strip managers?
        -set current strip?
        -will terrain strip factory essentially be terrain strip manager? or should i separate creation and logic?
        -way to get next cell (probably on terrain strip itself)?
        -weighted random; heavier on road and river, lighter on grass, lightest on train
     */
}

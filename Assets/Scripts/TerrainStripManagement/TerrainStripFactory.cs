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
    private int _currentStrip; //do not access directly!! use currentStrip
    private int _lastStripPos = -5;
    
    private int currentStrip
    {
        get { return this._currentStrip; }
        set { this._currentStrip = Mathf.Clamp(value, 0, _strips.Count - 1); }
    }
    
    public List<TerrainInfo> TerrainInfos;
    public List<Prop> TerrainProps;
    public GameObject TerrainStripPrefab;
    public int NumStrips;

    public static float GenSpeed;
    
    // Start is called before the first frame update
    void Start()
    {
        _strips = new List<TerrainStrip>(NumStrips);
        for (int i = 0; i < _strips.Capacity; i++)
        {
//            GameObject tempStrip = Instantiate(TerrainStripPrefab, new Vector3(0, 0, i - 5), Quaternion.identity);
//            _strips.Add(tempStrip.GetComponent<TerrainStrip>());
//            int randMat = Random.Range(0, TerrainInfos.Count);
//            int randProp = Random.Range(0, TerrainProps.Count);
//
//            if (i < 6)
//            {
//                _strips[i].SetupTerrainStrip(TerrainInfos[0]);
//                continue;
//            }
//
//            _strips[i].SetupTerrainStrip(TerrainInfos[randMat], TerrainProps[randProp]);
            AddNewStrip();
        }

        currentStrip = 5;
        TerrainStrip.StripDestroyed += OnStripDestroy;
        GenSpeed = 1;
        StartCoroutine(StripGenEnumerator());
    }

    void AddNewStrip()
    {
        GameObject tempStrip = Instantiate(TerrainStripPrefab, new Vector3(0, 0, _lastStripPos++), Quaternion.identity);
        _strips.Add(tempStrip.GetComponent<TerrainStrip>());
        int randMat = Random.Range(0, TerrainInfos.Count);
        int randProp = Random.Range(0, TerrainProps.Count);
        
        if (_strips.Count < 6)
        {
            _strips[_strips.Count - 1].SetupTerrainStrip(TerrainInfos[0]);
            return;
        }
        
        _strips[_strips.Count - 1].SetupTerrainStrip(TerrainInfos[randMat], TerrainProps[randProp]);
    }

    IEnumerator StripGenEnumerator()
    {
        while (true)
        {
            AddNewStrip();
            
//            Destroy(_strips[0].gameObject);
//            _strips.RemoveAt(0);
//
//            currentStrip = --currentStrip;
            
            yield return new WaitForSeconds(GenSpeed);
        }
    }

    private void OnStripDestroy(TerrainStrip strip)
    {
        _strips.Remove(strip);

        currentStrip = --currentStrip;
    }

    public Vector3 GetNextPosition(MoveDirection direction)
    {
        Cell temp;
        switch (direction)
        {
            //Clean this up, still a bit of repeated code
            case MoveDirection.UP:
                ++currentStrip;
                temp = _strips[currentStrip].GetCell(direction);
                if (!temp.accessible)
                {
                    --currentStrip;
                    temp = _strips[currentStrip].GetCell(direction);
                }

                break;
            case MoveDirection.DOWN:
                --currentStrip;
                temp = _strips[currentStrip].GetCell(direction);
                if (!temp.accessible)
                {
                    ++currentStrip;
                    temp = _strips[currentStrip].GetCell(direction);
                }

                break;
            default:
                temp = _strips[currentStrip].GetCell(direction);
                break;
        }
        Debug.Log(temp.gridPos);
        return temp.gridPos;
    }

    /*TO DO:
        -will terrain strip factory essentially be terrain strip manager? or should i separate creation and logic?
        -continuous strip generation
        -weighted random; heavier on road and river, lighter on grass, lightest on train
        -create strip managers?
     */
}

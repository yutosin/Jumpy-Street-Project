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

/*TerrainStripFactory is the home of TerrainStrip creation. It determines the TerrainType and what props appear on each
  individual TerrainStrip. It also assigns TerrainStripManagers. It also provides the actual Vector3 position based off
  the cells in our grid.*/

public class TerrainStripFactory : MonoBehaviour
{
    private List<TerrainStrip> _strips;
    private int _currentStrip; //do not access directly!! use currentStrip
    private int _lastStripPos = -5;
    
    private int currentStrip
    {
        get { return _currentStrip; }
        set { _currentStrip = Mathf.Clamp(value, 0, _strips.Count - 1); }
    }
    
    //Set in editor
    public List<TerrainInfo> TerrainInfos; //Provides the TerrainTypes and associated materials that we use for TerrainStrips
    public List<Prop> TerrainProps; //The prope that can be placed in TerrainStrips
    public GameObject TerrainStripPrefab;
    public int NumStrips;
    
    [HideInInspector]
    public static float GenSpeed;
    
    // Start is called before the first frame update
    void Start()
    {
        _strips = new List<TerrainStrip>(NumStrips);
        for (int i = 0; i < _strips.Capacity; i++)
        {
            AddNewStrip();
        }

        currentStrip = 5;
        TerrainStrip.StripDestroyed += OnStripDestroy;
        //Play around with value; still some situations where we run out of terrain
        GenSpeed = 1;
        StartCoroutine(StripGenEnumerator());
    }

    void AddNewStrip()
    {
        GameObject tempStrip = Instantiate(TerrainStripPrefab, new Vector3(0, 0, _lastStripPos++), Quaternion.identity);
        _strips.Add(tempStrip.GetComponent<TerrainStrip>());
        int randMat = Random.Range(0, TerrainInfos.Count);
        int randProp = Random.Range(0, TerrainProps.Count);
        
        //Want the first 5 strips to be all grass strips
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
        switch (direction)
        {
            case MoveDirection.UP:
                ++currentStrip;
                bool accessible = _strips[currentStrip].GetCell(direction).accessible;
                if (!accessible)
                    --currentStrip;

                break;
            case MoveDirection.DOWN:
                --currentStrip;
                accessible = _strips[currentStrip].GetCell(direction).accessible;
                if (!accessible)
                    ++currentStrip;

                break;
            default:
                break;
        }
        Cell temp = _strips[currentStrip].GetCell(direction);
        return temp.gridPos;
    }

    /*TO DO:
        -will terrain strip factory essentially be terrain strip manager? or should i separate creation and logic?
        -continuous strip generation
        -weighted random; heavier on road and river, lighter on grass, lightest on train
        -create strip managers?
     */
}

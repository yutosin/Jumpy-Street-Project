using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Prop
{
    public GameObject propPrefab;
    public GameObject gameObject;
    public bool propAccessibility;
    public TerrainType propTerrain;
}

/*TerrainStripFactory is the home of TerrainStrip creation. It determines the TerrainType and what props appear on each
  individual TerrainStrip. It also assigns TerrainStripManagers. It also provides the actual Vector3 position based off
  the cells in our grid.*/

public class TerrainStripFactory : MonoBehaviour
{
    private Dictionary<int, TerrainStrip> _stripPool;
    private Stack<int> _unusedStrips;
    private static Dictionary<TerrainType, List<GameObject>> _poolDictionary;
    private int _currentStrip; //do not access directly!! use currentStrip
    private int _lastStripPos = -5;
    
    private int currentStrip
    {
        get { return _currentStrip; }
        set { _currentStrip = Mathf.Clamp(value, 0,  _lastStripPos - 1); }
    }
    
    //Set in editor
    public List<TerrainInfo> TerrainInfos; //Provides the TerrainTypes and associated materials that we use for TerrainStrips
    public List<Prop> TerrainProps; //The prop that can be placed in TerrainStrips
    public GameObject TerrainStripPrefab;
    public int NumStrips;
    
    // Start is called before the first frame update
    void Start()
    { 
        _poolDictionary = new Dictionary<TerrainType, List<GameObject>>();
        CreatePropPools();
        
        _stripPool = new Dictionary<int, TerrainStrip>(NumStrips);
        for (int i = 0; i < NumStrips; i++)
        {
            AddNewStrip();
        }
        
        _unusedStrips = new Stack<int>();

        currentStrip = 0;
        TerrainStrip.StripInactive += OnStripInactive;
    }

    private void CreatePropPools()
    {
        foreach (var prop in TerrainProps)
        {
            List<GameObject> newPropPool = new List<GameObject>(20);
            for (int i = 0; i < newPropPool.Capacity; i++)
            {
                GameObject propRef = Instantiate(prop.propPrefab, Vector3.zero, Quaternion.identity);
                propRef.SetActive(false);
                newPropPool.Add(propRef);
            }
            _poolDictionary.Add(prop.propTerrain, newPropPool);
        }
    }
    
    void AddNewStrip()
    {
        GameObject tempStrip =
            Instantiate(TerrainStripPrefab, new Vector3(0, 0, _lastStripPos), Quaternion.identity);
        _stripPool.Add(_lastStripPos, tempStrip.GetComponent<TerrainStrip>());

        //Could try a weighted distribution for TerrainType but...may need to be context sensitive?
        //Possibly get last strip and have a certain chance of repeating last strip type depending on strip?
//            int randMat = 0;
//            float randValue = Random.value;
//            Debug.Log(randValue);
//            if (_strips.Count > 1)
//            {
//                TerrainType lasTerrainType = _strips[_strips.Count - 1].Type;
//                switch (lasTerrainType)
//                {
//                    case TerrainType.Grass:
//                        if (randValue <= .2f)
//                            randMat = 0;
//                        else
//                            randMat = Random.Range(0, TerrainInfos.Count);
//                        break;
//                    case TerrainType.River:
//                        if (randValue <= .2f)
//                            randMat = 1;
//                        else
//                            randMat = Random.Range(0, TerrainInfos.Count);
//                        break;
//                    case TerrainType.Road:
//                        if (randValue <= .9f)
//                            randMat = 2;
//                        else
//                            randMat = Random.Range(0, TerrainInfos.Count);
//                        break;
//                    case TerrainType.Train:
//                        if (randValue <= .2f)
//                            randMat = 3;
//                        else
//                            randMat = Random.Range(0, TerrainInfos.Count);
//                        break;
//                }
//            }
//            else
//                randMat = Random.Range(0, TerrainInfos.Count);

        int randMat = Random.Range(0, TerrainInfos.Count);
        int randProp = Random.Range(0, TerrainProps.Count);

        //Want the first 5 strips to be all grass strips
        if (_stripPool.Count < 6)
        {
            _stripPool[_lastStripPos].SetupTerrainStrip(TerrainInfos[0]);
            _lastStripPos++;
            return;
        }

        _stripPool[_lastStripPos].SetupTerrainStrip(TerrainInfos[randMat], TerrainProps[randProp]);
        _lastStripPos++;
    }
    
    private void OnStripInactive(TerrainStrip strip)
    {
        _unusedStrips.Push(strip.zPosKey);
        AddTerrainStrip();
    }

    public Vector3 GetNextPosition(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.UP:
                ++currentStrip;
                bool accessible = _stripPool[currentStrip].GetCell(direction).accessible;
                if (!accessible)
                    --currentStrip;

                break;
            case MoveDirection.DOWN:
                --currentStrip;
                accessible = _stripPool[currentStrip].GetCell(direction).accessible;
                if (!accessible)
                    ++currentStrip;

                break;
            default:
                break;
        }
        Cell temp = _stripPool[currentStrip].GetCell(direction);
        return temp.gridPos;
    }

    private void AddTerrainStrip()
    {
        if (_unusedStrips.Count > 0)
        {
            int stripKey = _unusedStrips.Pop();
            TerrainStrip unusedStrip = _stripPool[stripKey];
            unusedStrip.gameObject.transform.position = new Vector3(0, 0, _lastStripPos);

            int randMat = Random.Range(0, TerrainInfos.Count);
            int randProp = Random.Range(0, TerrainProps.Count);

            unusedStrip.ReassignTerrainStrip(TerrainInfos[randMat], TerrainProps[randProp]);

            _stripPool.Add(_lastStripPos, unusedStrip);
            _stripPool.Remove(stripKey);
            _lastStripPos++;
        }
        else
        {
            AddNewStrip();
        }
    }

    public static GameObject GetUsablePropFromPool(Prop prop)
    {
        List<GameObject> propPool = _poolDictionary[prop.propTerrain];
        for (int i = 0; i < propPool.Count; i++)
        {
            if (!propPool[i].activeInHierarchy)
            {
                return propPool[i];
            }
        }
        GameObject newPropRef = Instantiate(prop.propPrefab, Vector3.zero, Quaternion.identity);
        
        propPool.Add(newPropRef);

        return newPropRef;
    }

    /*TO DO:
        -will terrain strip factory essentially be terrain strip manager? or should i separate creation and logic?
        -continuous strip generation
        -weighted random; heavier on road and river, lighter on grass, lightest on train
        -create strip managers?
     */
}

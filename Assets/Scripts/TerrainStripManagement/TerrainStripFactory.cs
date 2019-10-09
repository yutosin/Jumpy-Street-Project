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
    public float yOffset;
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
    private int _sameStripCount = 0;
    
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
                GameObject propRef = Instantiate(prop.propPrefab, Vector3.zero, prop.propPrefab.transform.rotation);
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

        //In the future we not doing this randomly or at all; the TS will just grab the prop based on terrain type
        int randProp = Random.Range(0, TerrainProps.Count);

        //Want the first 5 strips to be all grass strips
        if (_stripPool.Count < 6)
        {
            _stripPool[_lastStripPos].SetupTerrainStrip(TerrainInfos[0], TerrainProps[0]);
            _lastStripPos++;
            return;
        }
        
        TerrainInfo newTerrainInfo = CreateWeightedTerrainInfo();
        if (newTerrainInfo.type == TerrainType.Grass)
            _stripPool[_lastStripPos].SetupTerrainStrip(newTerrainInfo, TerrainProps[0]);
        else if (newTerrainInfo.type == TerrainType.River)
            _stripPool[_lastStripPos].SetupTerrainStrip(newTerrainInfo, TerrainProps[1]);
        else
            _stripPool[_lastStripPos].SetupTerrainStrip(newTerrainInfo);
        _lastStripPos++;
    }
    
    private void AddTerrainStrip()
    {
        if (_unusedStrips.Count > 0)
        {
            int stripKey = _unusedStrips.Pop();
            TerrainStrip unusedStrip = _stripPool[stripKey];
            unusedStrip.gameObject.transform.position = new Vector3(0, 0, _lastStripPos);

            //In the future we not doing this randomly or at all; the TS will just grab the prop based on terrain type
            int randProp = Random.Range(0, TerrainProps.Count);
            int riverPropChance = Random.Range(1, 5);

            TerrainInfo newTerrainInfo = CreateWeightedTerrainInfo();
            //Ugly ugly code; need a better way of defining rules for terrain strips
            if (newTerrainInfo.type == TerrainType.Grass)
                unusedStrip.ReassignTerrainStrip(newTerrainInfo, TerrainProps[0]);
            else if (newTerrainInfo.type == TerrainType.River && riverPropChance == 1)
                unusedStrip.ReassignTerrainStrip(newTerrainInfo, TerrainProps[1]);
            else
                unusedStrip.ReassignTerrainStrip(newTerrainInfo);

            _stripPool.Add(_lastStripPos, unusedStrip);
            _stripPool.Remove(stripKey);
            _lastStripPos++;
        }
        else
        {
            AddNewStrip();
        }
    }

    private TerrainInfo CreateWeightedTerrainInfo()
    {
        //Could try a weighted distribution for TerrainType but...may need to be context sensitive?
        //Possibly get last strip and have a certain chance of repeating last strip type depending on strip?
        int randTerrain = 0;
        float randValue = Random.value;
        if (_stripPool.Count > 1)
        {
            TerrainType lasTerrainType = _stripPool[_lastStripPos - 1].Type;
            switch (lasTerrainType)
            {
                case TerrainType.Grass:
                    if (randValue <= .2f && _sameStripCount < 3)
                    {
                        randTerrain = 0;
                        _sameStripCount++;
                    }
                    else
                    {
                        //randTerrain = Random.Range(1, TerrainInfos.Count);
                        randTerrain = RandomRangeExcept(0, TerrainInfos.Count, 0);
                        _sameStripCount = 0;
                    }

                    break;
                case TerrainType.River:
                    if (randValue <= .5f && _sameStripCount < 5)
                    {
                        randTerrain = 1;
                        _sameStripCount++;
                    }
                    else
                    {
                        randTerrain = RandomRangeExcept(0, TerrainInfos.Count, 1);
                        _sameStripCount = 0;
                    }

                    break;
                case TerrainType.Road:
                    if (randValue <= .7f && _sameStripCount < 5)
                    {
                        randTerrain = 2;
                        _sameStripCount++;
                    }
                    else
                    {
                        randTerrain = RandomRangeExcept(0, TerrainInfos.Count, 2);
                        _sameStripCount = 0;
                    }

                    break;
                case TerrainType.Train:
                    if (randValue <= .1f && _sameStripCount < 3)
                    {
                        randTerrain = 3;
                        _sameStripCount++;
                    }
                    else
                    {
                        randTerrain = RandomRangeExcept(0, TerrainInfos.Count, 3);
                        _sameStripCount = 0;
                    }

                    break;
            }
        }
        else
            randTerrain = Random.Range(0, TerrainInfos.Count);

        return TerrainInfos[randTerrain];
    }

    private int RandomRangeExcept(int min, int max, int except)
    {
        int randomNum;
        do
        {
            randomNum = Random.Range(min, max);
        } while (randomNum == except);

        return randomNum;
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
    
    //Modify this to grab prop based on terrain type; will need to adjust instantiation code too
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
        GameObject newPropRef = Instantiate(prop.propPrefab, Vector3.zero, prop.propPrefab.transform.rotation);
        
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

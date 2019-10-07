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
    public bool available;
}

/*TerrainStripFactory is the home of TerrainStrip creation. It determines the TerrainType and what props appear on each
  individual TerrainStrip. It also assigns TerrainStripManagers. It also provides the actual Vector3 position based off
  the cells in our grid.*/

public class TerrainStripFactory : MonoBehaviour
{
    //22 strips visible to camera at any given time
    //private List<TerrainStrip> _strips; //object pool these strips with a dictionary; zpos as keys
    private Dictionary<int, TerrainStrip> _stripPool;
    private Stack<int> _unusedStrips;
    private int _currentStrip; //do not access directly!! use currentStrip
    private int _lastStripPos = -5;
    
    private int currentStrip
    {
        get { return _currentStrip; }
        set { _currentStrip = Mathf.Clamp(value, 0,  _lastStripPos); }
    }
    
    //Set in editor
    public List<TerrainInfo> TerrainInfos; //Provides the TerrainTypes and associated materials that we use for TerrainStrips
    public List<Prop> TerrainProps; //The prop that can be placed in TerrainStrips
    public GameObject TerrainStripPrefab;
    public int NumStrips;
    
    public static Dictionary<TerrainType, List<Prop>> PoolDictionary;
    
    [HideInInspector]
    public static float GenSpeed;
    
    // Start is called before the first frame update
    void Start()
    {
//        _strips = new List<TerrainStrip>(NumStrips);
//        for (int i = 0; i < NumStrips; i++)
//        {
//            AddNewStrip();
//        }
//        
        PoolDictionary = new Dictionary<TerrainType, List<Prop>>();
        CreatePropPools();
        
        _stripPool = new Dictionary<int, TerrainStrip>(NumStrips);
        for (int i = 0; i < NumStrips; i++)
        {
            AddNewStrip();
        }
        
        _unusedStrips = new Stack<int>();

        currentStrip = 0;
        TerrainStrip.StripInactive += OnStripInactive;
        //Play around with value; still some situations where we run out of terrain
        GenSpeed = .4f;
//        StartCoroutine(StripGenEnumerator());
    }
    
    /// <summary>
    /// Wont actuall need this; what really needs to happen is that AddNewStrip needs to be modified to be to add the strips
    /// to the new dictionary. then, instead of calling add strip and setup terrain strip, we instead have a call to reassign
    /// terrain strip in response to on strip destroy (now on strip unused or something). reassign terrain strip will have
    /// similar behavior to setup terrain strip since it needs terrain info and prop info
    /// </summary>

    private void CreatePropPools()
    {
        foreach (var prop in TerrainProps)
        {
            List<Prop> newPropPool = new List<Prop>(20);
            for (int i = 0; i < newPropPool.Capacity; i++)
            {
                Prop tempProp = prop;
                tempProp.gameObject = Instantiate(prop.propPrefab, Vector3.zero, Quaternion.identity);
                tempProp.gameObject.SetActive(false);
                tempProp.available = true;
                newPropPool.Add(tempProp);
            }
            PoolDictionary.Add(prop.propTerrain, newPropPool);
        }
    }
    
    //Gonna have to redesign with an object pool!! Too taxing on the CPU
    
    //Another idea: only add a strip every time player moves? if we have a number of strips outside the screen at all times
    //this could be a valid approach
    void AddNewStrip()
    {
        GameObject tempStrip =
            Instantiate(TerrainStripPrefab, new Vector3(0, 0, _lastStripPos), Quaternion.identity);
//        _strips.Add(tempStrip.GetComponent<TerrainStrip>());
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

//    IEnumerator StripGenEnumerator()
//    {
//        while (true)
//        {
//            AddTerrainStrip();
//            
//            yield return new WaitForSecondsRealtime(GenSpeed);
//        }
//    }
//    
    private void OnStripInactive(TerrainStrip strip, Prop[] propsForList)
    {
        _unusedStrips.Push(strip.zPosKey);
        if (propsForList.Length == 0)
            return;
        List<Prop> propPool = PoolDictionary[propsForList[0].propTerrain];
        for (int i = 0; i < propsForList.Length; i++)
        {
            propPool.Add(propsForList[i]);
        }
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
            //gotta remove and readd to dictionary as well as reassign info

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

    public static Prop GetUsablePropFromPool(Prop prop)
    {
        List<Prop> propPool = PoolDictionary[prop.propTerrain];
        for (int i = 0; i < propPool.Count; i++)
        {
            if (propPool[i].available)
            {
                var tempCell = propPool[i];
                propPool.Remove(propPool[i]);
                return tempCell;
            }
        }
        Prop newProp = new Prop();
        newProp.propPrefab = prop.propPrefab;
        newProp.propAccessibility = prop.propAccessibility;
        newProp.propTerrain = prop.propTerrain;
        newProp.available = false;

        newProp.gameObject = Instantiate(prop.propPrefab, Vector3.zero, Quaternion.identity);
        
        //propPool.Add(newProp);

        return newProp;
    }

    /*TO DO:
        -will terrain strip factory essentially be terrain strip manager? or should i separate creation and logic?
        -continuous strip generation
        -weighted random; heavier on road and river, lighter on grass, lightest on train
        -create strip managers?
     */
}

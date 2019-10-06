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
    private List<TerrainStrip> _strips; //object pool these strips with a dictionary; zpos as keys
    private Dictionary<int, TerrainStrip> _stripPool;
    private Stack<int> _unusedStrips;
    private int _currentStrip; //do not access directly!! use currentStrip
    private int _lastStripPos = -5;
    
    private int currentStrip
    {
        get { return _currentStrip; }
        set { _currentStrip = Mathf.Clamp(value, 0, _strips.Count - 1); }
    }
    
    //Set in editor
    public List<TerrainInfo> TerrainInfos; //Provides the TerrainTypes and associated materials that we use for TerrainStrips
    public List<Prop> TerrainProps; //The prop that can be placed in TerrainStrips
    public GameObject TerrainStripPrefab;
    public int NumStrips;
    
    public static Dictionary<Prop, List<Prop>> PoolDictionary;
    
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
        TerrainStrip.StripInactive += OnStripInactive;
        //Play around with value; still some situations where we run out of terrain
        GenSpeed = 1f;
        StartCoroutine(StripGenEnumerator());
    }
    
    /// <summary>
    /// Wont actuall need this; what really needs to happen is that AddNewStrip needs to be modified to be to add the strips
    /// to the new dictionary. then, instead of calling add strip and setup terrain strip, we instead have a call to reassign
    /// terrain strip in response to on strip destroy (now on strip unused or something). reassign terrain strip will have
    /// similar behavior to setup terrain strip since it needs terrain info and prop info
    /// </summary>
    private void CreateStripPool()
    {
        _strips = new List<TerrainStrip>(NumStrips);
        for (int i = 0; i < _strips.Capacity; i++)
        {
            AddNewStrip();
        }
    }

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
                newPropPool.Add(tempProp);
            }
            PoolDictionary.Add(prop, newPropPool);
        }
    }
    
    //Gonna have to redesign with an object pool!! Too taxing on the CPU
    
    //Another idea: only add a strip every time player moves? if we have a number of strips outside the screen at all times
    //this could be a valid approach
    void AddNewStrip()
    {
        GameObject tempStrip =
            Instantiate(TerrainStripPrefab, new Vector3(0, 0, _lastStripPos++), Quaternion.identity);
        _strips.Add(tempStrip.GetComponent<TerrainStrip>());

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
    
    private void OnStripInactive(TerrainStrip strip)
    {
        _unusedStrips.Push(strip.zPosKey);
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

    private void AddTerrainStrip()
    {
        if (_unusedStrips.Count == 0)
        {
            AddNewStrip();
        }

        int stripKey = _unusedStrips.Pop();
        TerrainStrip unusedStrip = _stripPool[stripKey];
        unusedStrip.transform.position = new Vector3(0, 0, _lastStripPos++);
        
        int randMat = Random.Range(0, TerrainInfos.Count);
        int randProp = Random.Range(0, TerrainProps.Count);
        //gotta remove and readd to dictionary as well as reassign info
        
        unusedStrip.ReassignTerrainStrip(TerrainInfos[randMat], TerrainProps[randProp]);

        _stripPool.Add(_lastStripPos, unusedStrip);
        _stripPool.Remove(stripKey);
    }

    public static Prop GetUsablePropFromPool(Prop prop)
    {
        List<Prop> propPool = PoolDictionary[prop];
        for (int i = 0; i < propPool.Count; i++)
        {
            if (propPool[i].available)
                return propPool[i];
        }
        Prop newProp = new Prop();
        newProp.propPrefab = prop.propPrefab;
        newProp.propAccessibility = prop.propAccessibility;
        newProp.propTerrain = prop.propTerrain;
        newProp.available = false;

        newProp.gameObject = Instantiate(prop.propPrefab, Vector3.zero, Quaternion.identity);
        
        propPool.Add(newProp);

        return newProp;
    }

    /*TO DO:
        -will terrain strip factory essentially be terrain strip manager? or should i separate creation and logic?
        -continuous strip generation
        -weighted random; heavier on road and river, lighter on grass, lightest on train
        -create strip managers?
     */
}

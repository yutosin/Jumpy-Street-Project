using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private static TerrainStripFactory _sharedInstance;
    public static TerrainStripFactory SharedInstance
    {
        get { return _sharedInstance; }
    }
    
    
    private Dictionary<int, TerrainStrip> _stripPool;
    private Stack<int> _unusedStrips;
    private static Dictionary<TerrainType, List<GameObject>> _poolDictionary;
    private int _currentStrip; //do not access directly!! use currentStrip
    private int _lastStripPos;
    private int _sameStripCount;
    
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
    public List<MovableProp> movablePropPefabs;

    private void Awake()
    {
        if (_sharedInstance != null)
        {
            Destroy(gameObject);
            return;
        }
		
        _sharedInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    { 
        _poolDictionary = new Dictionary<TerrainType, List<GameObject>>();
        _lastStripPos = -5;
        _sameStripCount = 0;
        
        CreatePropPools();
        MovableStripManager.CreateManagerPools();
        
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
        if (_stripPool.Count < 10)
        {
            _stripPool[_lastStripPos].SetupTerrainStrip(TerrainInfos[0], TerrainProps[0]);
            _lastStripPos++;
            return;
        }
        
        TerrainInfo newTerrainInfo = CreateWeightedTerrainInfo();
        if (newTerrainInfo.type == TerrainType.Grass)
            _stripPool[_lastStripPos].SetupTerrainStrip(newTerrainInfo, TerrainProps[0]);
        else if (newTerrainInfo.type == TerrainType.River)
        {
            float randValue = Random.value;
            if (randValue <= .75f)
                _stripPool[_lastStripPos].SetupTerrainStrip(newTerrainInfo, TerrainProps[1], true);
            else if (_stripPool[_lastStripPos - 1].Type != TerrainType.River
                || (_stripPool[_lastStripPos - 1].Type == TerrainType.River && _stripPool[_lastStripPos - 1].IsMovable))
                _stripPool[_lastStripPos].SetupTerrainStrip(newTerrainInfo, TerrainProps[1]);
            else
                _stripPool[_lastStripPos].SetupTerrainStrip(newTerrainInfo, TerrainProps[1], true);
        }
        else if (newTerrainInfo.type == TerrainType.Road)
        {
            _stripPool[_lastStripPos].SetupTerrainStrip(newTerrainInfo, default(Prop), true);
        }
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
            //int riverPropChance = Random.Range(1, 5);

            TerrainInfo newTerrainInfo = CreateWeightedTerrainInfo();
            //Ugly ugly code; need a better way of defining rules for terrain strips
            if (newTerrainInfo.type == TerrainType.Grass)
                unusedStrip.ReassignTerrainStrip(newTerrainInfo, TerrainProps[0]);
            else if (newTerrainInfo.type == TerrainType.River)
            {
                float randValue = Random.value;
                if (randValue <= .75f)
                    unusedStrip.ReassignTerrainStrip(newTerrainInfo, TerrainProps[1], true);
                else if (_stripPool[_lastStripPos - 1].Type != TerrainType.River
                         || (_stripPool[_lastStripPos - 1].Type == TerrainType.River && _stripPool[_lastStripPos - 1].IsMovable))
                    unusedStrip.ReassignTerrainStrip(newTerrainInfo, TerrainProps[1]);
                else
                    unusedStrip.ReassignTerrainStrip(newTerrainInfo, TerrainProps[1], true);
            }
            else if (newTerrainInfo.type == TerrainType.Road)
            {
                unusedStrip.ReassignTerrainStrip(newTerrainInfo, default(Prop), true);
            }
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

    public static int RandomRangeExcept(int min, int max, int except)
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
    
    private Vector3 GetNextRiverPosition(MoveDirection direction)
    {
        //Modify checkpoint to consider the movement direction
        PlayerMovement.isInRiver = true;
        Transform playerTransform = PlayerMovement.playerObject.transform;
        float horizontalDistanceCheck = 0f;
        horizontalDistanceCheck = (direction == MoveDirection.LEFT) ? -1.0f : horizontalDistanceCheck;
        horizontalDistanceCheck = (direction == MoveDirection.RIGHT) ? 1.0f : horizontalDistanceCheck;
        Vector3 checkPoint = new Vector3(playerTransform.position.x + horizontalDistanceCheck, 
            playerTransform.position.y,
            currentStrip);
        Cell nextCell = _stripPool[currentStrip].GetNearestCell(checkPoint);
        if (_stripPool[currentStrip].Type != TerrainType.River && nextCell.accessible)
        {
            PlayerMovement.isInRiver = false;
            playerTransform.parent = null;
            return nextCell.gridPos;
        }
        
        if (_stripPool[currentStrip].Type != TerrainType.River && !nextCell.accessible)
        {
            if (direction == MoveDirection.UP)
                --currentStrip;
            else
                ++currentStrip;
            return playerTransform.position;
        }

        if (nextCell.accessible)
        {
            playerTransform.parent = null;
            return nextCell.gridPos;
        }   

        LayerMask mask = LayerMask.GetMask("Log");
        Vector3 overlapCube = new Vector3(.5f, .75f, .75f);
        Collider[] colliders = Physics.OverlapBox(checkPoint, overlapCube, Quaternion.identity, mask);
        if (colliders.Length == 0)
        {
            Destroy(PlayerMovement.playerObject);
            return nextCell.gridPos;
        }

        Vector3 logPos = Vector3.zero;
        for (int i = 0; i < colliders.Length; i++)
        {
            if ((int)colliders[i].transform.position.z == currentStrip)
            {
                logPos = colliders[i].transform.position;
                PlayerMovement.playerObject.transform.parent = colliders[i].transform;
                break;
            }
        }

        if (logPos == Vector3.zero)
        {
            Destroy(PlayerMovement.playerObject);
            return nextCell.gridPos;
        }


        //Have to clean this up; something is wrong with my positioning logic, should be able to just use playerPos
        if (direction == MoveDirection.LEFT && playerTransform.position.x > logPos.x)
            return new Vector3(logPos.x - .5f, 1.5f, logPos.z);
        if (direction == MoveDirection.RIGHT && playerTransform.position.x < logPos.x)
            return new Vector3(logPos.x + .5f, 1.5f, logPos.z);
        if (logPos.x > nextCell.gridPos.x)
            return new Vector3(logPos.x - .5f, 1.5f, logPos.z);
        return new Vector3(logPos.x + .5f, 1.5f, logPos.z);
    }

    public Vector3 GetNextPosition(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.UP:
                ++currentStrip;
                if (_stripPool[currentStrip].Type == TerrainType.River || PlayerMovement.isInRiver)
                    return GetNextRiverPosition(direction);
                Cell nextCell = _stripPool[currentStrip].GetCell(direction);
                bool accessible = nextCell.accessible;
                if (!accessible)
                    --currentStrip;

                break;
            case MoveDirection.DOWN:
                --currentStrip;
                if (_stripPool[currentStrip].Type == TerrainType.River || PlayerMovement.isInRiver)
                    return GetNextRiverPosition(direction);
                nextCell = _stripPool[currentStrip].GetCell(direction);
                accessible = nextCell.accessible;
                if (!accessible)
                    ++currentStrip;

                break;
            default:
                break;
        }
        if (_stripPool[currentStrip].Type == TerrainType.River || PlayerMovement.isInRiver)
            return GetNextRiverPosition(direction);
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

    private void OnDestroy()
    {
        TerrainStrip.StripInactive -= OnStripInactive;
    }
}

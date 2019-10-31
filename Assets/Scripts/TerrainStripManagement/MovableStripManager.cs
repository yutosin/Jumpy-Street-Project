using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovablePropType
{
    TRUCK,
    CAR_1,
    CAR_2,
    CAR_3,
    LOG
}


public class MovableStripManager
{
    private TerrainType _type;
    private List<MovableProp> _movableProps;
    private float zPos = 0;
    private float _logDirection = 0;
    private bool _isActive = false;
    private float _lastStripSpeed = 0;
    private int _lastPropOffset, _lastNumProps = 0;
    private int _lastCarChoice = -1;

    public bool IsActive
    {
        get { return _isActive; }
    }

    private static Dictionary<MovablePropType, List<MovableProp>> _movablePropDictionary;
    private static MovablePropType[] _usableRoadTypes;

    public static void CreateManagerPools()
    {
        _movablePropDictionary = new Dictionary<MovablePropType, List<MovableProp>>();
        foreach (var movablePropPefab in TerrainStripFactory.SharedInstance.movablePropPefabs)
        {
            List<MovableProp> tempPool = new List<MovableProp>(30);
            for (int i = 0; i < 30; i++)
            {
                MovableProp temMovableProp = MonoBehaviour.Instantiate(movablePropPefab, Vector3.zero, movablePropPefab.transform.rotation);
                temMovableProp.gameObject.SetActive(false);
                tempPool.Add(temMovableProp);
            }
            _movablePropDictionary.Add(movablePropPefab.Type, tempPool);
        }

        _usableRoadTypes = new[]
            {MovablePropType.CAR_1, MovablePropType.CAR_2, MovablePropType.TRUCK};
    }
    
    private MovableProp GetUsablePropFromPool(MovablePropType type)
    {
        List<MovableProp> propPool = _movablePropDictionary[type];
        for (int i = 0; i < propPool.Count; i++)
        {
            if (!propPool[i].gameObject.activeInHierarchy)
            {
                return propPool[i];
            }
        }

        MovableProp poolPrefab = null;

        foreach (var movablePropPefab in TerrainStripFactory.SharedInstance.movablePropPefabs)
        {
            if (movablePropPefab.Type == type)
            {
                poolPrefab = movablePropPefab;
                break;
            }
        }

        if (poolPrefab == null)
            return null;
        
        MovableProp newPropRef = MonoBehaviour.Instantiate(poolPrefab, Vector3.zero, poolPrefab.transform.rotation);
        
        propPool.Add(newPropRef);

        return newPropRef;
    }

    private void SetupRiverType()
    {
        //All these values need balancing
        float speed = TerrainStripFactory.RandomRangeExcept(1, 3, _lastStripSpeed);
        int propOffset = TerrainStripFactory.RandomRangeExcept(4, 6, _lastPropOffset);
        int numProps = Random.Range(3, 5);

        for (int i = 0; i < numProps; i++)
        {
            MovableProp tempMovableProp = GetUsablePropFromPool(MovablePropType.LOG);
            tempMovableProp.transform.position = new Vector3( (-9f * _logDirection) + (i * propOffset * _logDirection), 0.5f, zPos);
            tempMovableProp.transform.rotation = 
                Quaternion.Euler(tempMovableProp.transform.eulerAngles.x, 90 * _logDirection, 0);
            tempMovableProp.Speed = speed; 
            tempMovableProp.Direction = new Vector3(_logDirection, 0, 0);
            tempMovableProp.gameObject.SetActive(true);
            tempMovableProp.Type = MovablePropType.LOG;
            
            _movableProps.Add(tempMovableProp);
        }

    }

    private void SetupRoadType()
    {
        //All these values need balancing
        float speed = TerrainStripFactory.RandomRangeExcept(1.5f, 3, _lastStripSpeed);
        int propOffset = TerrainStripFactory.RandomRangeExcept(3, 6, _lastPropOffset);
        int numProps = TerrainStripFactory.RandomRangeExcept(3, 5, _lastNumProps);
        int randomSign = Random.Range(0, 2) * 2 - 1;
        int carChoice = TerrainStripFactory.RandomRangeExcept(0, _usableRoadTypes.Length, _lastCarChoice);

        for (int i = 0; i < numProps; i++)
        {
            MovableProp tempMovableProp = GetUsablePropFromPool(_usableRoadTypes[carChoice]);
            tempMovableProp.transform.position = new Vector3( (-9f * randomSign) + (i * propOffset * randomSign), 0.7f, zPos);
            tempMovableProp.transform.rotation = 
                Quaternion.Euler(tempMovableProp.transform.eulerAngles.x, 90 * randomSign, 0);
            tempMovableProp.Speed = speed; 
            tempMovableProp.Direction = new Vector3(randomSign, 0, 0);
            tempMovableProp.gameObject.SetActive(true);
            
            _movableProps.Add(tempMovableProp);
        }
    }

    public void ReturnToInactive()
    {
        for (int i = 0; i < _movableProps.Count; i++)
        {
            _movableProps[i].gameObject.SetActive(false);
        }
        
        _movableProps.Clear();
        
        _isActive = false;
    }

    public void SetupManager(TerrainType type, float zPosition, float direction = 0)
    {
        _type = type;
        zPos = zPosition;
        _logDirection = direction;
        _isActive = true;
        
        _movableProps = new List<MovableProp>(5);
        
        if (type == TerrainType.River)
            SetupRiverType();
        else
            SetupRoadType();
    }
}

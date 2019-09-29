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

    }
}

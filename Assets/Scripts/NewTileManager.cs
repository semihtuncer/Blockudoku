using System;
using System.Collections.Generic;
using UnityEngine;

public class NewTileManager : MonoBehaviour
{
    #region Variables
    [Header("Setup")]
    public List<Transform> spawnPoints;
    public List<RandomItem> placableTiles = new List<RandomItem>();

    RandomItemBag<GameObject> randomBag;

    [Header("Spawned")]
    public List<GameObject> spawnedTiles;

    public static NewTileManager instance;
    #endregion

    void Awake()
    {
        instance = this;
        InitBag();
    }
    void Start()
    {
        CheckForTile();
    }

    void InitBag()
    {
        randomBag = new RandomItemBag<GameObject>();

        foreach (RandomItem go in placableTiles)
        {
            randomBag.AddItem(go.itemGO, go.itemWeight);
        }
    }
    void SpawnNewTiles()
    {
        foreach (Transform t in spawnPoints)
        {
            GameObject tempGO = Instantiate(randomBag.PeekItem(), t.position, t.rotation);

            tempGO.transform.SetParent(t, true);
            tempGO.transform.localScale = new Vector3(GridManager.instance.startScale, GridManager.instance.startScale, GridManager.instance.startScale);

            spawnedTiles.Add(tempGO);
        }
    }

    public void AddTile(string g)
    {
        if (spawnedTiles.Count == 0)
            return;

        GameObject wantedPrefab = null;

        foreach (RandomItem prefab in placableTiles)
        {
            if (prefab.itemGO.name + "(Clone)" == g)
            {
                wantedPrefab = prefab.itemGO;
            }
        }

        if (wantedPrefab == null)
            return;

        if (spawnedTiles.Count == spawnPoints.Count)
        {
            foreach (Transform t in spawnPoints)
            {
                spawnedTiles.Remove(t.GetChild(0).gameObject);
                DestroyImmediate(t.GetChild(0).gameObject);
            }
        }
        foreach (Transform t in spawnPoints)
        {
            if (t.childCount == 0)
            {
                GameObject a = Instantiate(wantedPrefab, t.position, t.rotation);

                a.transform.SetParent(t, true);
                a.transform.localScale = new Vector3(GridManager.instance.startScale, GridManager.instance.startScale, GridManager.instance.startScale);

                spawnedTiles.Add(a);

                return;
            }
        }
    }
    public void CheckForTile()
    {
        List<GameObject> tempDelete = new List<GameObject>();

        foreach (GameObject g in spawnedTiles)
        {
            if (g == null)
            {
                tempDelete.Add(g);
            }
        }
        foreach (GameObject dg in tempDelete)
        {
            spawnedTiles.Remove(dg);
        }
        tempDelete.Clear();

        if (spawnedTiles.Count == 0)
        {
            SpawnNewTiles();
        }
    }
}

[System.Serializable]
public class RandomItem
{
    public GameObject itemGO;
    public int itemWeight = 1;
}
public class RandomItemBag<T>
{
    public int _totalWeight = 0;

    public int _arraySize = 0;
    public T[] _array = null;

    public RandomItemBag(int size = 128)
    {
        _arraySize = size;
        _array = new T[size];
    }

    public void AddItem(T itemType, int weight)
    {
        if (_arraySize <= _totalWeight + weight)
        {
            Array.Resize<T>(ref _array, _arraySize * 2);
            _arraySize *= 2;
        }
        for (int i = 0; i < weight; i++)
            _array[_totalWeight + i] = itemType;

        _totalWeight += weight;
    }

    public T PeekItem()
    {
        int ndx = UnityEngine.Random.Range(0, _totalWeight);

        return _array[ndx];
    }
}
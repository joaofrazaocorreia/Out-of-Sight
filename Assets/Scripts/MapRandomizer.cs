
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Events;

public class MapRandomizer : MonoBehaviour
{
    private enum RandomizerMode {Set, Fill}
    
    [Header("General variables")]
    [SerializeField] [EnumButtons] private RandomizerMode randomizerMode;
    [SerializeField] private int spawnOrder;
    [SerializeField] private Transform objectsParent;
    [SerializeField] private List<Transform> objectSpawnPositions;
    [SerializeField] private string fetchSpawnPositionsWithTag;
    [SerializeField] private bool destroyPreviousChildren;
    [SerializeField] private UnityEvent onFinishLoading;
    
    [Header("Set Spawns variables")]
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private int numOfSpawns = 1;
    [SerializeField] private bool moveExistingObject;
    
    [Header("Fill Spawns variables")]
    [SerializeField] private List<GameObject> requiredObjects;
    [SerializeField] private List<GameObject> randomObjects;

    private List<GameObject> spawnedObjects;
    private static bool navmeshUpdateQueued;

    private void Awake()
    {
        spawnedObjects = new List<GameObject>();
        navmeshUpdateQueued = false;


        if(destroyPreviousChildren && objectsParent.childCount > 0)
        {
            for(int i = objectsParent.childCount; i > 0; i--)
            {
                Destroy(objectsParent.GetChild(i-1).gameObject);
            }
        }

        if(spawnOrder <= 0)
            SpawnObjects();
        else
            StartCoroutine(DelaySpawnObjects(spawnOrder * 0.01f));
    }

    private IEnumerator DelaySpawnObjects(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        SpawnObjects();
    }

    private void SpawnObjects()
    {
        if(fetchSpawnPositionsWithTag != "")
        {
            FetchSpawnPositions(fetchSpawnPositionsWithTag);
        }
        
        switch(randomizerMode) 
        {
            case RandomizerMode.Fill:
            {
                FillObjectSpawns();
                break;
            }

            case RandomizerMode.Set:
            {
                SpawnSetObjects();
                break;
            }
        }

        onFinishLoading?.Invoke();
        StartCoroutine(UpdateNavMesh());
    }

    private void FillObjectSpawns()
    {
        List<int> usedPosIndexes = new List<int>();
        List<int> usedObjectIndexes = new List<int>();
        while(spawnedObjects.Count < objectSpawnPositions.Count)
        {
            int randomPosIndex = Random.Range(0, objectSpawnPositions.Count);

            if(!usedPosIndexes.Contains(randomPosIndex))
            {
                int randomObjectIndex;
                GameObject newObject;

                usedPosIndexes.Add(randomPosIndex);

                while(true)
                {
                    if(!CheckRequiredSpawns())
                    {
                        randomObjectIndex = Random.Range(0, requiredObjects.Count);

                        if(!usedObjectIndexes.Contains(randomObjectIndex))
                        {
                            newObject = Instantiate(requiredObjects[randomObjectIndex], objectsParent);

                            spawnedObjects.Add(requiredObjects[randomObjectIndex]);
                            usedObjectIndexes.Add(randomObjectIndex);
                            break;
                        }
                    }

                    else
                    {
                        randomObjectIndex = Random.Range(0, randomObjects.Count);

                        newObject = Instantiate(randomObjects[randomObjectIndex], objectsParent);

                        spawnedObjects.Add(randomObjects[randomObjectIndex]);
                        break;
                    }
                }
                
                newObject.transform.position = objectSpawnPositions[randomPosIndex].transform.position;
                newObject.transform.rotation = objectSpawnPositions[randomPosIndex].transform.rotation;
            }
        }

        StartCoroutine(UpdateNavMesh());
    }

    private void SpawnSetObjects()
    {
        List<int> usedPosIndexes = new List<int>();
        while(spawnedObjects.Count < numOfSpawns)
        {
            int randomPosIndex = Random.Range(0, objectSpawnPositions.Count);

            if(!usedPosIndexes.Contains(randomPosIndex))
            {
                GameObject newObject;

                if(!moveExistingObject)
                    newObject = Instantiate(objectToSpawn, objectsParent);
                else
                {
                    newObject = objectToSpawn;
                    numOfSpawns = 1;
                }
                
                newObject.SetActive(false);
                newObject.transform.position = objectSpawnPositions[randomPosIndex].transform.position;
                newObject.transform.rotation = objectSpawnPositions[randomPosIndex].transform.rotation;
                newObject.SetActive(true);

                spawnedObjects.Add(newObject);
                usedPosIndexes.Add(randomPosIndex);
            }
        }

        StartCoroutine(UpdateNavMesh());
    }

    private bool CheckRequiredSpawns()
    {
        foreach(GameObject go in requiredObjects)
        {
            if(!spawnedObjects.Contains(go))
            {
                return false;
            }
        }

        return true;
    }

    private void FetchSpawnPositions(string tag)
    {
        List<GameObject> newSpawns = GameObject.FindGameObjectsWithTag(tag).ToList();

        foreach(GameObject go in newSpawns)
        {
            objectSpawnPositions.Add(go.transform);
        }
    }

    private IEnumerator UpdateNavMesh()
    {
        if(!navmeshUpdateQueued)
        {
            NavMeshSurface navMeshSurface = FindAnyObjectByType<NavMeshSurface>();
            navmeshUpdateQueued = true;

            yield return new WaitForSecondsRealtime(0.1f);

            navMeshSurface.RemoveData();
            navMeshSurface.BuildNavMesh();

            navmeshUpdateQueued = false;
        }

        yield return null;
    }
}

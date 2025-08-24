// Plik: Scripts/Core/ObjectPooler.cs
using UnityEngine;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour
{
    [SerializeField] private GameObject prefabToPool;
    [SerializeField] private int initialPoolSize = 50;

    private readonly Queue<GameObject> availableObjects = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject newObj = Instantiate(prefabToPool, transform);
            newObj.SetActive(false);
            availableObjects.Enqueue(newObj);
        }
    }

    public GameObject Get()
    {
        if (availableObjects.Count == 0)
        {
            GameObject newObj = Instantiate(prefabToPool, transform);
            return newObj;
        }

        GameObject obj = availableObjects.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject objToReturn)
    {
        objToReturn.SetActive(false);
        availableObjects.Enqueue(objToReturn);
    }
}
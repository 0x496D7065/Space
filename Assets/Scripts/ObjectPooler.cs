using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public GameObject prefab;
    public int poolSize = 20;

    private Queue<GameObject> pool = new Queue<GameObject>();

    public void Awake()
    {
        Debug.Log($"[Pooler] Initializing pool on {gameObject.name}");
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetFromPool(Vector3 position, Quaternion rotation)
    {
        GameObject obj = pool.Dequeue();
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        pool.Enqueue(obj);
        return obj;
    }
    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        //pool.Enqueue(obj); No need to re-enqueue twice, GetFromPool enqueues it immediatly for reuse
    }
}
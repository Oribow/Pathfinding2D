using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading;

/*
Author: Oribow
*/
namespace Pooling
{
    public class PoolManager : ScriptableObject
    {
        GameObject prefab;
        Stack<GameObject> pool;

        public PoolManager(GameObject prefab)
        {
            this.prefab = prefab;
            pool = new Stack<GameObject>();
        }

        public void DestroyObject(GameObject obj)
        {
            if (!obj.Equals(prefab))
            {
                Debug.LogWarning("Tried to destroyed a GameObject not defined in this pool! Will destroy it anyway.");
                Destroy(obj);
            }
            obj.SetActive(false);
            pool.Push(obj);
        }

        public GameObject CreateObject()
        {
            if (pool.Count == 0)
                CreateNewObject();
            GameObject obj = pool.Pop();
            obj.SetActive(true);
            return obj;
        }

        public GameObject CreateObject(float destroyTimer)
        {
            if (pool.Count == 0)
                CreateNewObject();
            GameObject obj = pool.Pop();
            obj.SetActive(true);
            Timer timer = new Timer(TimedDestroy, obj, Mathf.FloorToInt(destroyTimer * 1000), Timeout.Infinite);
            return obj;
        }

        private void CreateNewObject()
        {
            GameObject obj = Instantiate(prefab) as GameObject;
            pool.Push(obj);
        }

        void TimedDestroy(object gameObject)
        {
            DestroyObject((GameObject)gameObject);
        }
    }
}

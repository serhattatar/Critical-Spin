using System.Collections.Generic;
using UnityEngine;

namespace CriticalSpin.Core
{
    /// <summary>
    /// I wrote this simple object pool for UI prefabs. 
    /// It completely prevents the game from lagging during Instantiate() and Destroy() calls while spinning!
    /// </summary>
    public static class UIObjectPool
    {
        private static readonly Dictionary<int, Queue<Component>> _pools = new Dictionary<int, Queue<Component>>();

        public static T Spawn<T>(T prefab, Transform parent) where T : Component
        {
            if (prefab == null) return null;
            int key = prefab.gameObject.GetInstanceID();

            if (_pools.TryGetValue(key, out var queue) && queue.Count > 0)
            {
                T obj = (T)queue.Dequeue();
                if (obj == null) return Spawn(prefab, parent);

                obj.transform.SetParent(parent, false);
                obj.transform.localScale = Vector3.one;
                obj.gameObject.SetActive(true);
                return obj;
            }

            T newObj = Object.Instantiate(prefab, parent);
            newObj.transform.localScale = Vector3.one;
            newObj.gameObject.SetActive(true);
            return newObj;
        }

        public static void Despawn<T>(T obj, T prefab) where T : Component
        {
            if (obj == null || prefab == null) return;
            int key = prefab.gameObject.GetInstanceID();

            if (!_pools.ContainsKey(key))
                _pools[key] = new Queue<Component>();

            obj.gameObject.SetActive(false);
            obj.transform.SetParent(null, false);
            _pools[key].Enqueue(obj);
        }

        public static void ClearAll()
        {
            foreach (var queue in _pools.Values)
            {
                while (queue.Count > 0)
                {
                    Component obj = queue.Dequeue();
                    if (obj != null) Object.Destroy(obj.gameObject);
                }
            }
            _pools.Clear();
        }
    }
}

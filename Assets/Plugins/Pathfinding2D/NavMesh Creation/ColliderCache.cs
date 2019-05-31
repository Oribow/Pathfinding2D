using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class ContourCache
{
    public enum CacheCheckResult { NoChanges, Changes, NotCached}

    public int CachedColliderCount { get { return colliderInfo.Count; } }
    List<ColliderInfo> colliderInfo;

    public ContourCache(int intitalColliderCapacity)
    {
        colliderInfo = new List<ColliderInfo>(intitalColliderCapacity);
    }

    public void ResetScopeFlags()
    {
        foreach (var info in colliderInfo)
        {
            info.stillInScopeFlag = false;
        }
    }

    public CacheCheckResult DidColliderChanged(Collider2D collider, out ColliderInfo matchedInfo)
    {
        foreach (var info in colliderInfo)
        {
            if (info.collider == collider)
            {
                info.stillInScopeFlag = true;
                matchedInfo = info;
                return info.UpdateColliderInfo(collider) ? CacheCheckResult.Changes : CacheCheckResult.NoChanges;
            }
        }
        matchedInfo = new ColliderInfo(collider);
        colliderInfo.Add(matchedInfo);
        return CacheCheckResult.NotCached;
    }

    public void ClearUpCache()
    {
        for(int iCol = 0; iCol < colliderInfo.Count; iCol++)
        {
            if(colliderInfo[iCol].collider == null || !colliderInfo[iCol].stillInScopeFlag)
            {
                colliderInfo.RemoveAt(iCol);
                iCol--;
            }
        }
    }

    public class ColliderInfo
    {
        public Collider2D collider;
        //public ContourOverlapTree.Node node;

        //If false, this colliderInfo should be removed from the cache.
        public bool stillInScopeFlag;
        Vector3 position;
        Quaternion rotation;
        Vector3 scale;

        public ColliderInfo(Collider2D collider)
        {
            this.collider = collider;
            this.position = collider.transform.position;
            this.rotation = collider.transform.rotation;
            this.scale = collider.transform.lossyScale;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Returns true, if the colliders transform changed since the last update.</returns>
        public bool UpdateColliderInfo(Collider2D other)
        {
            bool hadToUpdate = false;
            if (other.transform.position != position)
            {
                hadToUpdate = true;
                position = other.transform.position;
            }
            if (other.transform.rotation != rotation)
            {
                hadToUpdate = true;
                rotation = other.transform.rotation;
            }
            if (other.transform.lossyScale != scale)
            {
                hadToUpdate = true;
                scale = other.transform.lossyScale;
            }
            return hadToUpdate;
        }
    }
}

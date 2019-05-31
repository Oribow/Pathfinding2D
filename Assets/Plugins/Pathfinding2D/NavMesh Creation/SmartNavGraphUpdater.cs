using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//Decides whenever the Graph needs updating and initiate this updating.
class SmartNavGraphUpdater
{
    ContourCache colCache;
    NavGraphBuildScheduler scheduler;
    ContourOverlapTree overlapGraph;

    public SmartNavGraphUpdater(NavGraphBuildScheduler scheduler)
    {
        this.colCache = new ContourCache(20);
        this.scheduler = scheduler;
        overlapGraph = new ContourOverlapTree();
    }

    public void DoUpdate(Collider2D[] collider)
    {
        bool changesHappened = false;
        int cachedColliderCount = colCache.CachedColliderCount;
        List<ContourCache.ColliderInfo> changedCollider = new List<ContourCache.ColliderInfo>(collider.Length);
        ContourCache.ColliderInfo matchedInfo;

        //Reset flags, so we can tell which collider are still in scope
        colCache.ResetScopeFlags();

        foreach (var col in collider)
        {
            var checkResult = colCache.DidColliderChanged(col, out matchedInfo);
            if (checkResult != ContourCache.CacheCheckResult.NoChanges)
            {
                changesHappened = true;
                changedCollider.Add(matchedInfo);
            }
            if (checkResult != ContourCache.CacheCheckResult.NotCached)
            {
                cachedColliderCount--;
            }
        }

        if (cachedColliderCount > 0)
        {
            //We got less collider then cached. Some either have left the scope or were destroyed.
            colCache.ClearUpCache();
            changesHappened = true;
        }

        if (!changesHappened)
            return; //Nothing changed, so there is nothing left to do.

        //Changes happened
       // scheduler.ScheduleGeneration(overlapGraph, changedCollider);
    }
}

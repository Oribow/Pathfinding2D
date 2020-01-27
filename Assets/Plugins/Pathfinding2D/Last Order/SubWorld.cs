using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public delegate void SubWorldEventHandler<U>(SubWorld sender, U eventArgs);

public class SubWorld : MonoBehaviour
{
    /*
    [SerializeField, ReadOnly]
    List<SurfaceContainer> surfaces;

    private World world;

    public void Add(NavSurface2d surface)
    {
        surfaces.Add(new SurfaceContainer(surface));
        world?.OnNavNodeConstructorAdded(surface);
    }

    public void Add(ILinkPoint linkP)
    {

    }

    public void Remove(NavSurface2d surface)
    {
        SurfaceContainer container = null;
        for (int i = 0; i < surfaces.Count; i++)
        {
            if (surfaces[i].surface == surface)
            {
                container = surfaces[i];
                surfaces[i] = surfaces[surfaces.Count - 1];
                surfaces.RemoveAt(surfaces.Count - 1);
                break;
            }
        }
        // call all dependants to update themself
        // they should not call remove themself
        // will remove their nav nodes (only thoose ending in this surface)
        foreach (var depend in container.dependants)
        {
            depend.OnRemovalFromSurface(container.surface);
            world?.OnNavNodeConstructorRemoved(depend);
        }

        // will remove nav nodes if they exist
        world?.OnNavNodeConstructorRemoved(container.surface);
    }

    public void Remove(ILinkPoint linkP)
    {

    }*/
}

[System.Serializable]
class SurfaceContainer
{
    public readonly NavSurface2d surface;
    public readonly List<ILinkPoint> dependants;

    public SurfaceContainer(NavSurface2d surface)
    {
        this.surface = surface;
        this.dependants = new List<ILinkPoint>(4);
    }
}

public class NavNodeConstructorEventArgs : EventArgs {
    public readonly INavNodeConstructor constructor;

    public NavNodeConstructorEventArgs(INavNodeConstructor constructor)
    {
        this.constructor = constructor;
    }
}

public interface INavNodeConstructor
{
    void CreateNavNodes(NavGraph2d graph);
    void DestroyNavNodes(NavGraph2d graph);
}

public interface ILinkPoint : INavNodeConstructor {
    void OnRemovalFromSurface(NavSurface2d surface);

}
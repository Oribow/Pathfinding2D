using UnityEngine;
using System.Collections;
//using Entities;
//using LightSensing;

public class NavAgent : MonoBehaviour
{
    /*[SerializeField, HideInInspector, AssignEntityAutomaticly]
    ThightAIMovementEntity actor;

    NavData2d.NavPosition StartPoint
    {
        get
        {
            if (cacheRefreshTime != Time.unscaledTime)
                RefreshStartPointMapping();
            return startPointCache;
        }
    }

    NavData2d.NavPosition startPointCache;
    float cacheRefreshTime;

    [SerializeField]
    Transform pathStartPoint;
    [SerializeField]
    public LightSkin lightSkin;

    PathPlaner pathPlaner;
    OnPathComputationFinished cPathFoundCallback;
    NavPathIterator cPath;
    LightMarker[] emptyMarkerArray;
    bool reachedGoal;

    public bool IsFollowingAPath { get { return cPath.path != null; } }
    public bool IsOnEdgeNode { get { return IsFollowingAPath && cPath.CurrentSegement.GetType() == typeof(PathSegment); } }
    public bool ReachedGoal { get { return reachedGoal; } }

    void Awake()
    {
        cPath = new NavPathIterator();
        emptyMarkerArray = new LightMarker[] { };
        pathPlaner = PathPlaner.Instance;
    }

    public delegate void OnPathComputationFinished(bool foundPath);

    public void SetDestination(NavData2d.NavPosition dest, OnPathComputationFinished onPathFinished, bool startsInUncomfortableLight = false, LightMarker[] lightMarker = null)
    {
        if (StartPoint.navNodeIndex == -1 || dest.navNodeIndex == -1) //We don't know where we are, abort!
        {
            onPathFinished.Invoke(false);
            return;
        }
        if (lightMarker == null)
            lightMarker = emptyMarkerArray;
        Debug.Assert(cPathFoundCallback == null);
        cPathFoundCallback = onPathFinished;
        pathPlaner.FindRequestedPath(new PathRequest(StartPoint, dest, OnPathCompleted, lightSkin, startsInUncomfortableLight, lightMarker));
        reachedGoal = false;
    }

    public void FleeFrom(Vector2[] threats, float targetDistanceFromThreats, OnPathComputationFinished onPathFinished, bool startsInUncomfortableLight = false, LightMarker[] lightMarker = null)
    {
        if (StartPoint.navNodeIndex == -1) //We don't know where we are, abort!
        {
            onPathFinished.Invoke(false);
            return;
        }
        if (lightMarker == null)
            lightMarker = emptyMarkerArray;
        Debug.Assert(cPathFoundCallback == null);
        cPathFoundCallback = onPathFinished;
        pathPlaner.Flee(new FleeRequest(StartPoint, threats, OnPathCompleted, lightSkin, startsInUncomfortableLight, targetDistanceFromThreats, lightMarker));
        reachedGoal = false;
    }

    public void SetDestination(Vector2 dest, OnPathComputationFinished onPathFinished, bool startsInUncomfortableLight = false, LightMarker[] lightMarker = null)
    {
        SetDestination(pathPlaner.MapPoint(dest), onPathFinished, startsInUncomfortableLight, lightMarker);
    }

    public void Stop()
    {
        if (cPath.path == null)
            return;
        cPath.CurrentSegement.StopTravers(actor.CC2DThightAIMotor);
        cPath.path = null;
        StopMoving();
    }

    void OnPathCompleted(NavPath path)
    {
        Debug.Assert(cPathFoundCallback != null);
        if (path == null)
            cPathFoundCallback(false);
        else
        {
            EnableMovementAgain();
            cPath.AssignNewPath(path);
            cPath.CurrentSegement.InitTravers(actor.CC2DThightAIMotor, cPath.GetNextSegment());
            cPathFoundCallback(true);
        }

        cPathFoundCallback = null;
    }

    void OnUpdatePathCompleted(NavPath path)
    {
        Debug.Assert(cPathFoundCallback != null);
        if (path == null)
            cPathFoundCallback(false);
        else
        {
            cPath.AssignNewPath(path);
            cPath.CurrentSegement.InitTravers(actor.CC2DThightAIMotor, cPath.GetNextSegment());
            cPathFoundCallback(true);
        }

        cPathFoundCallback = null;
    }

    void FixedUpdate()
    {
        if (cPath.path == null)
            return;

        cPath.path.Visualize();

        if (cPath.CurrentSegement.ReachedTarget(pathStartPoint.position))
        {
            cPath.CurrentSegement.StopTravers(actor.CC2DThightAIMotor);
            if (!cPath.NextSegment())
            {
                cPath.path = null;
                StopMoving();
                reachedGoal = true;
                return;
            }
            cPath.CurrentSegement.InitTravers(actor.CC2DThightAIMotor, cPath.GetNextSegment());
        }
        /*else if (cPath.IsSegmentTimedOut)
        {
            /*cPath.path = null;
            Debug.Log("Time out. Abort!");
            StopMoving();
            //Time out has problems! Usage not recommended.
        }
        else if (!cPath.CurrentSegement.IsOnTrack(pathStartPoint.position))
        {
            //cPath.path = null;
            Debug.Log("Lost Path. Abort!");
            cPath.path = null;
            StopMoving();
            return;
        }
        // else
        // {
        cPath.CurrentSegement.UpdateMovementInput(actor.CC2DThightAIMotor.CurrentMovementInput, actor.CC2DThightAIMotor);
        // }        
    }

    void StopMoving()
    {
        actor.CC2DThightAIMotor.CurrentMovementInput.ResetToNeutral();
        actor.CC2DThightAIMotor.SetManualXSpeed(0);
    }

    void EnableMovementAgain()
    {
        actor.CC2DThightAIMotor.StopUsingManualSpeed();
    }

    void RefreshStartPointMapping()
    {
        startPointCache = pathPlaner.MapPoint(pathStartPoint.position);
        cacheRefreshTime = Time.unscaledTime;
    }

    class NavPathIterator
    {
        public IPathSegment CurrentSegement { get { return path.pathSegments[currentSegment]; } }
        public bool IsSegmentTimedOut { get { return Time.time - segmentStartTime > CurrentSegement.TimeOut; } }

        public NavPath path;
        int currentSegment;
        float segmentStartTime;

        public IPathSegment GetNextSegment()
        {
            if (currentSegment + 1 >= path.pathSegments.Length)
                return null;
            return path.pathSegments[currentSegment];
        }

        public bool NextSegment()
        {
            currentSegment++;
            if (currentSegment >= path.pathSegments.Length)
                return false;
            return true;
        }

        public void AssignNewPath(NavPath path)
        {
            this.path = path;
            currentSegment = 0;
            segmentStartTime = Time.time;
        }
    }*/
}

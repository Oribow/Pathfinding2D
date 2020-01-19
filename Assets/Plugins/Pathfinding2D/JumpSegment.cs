using UnityEngine;
using System.Collections;
using Pathfinding2D;
using System;

public class JumpSegment : IPathSegment
{
    public const float TimeOutFudgeSeconds = 3;

    float xVel;
    float jumpForce;
    float xMin;
    float xMax;
    float yMin;
    float yMax;
    float timeOut;
    float duration;
    Vector2 goal;
    Vector2 start; //only for debug
    /*
    public JumpSegment(JumpLink link)
    {
        goal = link.targetPos.navPoint;
        start = link.startPoint;

        jumpForce = link.jumpForce;
        yMin = link.yMin - 0.1f;
        yMax = link.yMax + 0.1f;
        if (start.x > goal.x)
        {
            xMin = goal.x - 0.1f;
            xMax = start.x + 1;
            xVel = -link.xVel;
        }
        else
        {
            xMin = start.x - 1;
            xMax = goal.x + 0.1f;
            xVel = link.xVel;
        }
        duration = Mathf.Abs((link.xMax - link.xMin) / xVel);
        timeOut = duration + TimeOutFudgeSeconds;
    }*/

    public override float TimeOut
    {
        get
        {
            return timeOut;
        }
    }
    /*
    public override void UpdateMovementInput(MovementInput input, CC2DThightAIMotor motor)
    {
    }

    public override bool IsOnTrack(Vector2 position)
    {
        if (position.x < xMin || position.x > xMax /*|| position.y < yMin || position.y > yMax)
            return false;
        return true;
    }

    public override bool ReachedTarget(Vector2 position)
    {
        if ((position - goal).sqrMagnitude <= 0.05f)
            return true;
        return false;
    }

    public override void InitTravers(CC2DThightAIMotor motor, IPathSegment nextSeg)
    {
        motor.EnsureCorrectPosition(start.x); //Little teleporting hack, to ensure correct jumping!
        motor.SetManualXSpeed(xVel);
        motor.ManualJump(jumpForce, duration);
        motor.CurrentMovementInput.ResetToNeutral();
    }

    public override void StopTravers(CC2DThightAIMotor motor)
    {
        motor.StopUsingManualSpeed();
    }

    public override void Visualize()
    {
        Debug.DrawLine(start, goal, Color.blue);
    }

    public override float StartSpeed(CC2DThightAIMotor motor)
    {
        return xVel;
    }*/
}

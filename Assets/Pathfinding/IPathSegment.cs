using UnityEngine;
using System.Collections;
using System;

[Serializable]
public abstract class IPathSegment {
    public abstract float TimeOut { get; }

   /* public abstract float StartSpeed(CC2DThightAIMotor motor);
    public abstract void InitTravers(CC2DThightAIMotor motor, IPathSegment nextSeg);
    public abstract void StopTravers(CC2DThightAIMotor motor);
    public abstract void UpdateMovementInput(MovementInput input, CC2DThightAIMotor motor);
    public abstract bool IsOnTrack(Vector2 position);
    public abstract bool ReachedTarget(Vector2 position);
    public abstract void Visualize();*/
}

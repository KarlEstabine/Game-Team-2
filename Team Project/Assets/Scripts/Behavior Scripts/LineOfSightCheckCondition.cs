using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Line Of Sight Check", story: "Check [Target] With Line Of Sight [detector]", category: "Conditions", id: "4c27f3393c2ea0cc1c2f84e31ba8afb0")]
public partial class LineOfSightCheckCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<LineOfSightDetector> Detector;

    public override bool IsTrue()
    {
        return Detector.Value.performDetection(Target.Value) != null;
    }
}

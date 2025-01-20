using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Attack", story: "perform [attack]", category: "Action", id: "dd8cbee61307b5d2cf0c3f0cebf50b56")]
public partial class AttackAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAttack> Attack;

    protected override Status OnUpdate()
    {
        // Start the attack if not already attacking
        Attack.Value.performAttack();

        Debug.Log($"AttackAction: {Attack.Value.isShooting}");

        // Check if the enemy is still shooting
        if (!Attack.Value.isShooting)
        {
            return Status.Running; // The attack is still ongoing
        }
        else
        {
            return Status.Success; // The attack is finished
        }
    }
}


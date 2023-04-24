using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AgentBrainRegistry
{
    public static AgentBrain[] GetAllBrains()
    {
        return new AgentBrain[]
        {
            // TODO In case of creating new brains, add them here.
            //      If two "brains" reuse the same class but with different properties,
            //      use constructor arguments to affect the behavior.

            // Instantiate brain #1 (you can keep this one)
            new HumanBrain(),

            // Instantiate brain #2 (replace this one)
            new SimpleCpuBrain(-1),

            // Instantiate brain #2 (replace this one)
            new SimpleCpuBrain(1),

            // Instantiate brain #3  (replace this one)
            new IdlingBrain()
        };
    }
}

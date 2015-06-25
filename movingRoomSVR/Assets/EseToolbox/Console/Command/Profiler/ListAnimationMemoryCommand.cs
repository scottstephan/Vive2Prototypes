using Ese;
using UnityEngine;

public class ListAnimationMemoryCommand : AbstractListMemoryCommand<Animation>
{
    public override string[] GetCommands()
    {
        return new string[] {"listAnimation"};
    }
}

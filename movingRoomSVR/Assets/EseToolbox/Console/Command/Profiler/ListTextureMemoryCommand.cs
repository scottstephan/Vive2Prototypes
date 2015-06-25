using Ese;
using UnityEngine;


public class ListTextureMemoryCommand : AbstractListMemoryCommand<Texture>
{
    public override string[] GetCommands()
    {
        return new string[] {"listTexture"};
    }
}
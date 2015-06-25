using Ese;
using UnityEngine;


public class ListMeshMemoryCommand : AbstractListMemoryCommand<Mesh>
{
    public override string[] GetCommands()
    {
        return new string[] {"listMesh"};
    }
}

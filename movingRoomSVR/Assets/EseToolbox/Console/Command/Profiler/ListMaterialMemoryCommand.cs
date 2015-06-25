using Ese;
using UnityEngine;

public class ListMaterialMemoryCommand : AbstractListMemoryCommand<Material>
{
    public override string[] GetCommands()
    {
        return new string[] {"listMaterial"};
    }
}
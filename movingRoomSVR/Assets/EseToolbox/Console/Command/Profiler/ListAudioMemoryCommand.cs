using Ese;
using UnityEngine;

public class ListAudioMemoryCommand : AbstractListMemoryCommand<AudioSource>
{
    public override string[] GetCommands()
    {
        return new string[] {"listAudio"};
    }
}

using System;
using LiveSplit.EMUHELP;

public class MasterSystem : SMS { }
public class SegaMasterSystem : Genesis { }

public partial class SMS : EmuBase
{
    public SMS()
    {
        EmulatorNames = new string[]
        {
            "retroarch",
            "blastem",
            "Fusion",
            "EmuHawk",
        };

        GameProcess = new ProcessHook(EmulatorNames);
    }

    protected override void InitActions()
    {
        var Init = game.ProcessName.ToLower() switch
        {
            "retroarch" => Retroarch(),
            "blastem" => BlastEm(),
            "fusion" => Fusion(),
            "emuhawk" => EmuHawk(),
            _ => throw new NotImplementedException()
        };
        
        KeepAlive = Init.Item2;
        Watchers = Load(Init.Item1);
    }
}
using System;
using LiveSplit.EMUHELP;

public partial class SNES : EmuBase
{
    public SNES()
    {
        EmulatorNames = new string[]
        {
            //"retroarch",
            "EmuHawk",
            "snes9x-x64",
            "snes9x",
        };

        GameProcess = new ProcessHook(EmulatorNames);
    }

    protected override void InitActions()
    {
        var Init = game.ProcessName.ToLower() switch
        {
            //"retroarch" => Retroarch(),
            "emuhawk" => EmuHawk(),
            "snes9x-x64" or "snes9x" => Snes9x(),
            _ => throw new NotImplementedException()
        };

        KeepAlive = Init.Item2;
        Watchers = Load(Init.Item1);
    }
}
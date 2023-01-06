using System;
using System.Collections.Generic;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;
using System.Linq;

public class Playstation2 : PS2 { }

public partial class PS2 : EmuBase
{
    public Dictionary<string, string> Gamecodes { get; set; }
    public string GameRegion { get; private set; }

    public PS2()
    {
        EmulatorNames = new string[]
        {
            "pcsx2",
            "pcsx2-qtx64",
            "pcsx2-qtx64-avx2",
            "retroarch",
        };

        GameProcess = new ProcessHook(EmulatorNames);
    }

    public new bool Update()
    {
        if (!base.Update())
            return false;

        if (Gamecodes != null)
        {
            GameRegion = null;

            foreach (var entry in Watchers.Where(w => w is StringWatcher && w.Name.Contains("_Gamecode")))
            {
                if (entry.Current != null && Gamecodes.ContainsKey((string)entry.Current))
                {
                    GameRegion = Gamecodes[(string)entry.Current];
                    break;
                }
            }

            if (GameRegion == null)
                return false;
        }

        return true;
    }

    protected override void InitActions()
    {
        var Init = game.ProcessName.ToLower() switch
        {
            "pcsx2" or "pcsx2-qtx64" or "pcsx2-qtx64-avx2" => PCSX2(),
            "retroarch" => Retroarch(),
            _ => throw new NotImplementedException()
        };

        KeepAlive = Init.Item2;
        Watchers = Load(Init.Item1);
    }

    public new MemoryWatcher this[string index] => Gamecodes == null ? Watchers[index] : Watchers[$"{GameRegion}_{index}"];
}
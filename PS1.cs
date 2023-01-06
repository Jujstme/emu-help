using System;
using System.Collections.Generic;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public class Playstation : PS1 { }

public class Playstation1 : PS1 { }

public partial class PS1 : EmuBase
{
    public Dictionary<string, string> Gamecodes { get; set; }
    public string GameRegion { get; private set; }

    public PS1()
    {
        EmulatorNames = new string[]
        {
            "ePSXe",
            "psxfin",
            "duckstation-qt-x64-ReleaseLTCG",
            "duckstation-nogui-x64-ReleaseLTCG",
            "retroarch",
            "pcsx-redux.main",
            "xebra",
            "EmuHawk",
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

            foreach(var entry in Watchers.Where(w => w is StringWatcher && w.Name.Contains("_Gamecode")))
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
            "epsxe" => ePSXe(),
            "psxfin" => pSX(),
            "duckstation-qt-x64-releaseltcg" or "duckstation-nogui-x64-releaseltcg" => Duckstation(),
            "retroarch" => Retroarch(),
            "pcsx-redux.main" => PCSX_Redux(),
            "xebra" => Xebra(),
            "emuhawk" => EmuHawk(),
            _ => throw new NotImplementedException()
        };

        KeepAlive = Init.Item2;
        Watchers = Load(Init.Item1);
    }

    public new MemoryWatcher this[string index] => Gamecodes == null ? Watchers[index] : Watchers[$"{GameRegion}_{index}"];
}
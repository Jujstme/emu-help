using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;
using System.Linq;

public class Gamecube : GCN { }
public class GameCube : GCN { }

public partial class GCN : EmuBase
{
    public string[] Gamecodes { get; set; }
    private IntPtr MEM1 { get; set; }

    public GCN()
    {
        EmulatorNames = new string[]
        {
            "Dolphin",
        };

        GameProcess = new ProcessHook(EmulatorNames);
    }

    public new bool Update()
    {
        if (!base.Update())
            return false;

        if (Gamecodes != null && !Gamecodes.Contains(game.ReadString(MEM1, 6, " ")))
            return false;

        return true;
    }

    protected override void InitActions()
    {
        var Init = game.ProcessName.ToLower() switch
        {
            "dolphin" => Dolphin(),
            _ => throw new NotImplementedException()
        };

        MEM1 = Init.Item1;
        KeepAlive = Init.Item2;
        Watchers = Load(MEM1);

        if (Endianess == Endianess.BigEndian)
            LittleEndianWatchers = ToLittleEndian.SetFakeWatchers(Watchers);
    }
}
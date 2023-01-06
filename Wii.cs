using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;
using System.Linq;

public class WII : Wii { }

public partial class Wii : EmuBase
{
    protected new Func<IntPtr, IntPtr, MemoryWatcherList> Load { get; set; }
    public string[] Gamecodes { get; set; }
    private IntPtr MEM1 { get; set; }
    private IntPtr MEM2 { get; set; }

    public Wii()
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
            _ => throw new NotImplementedException(),
        };

        MEM1 = Init.Item1;
        MEM2 = Init.Item2;
        KeepAlive = Init.Item3;
        Watchers = Load(MEM1, MEM2);

        if (Endianess == Endianess.BigEndian)
            LittleEndianWatchers = ToLittleEndian.SetFakeWatchers(Watchers);
    }
}
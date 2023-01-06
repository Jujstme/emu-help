using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public class Megadrive : Genesis { }
public class MegaDrive : Genesis { }

public partial class Genesis : EmuBase
{
    public Genesis()
    {
        EmulatorNames = new string[]
        {
            "retroarch",
            "SEGAGameRoom",
            "SEGAGenesisClassics",
            "Fusion",
            "gens",
            "blastem",
            "EmuHawk",
        };

        GameProcess = new ProcessHook(EmulatorNames);
    }

    protected override void InitActions()
    {
        var Init = game.ProcessName.ToLower() switch
        {
            "retroarch" => Retroarch(),
            "segagenesisclassics" or "segagameroom" => SEGAClassics(),
            "fusion" => Fusion(),
            "gens" => Gens(),
            "blastem" => BlastEm(),
            "emuhawk" => EmuHawk(),
            _ => throw new NotImplementedException()
        };

        IntPtr WRAM = Init.Item1;
        KeepAlive = Init.Item2;
        Watchers = Load(WRAM);

        if (Endianess == Endianess.LittleEndian)
        {
            foreach (var watcher in Watchers)
            {
                if (watcher is MemoryWatcher<byte> || watcher is MemoryWatcher<sbyte> || watcher is MemoryWatcher<bool>)
                {
                    var Address = (long)watcher.GetProperty<IntPtr>("Address") - (long)WRAM;
                    Address = (Address & 1) == 0 ? Address + 1 : Address - 1;
                    watcher.SetProperty("Address", (IntPtr)((long)WRAM + Address));
                }
            }
        }

        if (Endianess == Endianess.BigEndian)
            LittleEndianWatchers = ToLittleEndian.SetFakeWatchers(Watchers);
    }
}
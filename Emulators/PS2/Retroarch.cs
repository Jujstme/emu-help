using System;
using System.Collections.Generic;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class PS2
{
    private Tuple<IntPtr, Func<bool>> Retroarch()
    {
        var SupportedCored = new Dictionary<string, int>
        {
            { "pcsx2_libretro.dll", 0x17000 }, 
        };

        var WRAMbases = new Dictionary<string, IntPtr>();

        foreach (var entry in SupportedCored)
            WRAMbases.Add(entry.Key, game.MemoryPages(true).FirstOrDefault(p => p.AllocationProtect == MemPageProtect.PAGE_NOACCESS && (int)p.RegionSize == entry.Value).BaseAddress);
        
        Debugs.Info("lel");
        IntPtr WRAMbase = WRAMbases.Values.First(b => !b.IsZero());

        Func<bool> checkIfAlive = () => game.ReadBytes(WRAMbase, 1, out _);

        Debugs.Info("  => Hooked to emulator: Retroarch");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, checkIfAlive);
    }
}

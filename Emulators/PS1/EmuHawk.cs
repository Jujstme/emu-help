using System;
using System.Collections.Generic;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class PS1
{
    private Tuple<IntPtr, Func<bool>> EmuHawk()
    {
        var SupportedCores = new Dictionary<string, Tuple<int, int>>
        {
            { "default", new Tuple<int, int>(0xD819000, 0x2FADC0) },
            { "mednafen_psx_hw_libretro.dll", new Tuple<int, int>( 0x200000, 0) },
            { "pcsx_rearmed_libretro.dll", new Tuple<int, int>(0x210000, 0) },
        };

        var WRAMbases = new Dictionary<string, IntPtr>();

        foreach (var entry in SupportedCores)
            WRAMbases.Add(entry.Key, game.MemoryPages(true).FirstOrDefault(p => p.Type == MemPageType.MEM_MAPPED && (int)p.RegionSize == entry.Value.Item1).BaseAddress);

        var currentCore = WRAMbases.First(c => !c.Value.IsZero());
        IntPtr WRAMbase = currentCore.Value + SupportedCores[currentCore.Key].Item2;

        bool checkIfAlive() => game.ReadBytes(WRAMbase, 1, out _);

        Debugs.Info("  => Hooked to emulator: EmuHawk / BizHawk");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, checkIfAlive);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class PS1
{
    public Tuple<IntPtr, Func<bool>> EmuHawk()
    {
        var SupportedCores = new Dictionary<string, int>
        {
            { "default", 0xD819000 },
            { "mednafen_psx_hw_libretro.dll", 0x200000 },
            //{ "mednafen_psx_libretro.dll", 0x200000 }, // same as above
            { "pcsx_rearmed_libretro.dll", 0x210000 },
        };

        var WRAMbases = new Dictionary<string, IntPtr>();

        foreach (var entry in SupportedCores)
            WRAMbases.Add(entry.Key, game.MemoryPages(true).FirstOrDefault(p => p.Type == MemPageType.MEM_MAPPED && (int)p.RegionSize == entry.Value).BaseAddress);

        IntPtr WRAMbase = WRAMbases.Values.First(b => !b.IsZero());

        if (WRAMbase == WRAMbases["default"])
            WRAMbase += 0x2FADC0;


        bool checkIfAlive() => game.ReadBytes(WRAMbase, 1, out _);

        Debugs.Info("  => Hooked to emulator: EmuHawk / BizHawk");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, (Func<bool>)checkIfAlive);
    }
}

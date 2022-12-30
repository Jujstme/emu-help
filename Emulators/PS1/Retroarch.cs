using System;
using System.Collections.Generic;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class PS1
{
    private Tuple<IntPtr, Func<bool>> Retroarch()
    {
        var SupportedCored = new Dictionary<string, int>
        {
            // { "mednafen_psx_hw_libretro.dll", 0x200000 },
            { "mednafen_psx_libretro.dll", 0x200000 }, // SwanStation uses the same size
            { "pcsx_rearmed_libretro.dll", 0x210000 }
        };

        var WRAMbases = new Dictionary<string, IntPtr>();

        foreach (var entry in SupportedCored)
            WRAMbases.Add(entry.Key, game.MemoryPages(true).FirstOrDefault(p => p.Type == MemPageType.MEM_MAPPED && (int)p.RegionSize == entry.Value).BaseAddress);

        IntPtr WRAMbase = WRAMbases.Values.First(b => !b.IsZero());

        Func<bool> checkIfAlive = () => game.ReadBytes(WRAMbase, 1, out _);

        Debugs.Info("  => Hooked to emulator: Retroarch");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, checkIfAlive);

        /*
        int pageSize;

        var SupportedCores = new string[] { "mednafen_psx_hw_libretro.dll", "mednafen_psx_libretro.dll", "pcsx_rearmed_libretro.dll" };

        var CurrentCore = game.ModulesWow64Safe().FirstOrDefault(c => SupportedCores.Contains(c.ModuleName));
        if (CurrentCore == null)
            throw new NullReferenceException();

        switch (CurrentCore.ModuleName)
        {
            case "mednafen_psx_hw_libretro.dll":
            case "mednafen_psx_libretro.dll":
                pageSize = 0x200000;
                break;
            case "pcsx_rearmed_libretro.dll":
                pageSize = 0x210000;
                break;
            default:
                throw new NullReferenceException();
        }

        IntPtr WRAMbase = game.MemoryPages(true).First(p => p.Type == MemPageType.MEM_MAPPED && (int)p.RegionSize == pageSize).BaseAddress;

        Func<bool> checkIfAlive = () => game.ReadBytes(WRAMbase, 1, out _);

        Debugs.Info("  => Hooked to emulator: Retroarch");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, checkIfAlive);
        */
    }
}

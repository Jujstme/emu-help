using System;
using System.Collections.Generic;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class PS2
{
    private Tuple<IntPtr, Func<bool>> Retroarch()
    {
        var SupportedCores = new Dictionary<string, int>
        {
            { "pcsx2_libretro.dll", 0x2639000 }, 
        };

        game.ResetModulesWow64Cache();
        ProcessModuleWow64Safe CurrentCore = game.ModulesWow64Safe().First(m => SupportedCores.Keys.Contains(m.ModuleName.ToLower()));

        IntPtr WRAMbase = game.MemoryPages(true).First(p => p.AllocationProtect == MemPageProtect.PAGE_NOACCESS && (int)p.RegionSize == SupportedCores[CurrentCore.ModuleName.ToLower()]).BaseAddress;

        bool checkIfAlive() => game.ReadBytes(CurrentCore.BaseAddress, 1, out _);

        Debugs.Info("  => Hooked to emulator: Retroarch");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, checkIfAlive);
    }
}
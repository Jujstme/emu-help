using System;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class PS1
{
    private Tuple<IntPtr, Func<bool>> Retroarch()
    {
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

        IntPtr WRAMbase = game.MemoryPages(true).FirstOrDefault(p => p.Type == MemPageType.MEM_MAPPED && (int)p.RegionSize == pageSize).BaseAddress;
        LiveSplit.EMUHELP.ExtensionMethods.CheckPtr(WRAMbase);

        Func<bool> checkIfAlive = () => game.ReadBytes(WRAMbase, 0, out _);

        Debugs.Info("  => Hooked to emulator: Retroarch");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, checkIfAlive);
    }
}

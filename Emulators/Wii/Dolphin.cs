using System;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class Wii
{
    public Tuple<IntPtr, IntPtr, Func<bool>> Dolphin()
    {
        IntPtr MEM1 = game.MemoryPages(true).First(p => p.Type == MemPageType.MEM_MAPPED && p.State == MemPageState.MEM_COMMIT && (int)p.RegionSize == 0x2000000).BaseAddress;
        IntPtr MEM2 = game.MemoryPages(true).First(p => p.Type == MemPageType.MEM_MAPPED && p.State == MemPageState.MEM_COMMIT && (int)p.RegionSize == 0x4000000).BaseAddress;


        bool checkIfAlive() => game.ReadBytes(MEM1, 1, out _);

        Endianess = Endianess.BigEndian;

        Debugs.Info("  => Hooked to emulator: Dolphin");
        Debugs.Info($"  => MEM1 address found at 0x{MEM1.ToString("X")}");
        Debugs.Info($"  => MEM2 address found at 0x{MEM2.ToString("X")}");

        return Tuple.Create(MEM1, MEM2, (Func<bool>)checkIfAlive);
    }
}

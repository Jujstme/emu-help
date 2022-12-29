using System;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class GCN
{
    public Tuple<IntPtr, Func<bool>> Dolphin()
    {
        IntPtr MEM1 = game.MemoryPages(true).FirstOrDefault(p => p.Type == MemPageType.MEM_MAPPED && p.State == MemPageState.MEM_COMMIT && (int)p.RegionSize == 0x2000000).BaseAddress;
        LiveSplit.EMUHELP.ExtensionMethods.CheckPtr(MEM1);

        bool checkIfAlive() => game.ReadBytes(MEM1, 1, out _);
        
        IsBigEndian = true;

        Debugs.Info("  => Hooked to emulator: Dolphin");
        Debugs.Info($"  => MEM1 address found at 0x{MEM1.ToString("X")}");

        return Tuple.Create(MEM1, (Func<bool>)checkIfAlive);
    }
}

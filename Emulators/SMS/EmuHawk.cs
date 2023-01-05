using System;
using System.Collections.Generic;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class SMS
{
    private Tuple<IntPtr, Func<bool>> EmuHawk()
    {
        IntPtr entryPoint = IntPtr.Zero;
        IntPtr entryPoint2 = IntPtr.Zero;
        IntPtr WRAMbase = IntPtr.Zero;
        Func<bool> checkIfAlive = () => true;

        foreach (var page in game.MemoryPages())
        {
            entryPoint = new SignatureScanner(game, page.BaseAddress, (int)page.RegionSize).Scan(new SigScanTarget("45 8B C8 41 81 E1 FF 1F 00 00 44 3B 48 08 73 ?? 4D"));
            entryPoint2 = new SignatureScanner(game, page.BaseAddress, (int)page.RegionSize).Scan(new SigScanTarget("53 48 BB ???????? ???????? FF D3 5B 73"));

            if (!entryPoint.IsZero() || !entryPoint2.IsZero())
                break;
        }

        if ((entryPoint.IsZero() && entryPoint2.IsZero()) || (!entryPoint.IsZero() && !entryPoint2.IsZero()))
            throw new Exception();

        if (!entryPoint.IsZero())
        {
            var injection = new List<byte>();
            byte[] origbytes = game.ReadBytes(entryPoint, 14);

            injection.AddRange(new byte[] { 0x48, 0x83, 0xC0, 0x10, 0x48, 0x89, 0x05, 0x15, 0x0, 0x0, 0x0, 0x48, 0x83, 0xE8, 0x10 });
            injection.AddRange(origbytes);
            injection.Add(0xC3);
            IntPtr targetAddr = game.AllocateMemory(0x1000);
            game.WriteBytes(targetAddr, injection.ToArray());

            injection.Clear();
            injection.AddRange(new byte[] { 0x53, 0x48, 0xBB });
            injection.AddRange(BitConverter.GetBytes((long)targetAddr));
            injection.AddRange(new byte[] { 0xFF, 0xD3, 0x5B });
            game.WriteBytes(entryPoint, injection.ToArray());

            WRAMbase = game.ReadPointer(targetAddr + 0x20);
            checkIfAlive = () => game.ReadPointer(targetAddr + 0x20) == WRAMbase;
        }

        else if (!entryPoint2.IsZero())
        {
            var temp = game.ReadPointer(entryPoint2 + 3) + 0x20;
            WRAMbase = game.ReadPointer(temp);
            checkIfAlive = () => game.ReadPointer(temp) == WRAMbase;
        }


        Debugs.Info("  => Hooked to emulator: EmuHawk / BizHawk");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, checkIfAlive);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public partial class SMS
{
    private Tuple<IntPtr, Func<bool>> Retroarch()
    {
        var supportedCores = new string[]
        {
            "genesis_plus_gx_libretro.dll",
            "genesis_plus_gx_wide_libretro.dll",
            "picodrive_libretro.dll",
            "smsplus_libretro.dll",
            "gearsystem_libretro.dll",
        };

        game.ResetModulesWow64Cache();
        ProcessModuleWow64Safe CurrentCore = game.ModulesWow64Safe().First(m => supportedCores.Contains(m.ModuleName));

        IntPtr WRAMbase = IntPtr.Zero;
        IntPtr targetAddr = IntPtr.Zero;

        switch (CurrentCore.ModuleName.ToLower())
        {
            case "genesis_plus_gx_libretro.dll":
            case "genesis_plus_gx_wide_libretro.dll":
                WRAMbase = game.Is64Bit()
                    ? game.SigScan(CurrentCore).ScanOrThrow(new SigScanTarget(3, "48 8D 0D ???????? 4C 8B 2D ????????") { OnFound = (p, s, addr) => addr + 0x4 + p.ReadValue<int>(addr) })
                    : game.SigScan(CurrentCore).ScanOrThrow(new SigScanTarget(1, "A3 ???????? 29 F9") { OnFound = (p, s, addr) => p.ReadPointer(addr) });
                break;

            case "picodrive_libretro.dll":
                WRAMbase = game.Is64Bit()
                    ? game.SigScan(CurrentCore).ScanOrThrow(new SigScanTarget(3, "48 8D 0D ???????? 41 B8 ????????") { OnFound = (p, s, addr) => addr + 0x4 + p.ReadValue<int>(addr) })
                    : game.SigScan(CurrentCore).ScanOrThrow(new SigScanTarget(1, "B9 ???????? C1 EF 10") { OnFound = (p, s, addr) => p.ReadPointer(addr) });
                WRAMbase += 0x20000;
                break;

            case "smsplus_libretro.dll":
                WRAMbase = game.Is64Bit()
                    ? game.SigScan(CurrentCore).ScanOrThrow(new SigScanTarget(5, "31 F6 48 C7 05") { OnFound = (p, s, addr) => addr + 0x8 + p.ReadValue<int>(addr) })
                    : game.SigScan(CurrentCore).ScanOrThrow(new SigScanTarget(4, "83 FA 02 B8") { OnFound = (p, s, addr) => p.ReadPointer(addr) });
                break;

            case "gearsystem_libretro.dll":
                IntPtr entryPoint;
                switch (game.Is64Bit())
                {
                    case true:
                        entryPoint = game.SigScan(CurrentCore).ScanOrThrow(new SigScanTarget(1, "77 2B 0F B7 DA") { OnFound = (p, s, addr) => addr + p.ReadValue<byte>(addr) + 1 });
                        switch (game.ReadValue<byte>(entryPoint))
                        {
                            case 0x51: // process already injected
                                WRAMbase = game.ReadPointer(game.ReadPointer(entryPoint + 3) + 0x30);
                                WRAMbase.ThrowIfZero();
                                break;

                            case 0x48: // process not yet injected
                                var injection = new List<byte>();
                                byte[] origBytes = game.ReadBytes(entryPoint, 15);

                                injection.AddRange(origBytes.Take(11));
                                injection.AddRange(new byte[] { 0x48, 0x05, 0x00, 0xC0, 0x00, 0x00, 0x48, 0x89, 0x05, 0x18, 0x0, 0x0, 0x0, 0x48, 0x2D, 0x00, 0xC0, 0x00, 0x00 });
                                injection.AddRange(origBytes.Skip(11));
                                injection.Add(0xC3);
                                targetAddr = game.AllocateMemory(0x1000);
                                game.WriteBytes(targetAddr, injection.ToArray());

                                injection.Clear();
                                injection.AddRange(new byte[] { 0x51, 0x48, 0xB9 });
                                injection.AddRange(BitConverter.GetBytes((long)targetAddr));
                                injection.AddRange(new byte[] { 0xFF, 0xD1, 0x59, 0x90 });
                                game.WriteBytes(entryPoint, injection.ToArray());

                                Thread.Sleep(50);
                                WRAMbase = game.ReadPointer(targetAddr + 0x30);
                                WRAMbase.ThrowIfZero();
                                break;

                            default:
                                throw new Exception();
                        }
                        break;

                    default:
                        entryPoint = game.SigScan(CurrentCore).ScanOrThrow(new SigScanTarget(1, "77 ?? 0F B7 DB 81 EB") { OnFound = (p, s, addr) => addr + p.ReadValue<byte>(addr) + 4 });
                        switch (game.ReadValue<byte>(entryPoint))
                        {
                            case 0x0F: // Not yet injected
                                var injection = new List<byte>();
                                byte[] origBytes = game.ReadBytes(entryPoint, 6);
                                targetAddr = game.AllocateMemory(0x1000);

                                injection.AddRange(origBytes);
                                injection.AddRange(new byte[] { 0x5, 0x0, 0xC0, 0x0, 0x0, 0xA3 });
                                injection.AddRange(BitConverter.GetBytes((int)targetAddr + 0x20));
                                injection.AddRange(new byte[] { 0x2D, 0x0, 0xC0, 0x0, 0x0, 0xC3 });
                                game.WriteBytes(targetAddr, injection.ToArray());

                                injection.Clear();
                                injection.Add(0xE8);
                                injection.AddRange(BitConverter.GetBytes((int)((long)targetAddr - (long)entryPoint - 5)));
                                injection.Add(0x90);
                                game.WriteBytes(entryPoint, injection.ToArray());

                                Thread.Sleep(50);
                                WRAMbase = game.ReadPointer(targetAddr + 0x20);
                                WRAMbase.ThrowIfZero();
                                break;

                            case 0xE8: // Already injected
                                entryPoint += 1;
                                WRAMbase = game.ReadPointer(entryPoint + game.ReadValue<int>(entryPoint) + 0x24);
                                WRAMbase.ThrowIfZero();
                                break;

                            default:
                                throw new Exception();
                        }
                        break;
                }
                break;

            default:
                throw new Exception();
        }

        Func<bool> checkIfAlive = () =>
        {
            var success = game.ReadBytes(CurrentCore.BaseAddress, 1, out _);
            
            if (success)
                return true;
            else
            {
                if (!targetAddr.IsZero())
                    game?.FreeMemory(targetAddr);

                return false;
            }
        };


        Debugs.Info("  => Hooked to emulator: Retroarch");
        Debugs.Info($"  => WRAM address found at 0x{WRAMbase.ToString("X")}");

        return Tuple.Create(WRAMbase, checkIfAlive);
    }
}
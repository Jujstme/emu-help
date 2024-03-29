﻿using System;
using System.Runtime.InteropServices;
using LiveSplit.EMUHELP;
using LiveSplit.EMUHELP.PS2;

public class Playstation2 : PS2 { }

public class PS2 : HelperBase
{
    public PS2(bool generateCode) : base(generateCode)
    {
        var ProcessNames = new string[]
        {
            "pcsx2x64",
            "pcsx2-qt",
            "pcsx2x64-avx2",
            "pcsx2-avx2",
            "pcsx2",
            "retroarch",
        };

        GameProcess = new ProcessHook(ProcessNames);
        Debugs.Info("  => PS2 Helper started");
    }

    public PS2()
        : this(true) { }

    protected override void InitActions()
    {
        Emu = Game.ProcessName.ToLower() switch {
            "pcsx2x64" or "pcsx2-qt" or "pcsx2x64-avx2" or "pcsx2-avx2" or "pcsx2" => new PCSX2(this),
            "retroarch" => new Retroarch(this),
            _ => throw new NotImplementedException(),
        };
    }

    internal override bool IsAddressInBounds<T>(ulong address)
    {
        var size = address + (ulong)Marshal.SizeOf<T>();
        return address >= 0x00100000 && size <= 0x02000000;
    }

    internal override bool IsStringAddressInBounds(ulong address, int stringLength)
    {
        var size = address + (ulong)stringLength;
        return address >= 0x00100000 && size <= 0x02000000;
    }

    public override bool TryGetAddress(ulong address, out IntPtr realAddress)
    {
        realAddress = default;

        if (Emu.GetMemoryAddress(0) == null)
            return false;

        if (address < 0x00100000 || address > 0x01FFFFFF)
            return false;

        realAddress = (IntPtr)((ulong)Emu.GetMemoryAddress(0) + address);
        return true;
    }
}
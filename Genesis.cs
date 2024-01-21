using System;
using System.Runtime.InteropServices;
using System.Text;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;
using LiveSplit.EMUHELP.Genesis;

public class Megadrive : Genesis { }

public partial class Genesis : HelperBase
{
    private GenesisBase emu { get; set; }
    private IntPtr ram_base => emu.ram_base;
    internal override Endianness.Endian Endian => emu.Endian;

    public Genesis(bool generateCode) : base(generateCode)
    {
        var ProcessNames = new string[]
        {
            "retroarch",
            "SEGAGameRoom",
            "SEGAGenesisClassics",
            "Fusion",
            "gens",
            "blastem",
        };

        GameProcess = new ProcessHook(ProcessNames);
        Debugs.Info("  => Genesis Helper started");
    }

    public Genesis()
        : this(true) { }

    protected override void InitActions()
    {
        emu = game.ProcessName switch
        {
            "retroarch" => new Retroarch(this),
            "SEGAGameRoom" or "SEGAGenesisClassics" => new SegaClassics(this),
            "Fusion" => new Fusion(this),
            "gens" => new Gens(this),
            "blastem" => new BlastEm(this),
            _ => throw new NotImplementedException(),
        };

        KeepAlive = emu.KeepAlive;
        MakeWatchers();
    }

    internal override bool IsAddressInBounds<T>(ulong address)
    {
        var defOffset = address;

        if ((defOffset > 0xFFFF && defOffset < 0xFF0000) || defOffset > 0xFFFFFF)
            return false;
        else if (defOffset >= 0xFF0000 && defOffset <= 0xFFFFFF)
            defOffset -= 0xFF0000;

        return defOffset + (ulong)Marshal.SizeOf<T>() <= 0x10000;
    }

    internal override bool IsStringAddressInBounds(ulong address, int stringLength)
    {
        var defOffset = address;

        if ((defOffset > 0xFFFF && defOffset < 0xFF0000) || defOffset > 0xFFFFFF)
            return false;
        else if (defOffset >= 0xFF0000 && defOffset <= 0xFFFFFF)
            defOffset -= 0xFF0000;

        return defOffset + (ulong)stringLength <= 0x10000;
    }

    public override bool TryGetAddress(ulong address, out IntPtr realAddress)
    {
        realAddress = default;
     
        if (ram_base == null)
            return false;

        var defOffset = address;

        if ((defOffset > 0xFFFF && defOffset < 0xFF0000) || defOffset > 0xFFFFFF)
            return false;
        else if (defOffset >= 0xFF0000 && defOffset <= 0xFFFFFF)
            defOffset -= 0xFF0000;

        realAddress = (IntPtr)((ulong)ram_base + defOffset);
        return true;
    }

    public override bool TryReadValue<T>(ulong address, out T value, params uint[] offsets)
    {
        value = default;

        if (ram_base == null || !IsAddressInBounds<T>(address))
            return false;

        var alignedOffset = (int)address & ~1;
        if (!TryGetAddress((ulong)alignedOffset, out IntPtr realAddress))
            return false;

        int size = Marshal.SizeOf<T>();
        int misalignment = (int)(address & 1);

        var f_size = size + misalignment;
        if ((f_size & 1) != 0)
            f_size++;

        if (game.ReadBytes(realAddress, f_size, out var buf))
        {
            if (this.Endian == Endianness.Endian.Little)
            {
                for (int i = 0; i < buf.Length; i += 2)
                    (buf[i + 1], buf[i]) = (buf[i], buf[i + 1]);
            }

            if (buf.TryConvertTo<T>(misalignment, out T tempValue))
            {
                value = tempValue.FromEndian(Endian);
                return true;
            }

        }

        return default;
    }
    
    public override bool TryReadString(ulong address, int length, out string value, params uint[] offsets)
    {
        value = default;

        if (ram_base == null || !IsStringAddressInBounds(address, length))
            return false;

        var alignedOffset = (int)address & ~1;
        if (!TryGetAddress((ulong)alignedOffset, out IntPtr realAddress))
            return false;

        int misalignment = (int)(address & 1);

        var f_size = length + misalignment;
        if ((f_size & 1) != 0)
            f_size++;

        if (game.ReadBytes(realAddress, f_size, out var buf))
        {
            if (this.Endian == Endianness.Endian.Little)
            {
                for (int i = 0; i < buf.Length; i += 2)
                    (buf[i + 1], buf[i]) = (buf[i], buf[i + 1]);
            }
            value = Encoding.UTF8.GetString(buf, 0, length);
            return true;
        }

        return default;
    }
}
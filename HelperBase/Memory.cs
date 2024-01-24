using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public abstract partial class HelperBase
{
    /// <summary>
    /// Converts a provided emulator address to a real memory address in the emulator's virtual address space.
    /// </summary>
    /// <param name="address">Address to get real physical address of.</param>
    /// <returns><see cref="IntPtr.Zero"/> on failure, else the real memory address.</returns>
    public abstract bool TryGetAddress(ulong address, out IntPtr realAddress);

    /// <summary>
    /// Converts a provided emulator address to a real memory address in the emulator's virtual address space.
    /// </summary>
    /// <param name="address">Address to get real physical address of.</param>
    /// <returns><see cref="IntPtr.Zero"/> on failure, else the real memory address.</returns>
    public IntPtr GetAddress(ulong address) => TryGetAddress(address, out IntPtr realAddress) ? realAddress : IntPtr.Zero;

    /// <summary>
    /// Checks whether reading a certain T is limited inside the bounds of the emulated system's memory range.
    /// </summary>
    /// <param name="address">Address to get real physical address of.</param>
    /// <returns><see cref="true"/> if the T and the address provided stay in bounds, false otherwise.</returns>
    internal abstract bool IsAddressInBounds<T>(ulong address);

    internal abstract bool IsStringAddressInBounds(ulong address, int stringLength);

    public virtual bool TryReadValue<T>(ulong address, out T value, params uint[] offsets) where T : unmanaged
    {
        value = default;

        if (!TryGetAddress(address, out IntPtr realAddress) || !IsAddressInBounds<T>(address))
            return false;

        if (offsets.Length != 0)
        {
            if (!TryReadValue<uint>(address, out uint tempOffset))
                return false;

            for (int i = 0; i < offsets.Length - 1; i++)
            {
                if (!TryReadValue<uint>(tempOffset + offsets[i], out tempOffset))
                    return false;
            }

            if (!TryGetAddress(tempOffset + offsets[offsets.Length - 1], out realAddress))
                return false;
        }

        if (!Game.ReadValue<T>(realAddress, out value))
            return false;

        value = value.FromEndian(Emu.Endian);
        return true;
    }

    public T ReadValue<T>(ulong address, params uint[] offsets) where T : unmanaged => TryReadValue<T>(address, out T value, offsets) ? value : default;


    public virtual bool TryReadString(ulong address, int length, out string value, params uint[] offsets)
    {
        value = default;

        if (!TryGetAddress(address, out IntPtr realAddress) || !IsStringAddressInBounds(address, length))
            return false;

        if (offsets.Length != 0)
        {
            if (!TryReadValue<uint>(address, out uint tempOffset))
                return false;

            for (int i = 0; i < offsets.Length - 1; i++)
            {
                if (!TryReadValue<uint>(tempOffset, out tempOffset))
                    return false;
            }

            if (!TryGetAddress(tempOffset + offsets[offsets.Length - 1], out realAddress))
                return false;
        }

        return Game.ReadString(realAddress, length, out value);
    }

    public string ReadString(ulong address, int length, params uint[] offsets) => TryReadString(address, length, out string value) ? value : default;
}

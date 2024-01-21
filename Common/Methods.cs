using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using EMUHELP.Extensions;
using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP
{
    /// <summary>
    /// Custom extension methods.
    /// </summary>
    public static class EmuHelpExtensionMethods
    {
        /// <summary>
        /// Perform a signature scan, similarly to how it would achieve with SignatureScanner.Scan()
        /// </summary>
        /// <returns>An <see cref="IntPtr"/> conteining the address of the signature, if found.</returns>
        /// <exception cref="SigscanFailedException"></exception>
        public static IntPtr ScanOrThrow(this SignatureScanner scanner, SigScanTarget target, int align = 1)
        {
            IntPtr tempAddr = scanner.Scan(target, align);
            tempAddr.ThrowIfZero();
            return tempAddr;
        }

        /// <summary>
        /// Checks whether a provided IntPtr is equal to IntPtr.Zero. If it is, an Exception will be thrown.
        /// </summary>
        /// <param name="ptr"></param>
        /// <exception cref="SigscanFailedException"></exception>
        public static void ThrowIfZero(this IntPtr ptr)
        {
            if (ptr.IsZero())
                throw new SigscanFailedException();
        }

        /// <summary>
        /// Checks whether a specific bit inside a byte value is set or not.
        /// </summary>
        /// <param name="value">The <see cref="byte"/> value in which to perform the check</param>
        /// <param name="bitPos">The bit position (from 0 to 7).</param>
        /// <returns>True if the bit is set, otherwise false.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Will be thrown if a bit was specified in a position outside the allowed interval.</exception>
        public static bool BitCheck(this byte value, byte bitPos)
        {
            if (bitPos < 0 || bitPos > 7)
                throw new ArgumentOutOfRangeException();
            return (value & (1 << bitPos)) != 0;
        }

        /// <summary>
        /// Checks if a provided <see cref="IntPtr"/> is equal to <see cref="IntPtr.Zero"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True if the value is <see cref="IntPtr.Zero"/>, false otherwise.</returns>
        public static bool IsZero(this IntPtr value) => value == IntPtr.Zero;

        /// <summary>
        /// Quickly creates a new <see cref="SignatureScanner"/> object. If no argument is specified, sigscanning will be performed in the <see cref="Process.MainModule"/> memory space.
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public static SignatureScanner SigScanner(this Process process)
        {
            return new SignatureScanner(process, process.MainModuleWow64Safe().BaseAddress, process.MainModuleWow64Safe().ModuleMemorySize);
        }

        /// <summary>
        /// Quickly creates a new <see cref="SignatureScanner"/> object for the address space of the specified <see cref="ProcessModuleWow64Safe"/>.
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public static SignatureScanner SigScanner(this Process process, ProcessModuleWow64Safe module)
        {
            return new SignatureScanner(process, module.BaseAddress, module.ModuleMemorySize);
        }

        /// <summary>
        /// Quickly creates a new <see cref="SignatureScanner"/> object for the address space of the specified <see cref="ProcessModule"/>.
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public static SignatureScanner SigScanner(this Process process, ProcessModule module)
        {
            return new SignatureScanner(process, module.BaseAddress, module.ModuleMemorySize);
        }

        /// <summary>
        /// "Safe" version of the signature scanner for problematic process in which the standard
        /// sigscanning function might fail due to memory access restrictions.
        /// </summary>
        /// <param name="process">The target <see cref="Process"/> in which to perform the signature scan.</param>
        /// <param name="scanTarget">A <see cref="SigScanTarget"/> instance containing the memory range in which to perform the pattern scanning.</param>
        /// <returns>An <see cref="IntPtr"/> with the address of the found signature, <see cref="IntPtr.Zero"/> if the scan fails or if the signature is not found.</returns>
        public static IntPtr SafeSigScan(this Process process, SigScanTarget scanTarget)
        {
            IntPtr ptr = default;

            var mainModuleBase = (long)process.MainModuleWow64Safe().BaseAddress;
            var mainModuleEndAddress = (long)mainModuleBase + process.MainModuleWow64Safe().ModuleMemorySize;

            foreach (var entry in process.MemoryPages(true).Where(p => (long)p.BaseAddress >= mainModuleBase && (long)p.BaseAddress < mainModuleEndAddress))
            {
                var scanner = new SignatureScanner(process, entry.BaseAddress, (int)entry.RegionSize);
                ptr = scanner.Scan(scanTarget);
                if (!ptr.IsZero())
                    break;
            }

            return ptr;
        }

        /// <summary>
        /// "Safe" version of the signature scanner for problematic process in which the standard
        /// sigscanning function might fail due to memory access restrictions.
        /// </summary>
        /// <param name="process">The target <see cref="Process"/> in which to perform the signature scan.</param>
        /// <param name="scanTarget">A <see cref="SigScanTarget"/> instance containing the memory range in which to perform the pattern scanning.</param>
        /// <returns>An <see cref="IntPtr"/> with the address of the found signature. If the scan fails or if the signature is not found, a <see cref="SigscanFailedException"/> is thrown.</returns>
        /// <exception cref="SigscanFailedException"></exception>
        public static IntPtr SafeSigScanOrThrow(this Process process, SigScanTarget scanTarget)
        {
            var ptr = process.SafeSigScan(scanTarget);
            if (ptr.IsZero())
                throw new SigscanFailedException();
            return ptr;
        }

        /// <summary>
        /// Enumerates through the debug symbols defined in the PE module specified.
        /// </summary>
        /// <param name="module">A ProcessModuleWow64Safe with a valid PE header</param>
        /// <param name="process">The parent Process of the specified module</param>
        /// <returns></returns>
        public static IEnumerable<Symbol> GetSymbols(this ProcessModuleWow64Safe module, Process process)
        {
            IntPtr baseAddress = module.BaseAddress;
            bool is64Bit = process.Is64Bit();

            if (process.ReadValue(baseAddress + 0x3C, out int e_lfanew)
                && process.ReadValue(baseAddress + e_lfanew + (is64Bit ? 0x88 : 0x78), out int exportDir)
                && exportDir != 0)
            {
                if (process.ReadValue(baseAddress + exportDir, out Symbol.ExportedSymbolsTableDef symbolsDef))
                {
                    for (int i = 0; i < symbolsDef.numberOfFunctions; i++)
                    {
                        if (process.ReadValue(baseAddress + symbolsDef.functionAddressArrayIndex + i * 4, out int addr)
                            && process.ReadValue(baseAddress + symbolsDef.functionNameArrayIndex + i * 4, out int nameAddr)
                            && process.ReadString(baseAddress + nameAddr, 255, out string name))
                        {
                            yield return new Symbol(baseAddress + addr, name);
                        }
                    }
                }
            }
        }

        public static void ResetModulesWow64Cache(this Process process)
        {
            process.Refresh();
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            //process.SetFieldValue<Dictionary<int, ProcessModuleWow64Safe[]>>("ModuleCache", new(), bindingFlags);
            typeof(ExtensionMethods).GetField("ModuleCache", bindingFlags).SetValue(null, new Dictionary<int, ProcessModuleWow64Safe[]>());
        }

        /// <summary>
        /// Reinterprets a certain byte array to any <see cref="T"/>. This function succeeds as long as the byte array provided is long enough to contain the type T you're trying to convert into.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="offset"></param>
        /// <returns>True if successful, false otherwise.</returns>
        public static bool TryConvertTo<T>(this byte[] obj, int offset, out T value) where T: unmanaged
        {
            value = default(T);
            if (offset + Marshal.SizeOf<T>() > obj.Length)
                return false;
            GCHandle handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
            value = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject() + offset, typeof(T));
            handle.Free();
            return true;
        }

        /// <summary>
        /// Reinterprets a certain byte array to any <see cref="T"/>. This function succeeds as long as the byte array provided is long enough to contain the type T you're trying to convert into.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="offset"></param>
        /// <returns>A value of type T containing the same bytes as the original input, <see cref="default(T)"/> otherwise.</returns>
        public static T ConvertTo<T>(this byte[] obj, int offset) where T : unmanaged
        {
            return TryConvertTo<T>(obj, offset, out T value) switch
            {
                true => value,
                false => default(T),
            };
        }
    }

    public class SigscanFailedException : Exception
    {
        /// <summary>
        /// Indicates a missing or not found byte pattern inside the target memory scanned.
        /// </summary>
        public SigscanFailedException() { }
        public SigscanFailedException(string message) : base(message) { }
    }

    /// <summary>
    /// Tools for dealing with Endianess
    /// </summary>
    public static class Endianness
    {
        /// <summary>
        /// Converts any <see cref="UnmanagedType"/> by swapping its endianness. It's essentially a no-op if the value provided is specified to already be <see cref="Endian.Little"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="endian"></param>
        /// <returns></returns>
        public static T FromEndian<T>(this T value, Endian endian) where T : unmanaged
        {
            if (endian == Endian.Little)
                return value;

            var rawSize = Marshal.SizeOf<T>();

            if (rawSize == 1)
                return value;

            byte[] buffer = new byte[rawSize];

            for (int i = 0; i < rawSize; i++)
                buffer[i] = Marshal.ReadByte(value, rawSize - 1 - i);

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T newValue = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return newValue;
        }

        public enum Endian
        {
            Little,
            Big,
        }
    }
}


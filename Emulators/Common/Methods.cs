using System;
using System.Runtime.InteropServices;
using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP
{
    /// <summary>
    /// Custom extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Perform a signature scan, similarly to how it would achieve with SignatureScanner.Scan()
        /// </summary>
        /// <returns>Address of the signature, if found. Otherwise, an Exception will be thrown.</returns>
        public static IntPtr ScanOrThrow(this SignatureScanner scanner, SigScanTarget target, int align = 1)
        {
            IntPtr tempAddr = scanner.Scan(target, align);
            CheckPtr(tempAddr);
            return tempAddr;
        }

        /// <summary>
        /// Checks whether a provided IntPtr is equal to IntPtr.Zero. If it is, an Exception will be thrown.
        /// </summary>
        /// <param name="ptr"></param>
        /// <exception cref="SigscanFailedException"></exception>
        public static void CheckPtr(this IntPtr ptr)
        {
            if (ptr.IsZero())
                throw new SigscanFailedException();
        }

        /// <summary>
        /// Checks is a specific bit inside a byte value is set or not.
        /// </summary>
        /// <param name="value">The byte value in which to perform the check</param>
        /// <param name="bitPos">The bit position. Can range from 0 to 7: any value outside this range will make the function automatically return false.</param>
        /// <returns></returns>
        public static bool BitCheck(this byte value, byte bitPos)
        {
            return bitPos >= 0 && bitPos <= 7 && (value & (1 << bitPos)) != 0;
        }

        /// <summary>
        /// Checks if a provided IntPtr value is equal to IntPtr.Zero
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True is the value is IntPtr.Zero, false otherwise.</returns>
        public static bool IsZero(this IntPtr value)
        {
            return value == IntPtr.Zero;
        }
    }

    public class SigscanFailedException : Exception
    {
        public SigscanFailedException() { }
        public SigscanFailedException(string message) : base(message) { }
    }

    /// <summary>
    /// Tools for dealing with Endianess
    /// </summary>
    public static class ToLittleEndian
    {
        public static T SwapEndian<T>(this T value)
        {
            if (value is string || value is bool || value is byte || value is sbyte)
                return value;

            int rawSize = Marshal.SizeOf(value);

            if (rawSize == 1)
                return value;

            dynamic output = 0;

            for (int i = 0; i < rawSize; i++)
                output |= Marshal.ReadByte(value, rawSize - 1 - i) << (i * 8);

            return output;
        }

        /// <summary>
        /// Creates a new FakeMemoryWatcherList from an existing MemoryWatcherList, with each element having it's Current and Old properties with switched endianess.
        /// </summary>
        /// <param name="Watchers">A MemoryWatcherList with elements we want to convert from Big Endian to Little Endian (or vice versa).</param>
        /// <returns></returns>
        public static FakeMemoryWatcherList SetFakeWatchers(MemoryWatcherList Watchers)
        {
            var list = new FakeMemoryWatcherList();

            foreach (var entry in Watchers)
                list.Add(new FakeMemoryWatcher<dynamic>(() => entry.Current.SwapEndian<dynamic>()) { Name = entry.Name });

            return list;
        }
    }
}
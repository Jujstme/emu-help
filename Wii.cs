using System;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public class WII : Wii { }

public partial class Wii : EmuBase
{
    public new Func<IntPtr, IntPtr, MemoryWatcherList> Load { get; set; }
    public string[] Gamecodes { get; set; }
    private IntPtr MEM1 { get; set; }
    private IntPtr MEM2 { get; set; }
    public new FakeMemoryWatcherList Watchers { get; protected set; } = new();
    public new FakeMemoryWatcher this[string index] => Endianess == Endianess.BigEndian ? LittleEndianWatchers[index] : this.Watchers[index];

    public Wii()
    {
        EmulatorNames = new string[]
        {
            "Dolphin",
        };

        GameProcess = new ProcessHook(EmulatorNames);
    }

    public new bool Update()
    {
        if (Load == null || !base.Init())
            return false;

        if (!KeepAlive())
        {
            GameProcess.InitStatus = GameInitStatus.NotStarted;
            return false;
        }

        Watchers.UpdateAll();

        if (Endianess == Endianess.BigEndian)
            LittleEndianWatchers.UpdateAll();

        return true;
    }

    protected override void InitActions()
    {
        var Init = game.ProcessName.ToLower() switch
        {
            "dolphin" => Dolphin(),
            _ => throw new NotImplementedException(),
        };

        MEM1 = Init.Item1;
        MEM2 = Init.Item2;
        KeepAlive = Init.Item3;

        var tempWatchers = Load(MEM1, MEM2);

        Watchers = new();
        WiiDeepPointer wdpr;
        
        // For each watcher this checks whether it's using DeepPointer.
        // If it is, it converts the DeepPointer to a new EmuDeepPointer
        foreach (var watcher in tempWatchers)
        {
            var addrType = watcher.GetProperty<int>("AddrType");
            
            // If addrType != 0, then it's a standard MemoryWatcher
            if (addrType != 0)
            {
                var address = watcher.GetProperty<IntPtr>("Address");
                wdpr = new WiiDeepPointer(MEM1, MEM2, address);
                switch (watcher)
                {
                    case MemoryWatcher<byte>: Watchers.Add(new FakeMemoryWatcher<byte>(() => wdpr.Deref<byte>(game)) { Name = watcher.Name }); break;
                    case MemoryWatcher<sbyte>: Watchers.Add(new FakeMemoryWatcher<sbyte>(() => wdpr.Deref<sbyte>(game)) { Name = watcher.Name }); break;
                    case MemoryWatcher<bool>: Watchers.Add(new FakeMemoryWatcher<bool>(() => wdpr.Deref<bool>(game)) { Name = watcher.Name }); break;
                    case MemoryWatcher<short>: Watchers.Add(new FakeMemoryWatcher<short>(() => wdpr.Deref<short>(game)) { Name = watcher.Name }); break;
                    case MemoryWatcher<ushort>: Watchers.Add(new FakeMemoryWatcher<ushort>(() => wdpr.Deref<ushort>(game)) { Name = watcher.Name }); break;
                    case MemoryWatcher<int>: Watchers.Add(new FakeMemoryWatcher<int>(() => wdpr.Deref<int>(game)) { Name = watcher.Name }); break;
                    case MemoryWatcher<uint>: Watchers.Add(new FakeMemoryWatcher<uint>(() => wdpr.Deref<uint>(game)) { Name = watcher.Name }); break;
                    case MemoryWatcher<long>: Watchers.Add(new FakeMemoryWatcher<long>(() => wdpr.Deref<long>(game)) { Name = watcher.Name }); break;
                    case MemoryWatcher<ulong>: Watchers.Add(new FakeMemoryWatcher<ulong>(() => wdpr.Deref<ulong>(game)) { Name = watcher.Name }); break;
                    case MemoryWatcher<float>: Watchers.Add(new FakeMemoryWatcher<float>(() => wdpr.Deref<float>(game)) { Name = watcher.Name }); break;
                    case MemoryWatcher<double>: Watchers.Add(new FakeMemoryWatcher<double>(() => wdpr.Deref<double>(game)) { Name = watcher.Name }); break;
                    case MemoryWatcher<char>: Watchers.Add(new FakeMemoryWatcher<char>(() => wdpr.Deref<char>(game)) { Name = watcher.Name }); break;
                    case StringWatcher: var numBytes = watcher.GetField<int>("_numBytes"); Watchers.Add(new FakeMemoryWatcher<string>(() => wdpr.DerefString(game, numBytes)) { Name = watcher.Name }); break;
                    default: throw new NotImplementedException();
                }
                continue;
            }

            // In case of DeepPointer, we need to take out the baseaddress and the offsets
            DeepPointer dpr = watcher.GetProperty<DeepPointer>("DeepPtr");
            IntPtr absBase = dpr.GetField<IntPtr>("_absoluteBase");
            int[] offsets = dpr.GetField<List<int>>("_offsets").Skip(1).ToArray();

            wdpr = new WiiDeepPointer(MEM1, MEM2, absBase, offsets);
            switch (watcher)
            {
                case MemoryWatcher<byte>:  Watchers.Add(new FakeMemoryWatcher<byte>(() => wdpr.Deref<byte>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<sbyte>: Watchers.Add(new FakeMemoryWatcher<sbyte>(() => wdpr.Deref<sbyte>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<bool>: Watchers.Add(new FakeMemoryWatcher<bool>(() => wdpr.Deref<bool>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<short>: Watchers.Add(new FakeMemoryWatcher<short>(() => wdpr.Deref<short>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<ushort>: Watchers.Add(new FakeMemoryWatcher<ushort>(() => wdpr.Deref<ushort>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<int>: Watchers.Add(new FakeMemoryWatcher<int>(() => wdpr.Deref<int>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<uint>: Watchers.Add(new FakeMemoryWatcher<uint>(() => wdpr.Deref<uint>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<long>: Watchers.Add(new FakeMemoryWatcher<long>(() => wdpr.Deref<long>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<ulong>: Watchers.Add(new FakeMemoryWatcher<ulong>(() => wdpr.Deref<ulong>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<float>: Watchers.Add(new FakeMemoryWatcher<float>(() => wdpr.Deref<float>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<double>: Watchers.Add(new FakeMemoryWatcher<double>(() => wdpr.Deref<double>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<char>: Watchers.Add(new FakeMemoryWatcher<char>(() => wdpr.Deref<char>(game)) { Name = watcher.Name }); break;
                case StringWatcher: var numBytes = watcher.GetField<int>("_numBytes"); Watchers.Add(new FakeMemoryWatcher<string>(() => wdpr.DerefString(game, numBytes)) { Name = watcher.Name }); break;
                default: throw new NotImplementedException();
            }
        }

        if (Endianess == Endianess.BigEndian)
        {
            LittleEndianWatchers = new();

            foreach (var entry in Watchers)
            {
                switch (entry)
                {
                    case FakeMemoryWatcher<byte>: LittleEndianWatchers.Add(new FakeMemoryWatcher<byte>(() => (byte)entry.Current) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<sbyte>: LittleEndianWatchers.Add(new FakeMemoryWatcher<sbyte>(() => (sbyte)entry.Current) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<bool>: LittleEndianWatchers.Add(new FakeMemoryWatcher<bool>(() => (bool)entry.Current) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<short>: LittleEndianWatchers.Add(new FakeMemoryWatcher<short>(() => ((short)entry.Current).SwapEndianess()) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<ushort>: LittleEndianWatchers.Add(new FakeMemoryWatcher<ushort>(() => ((ushort)entry.Current).SwapEndianess()) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<int>: LittleEndianWatchers.Add(new FakeMemoryWatcher<int>(() => ((int)entry.Current).SwapEndianess()) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<uint>: LittleEndianWatchers.Add(new FakeMemoryWatcher<uint>(() => ((uint)entry.Current).SwapEndianess()) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<long>: LittleEndianWatchers.Add(new FakeMemoryWatcher<long>(() => ((long)entry.Current).SwapEndianess()) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<ulong>: LittleEndianWatchers.Add(new FakeMemoryWatcher<ulong>(() => ((ulong)entry.Current).SwapEndianess()) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<float>: LittleEndianWatchers.Add(new FakeMemoryWatcher<float>(() => ((float)entry.Current).SwapEndianess()) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<double>: LittleEndianWatchers.Add(new FakeMemoryWatcher<double>(() => ((double)entry.Current).SwapEndianess()) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<char>: LittleEndianWatchers.Add(new FakeMemoryWatcher<char>(() => (char)entry.Current) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<string>: LittleEndianWatchers.Add(new FakeMemoryWatcher<string>(() => (string)entry.Current) { Name = entry.Name }); break;
                    default: throw new NotImplementedException();
                }
            }
        }
    }


    public class WiiDeepPointer
    {
        private readonly IntPtr MEM1;
        private readonly IntPtr MEM2;
        private readonly IntPtr _absoluteBase;
        private List<int> _offsets;

        public WiiDeepPointer(IntPtr MEM1, IntPtr MEM2, IntPtr absoluteBase, params int[] offsets)
        {
            this.MEM1 = MEM1;
            this.MEM2 = MEM2;
            _absoluteBase = absoluteBase;
            InitializeOffsets(offsets);
        }

        public bool DerefOffsets(Process process, out IntPtr ptr)
        {
            ptr = _absoluteBase;
            for (int i = 0; i < _offsets.Count - 1; i++)
            {
                if (!process.ReadValue<uint>(ptr + _offsets[i], out var tempPtr) || tempPtr == 0)
                    return false;
                tempPtr = tempPtr.SwapEndianess();
                ptr = tempPtr < 0x90000000 ? (IntPtr)((long)MEM1 + tempPtr - 0x80000000) : (IntPtr)((long)MEM2 + tempPtr - 0x90000000);
            }

            ptr += _offsets[_offsets.Count - 1];
            return true;
        }

        private void InitializeOffsets(params int[] offsets)
        {
            _offsets = new List<int>{ 0 };
            _offsets.AddRange(offsets);
        }


        public bool Deref<T>(Process process, out T value) where T : struct
        {
            if (!DerefOffsets(process, out IntPtr ptr) || !process.ReadValue(ptr, out value))
            {
                value = default;
                return false;
            }

            return true;
        }

        public bool DerefBytes(Process process, int count, out byte[] value)
        {
            if (!DerefOffsets(process, out var ptr) || !process.ReadBytes(ptr, count, out value))
            {
                value = null;
                return false;
            }

            return true;
        }

        public bool DerefString(Process process, ReadStringType type, int numBytes, out string str)
        {
            StringBuilder stringBuilder = new(numBytes);
            if (!DerefString(process, type, stringBuilder))
            {
                str = null;
                return false;
            }

            str = stringBuilder.ToString();
            return true;
        }

        public T Deref<T>(Process process, T default_ = default) where T : struct => !Deref(process, out T value) ? default_ : value;
        public byte[] DerefBytes(Process process, int count) => !DerefBytes(process, count, out var value) ? null : value;
        public bool DerefString(Process process, StringBuilder sb) => DerefString(process, ReadStringType.AutoDetect, sb);
        public bool DerefString(Process process, ReadStringType type, StringBuilder sb) => DerefOffsets(process, out var ptr) && process.ReadString(ptr, type, sb);
        public string DerefString(Process process, int numBytes, string default_ = null) => !DerefString(process, ReadStringType.AutoDetect, numBytes, out var str) ? default_ : str;
        public string DerefString(Process process, ReadStringType type, int numBytes, string default_ = null) => !DerefString(process, type, numBytes, out var str) ? default_ : str;
        public bool DerefString(Process process, int numBytes, out string str) => DerefString(process, ReadStringType.AutoDetect, numBytes, out str);
    }
}



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

    private Dictionary<string, WiiDeepPointer> _wiidptr { get; set; }
    private FakeMemoryWatcherList _fakeWatchers { get; set; }

    public new dynamic this[string index]
    {
        get
        {
            if (Endianess == Endianess.BigEndian)
                return LittleEndianWatchers[index];

            var watcher = Watchers.FirstOrDefault(w => w.Name == index);
            if (watcher != null)
                return watcher;


            return _fakeWatchers.First(w => w.Name == index);
        }
    }

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

        Watchers.UpdateAll(game);

        if (_fakeWatchers != null)
            _fakeWatchers.UpdateAll();

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
        Watchers = new();

        var tempWatchers = Load(MEM1, MEM2);

        _wiidptr = new();
        _fakeWatchers = new();
        
        // For each watcher this checks whether it's using DeepPointer.
        // If it is, it converts the DeepPointer to a new EmuDeepPointer
        foreach (var watcher in tempWatchers)
        {
            var addrType = watcher.GetProperty<int>("AddrType");
            
            if (addrType != 0) // If not DeepPointer, there's no other hack to do
            {
                Watchers.Add(watcher);
                continue;
            }

            // In case of DeepPointer, we need to take out the baseaddress and the offsets
            DeepPointer dpr = watcher.GetProperty<DeepPointer>("DeepPtr");
            IntPtr absBase = dpr.GetField<IntPtr>("_absoluteBase");
            int[] offsets = dpr.GetField<List<int>>("_offsets").Skip(1).ToArray();

            _wiidptr.Add(watcher.Name, new WiiDeepPointer(MEM1, MEM2, absBase, offsets));
            switch (watcher)
            {
                case MemoryWatcher<byte>:  _fakeWatchers.Add(new FakeMemoryWatcher<byte>(() => _wiidptr[watcher.Name].Deref<byte>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<sbyte>: _fakeWatchers.Add(new FakeMemoryWatcher<sbyte>(() => _wiidptr[watcher.Name].Deref<sbyte>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<bool>: _fakeWatchers.Add(new FakeMemoryWatcher<bool>(() => _wiidptr[watcher.Name].Deref<bool>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<short>: _fakeWatchers.Add(new FakeMemoryWatcher<short>(() => _wiidptr[watcher.Name].Deref<short>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<ushort>: _fakeWatchers.Add(new FakeMemoryWatcher<ushort>(() => _wiidptr[watcher.Name].Deref<ushort>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<int>: _fakeWatchers.Add(new FakeMemoryWatcher<int>(() => _wiidptr[watcher.Name].Deref<int>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<uint>: _fakeWatchers.Add(new FakeMemoryWatcher<uint>(() => _wiidptr[watcher.Name].Deref<uint>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<long>: _fakeWatchers.Add(new FakeMemoryWatcher<long>(() => _wiidptr[watcher.Name].Deref<long>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<ulong>: _fakeWatchers.Add(new FakeMemoryWatcher<ulong>(() => _wiidptr[watcher.Name].Deref<ulong>(game)) { Name = watcher.Name }); break;
                //case MemoryWatcher<IntPtr>: _fakeWatchers.Add(new FakeMemoryWatcher<byte>(() => _wiidptr[watcher.Name].Deref<byte>(game))); break;
                //case MemoryWatcher<UIntPtr>: _fakeWatchers.Add(new FakeMemoryWatcher<byte>(() => _wiidptr[watcher.Name].Deref<byte>(game))); break;
                case MemoryWatcher<float>: _fakeWatchers.Add(new FakeMemoryWatcher<float>(() => _wiidptr[watcher.Name].Deref<float>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<double>: _fakeWatchers.Add(new FakeMemoryWatcher<double>(() => _wiidptr[watcher.Name].Deref<double>(game)) { Name = watcher.Name }); break;
                case MemoryWatcher<char>: _fakeWatchers.Add(new FakeMemoryWatcher<char>(() => _wiidptr[watcher.Name].Deref<char>(game)) { Name = watcher.Name }); break;
                case StringWatcher: var numBytes = watcher.GetField<int>("_numBytes"); _fakeWatchers.Add(new FakeMemoryWatcher<string>(() => _wiidptr[watcher.Name].DerefString(game, numBytes)) { Name = watcher.Name }); break;
            }
        }

        if (Endianess == Endianess.BigEndian)
        {
            LittleEndianWatchers = ToLittleEndian.SetFakeWatchers(Watchers);

            foreach (var entry in _fakeWatchers)
            {
                switch (entry)
                {
                    case FakeMemoryWatcher<byte>: LittleEndianWatchers.Add(new FakeMemoryWatcher<byte>(() => _wiidptr[entry.Name].Deref<byte>(game)) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<sbyte>: LittleEndianWatchers.Add(new FakeMemoryWatcher<sbyte>(() => _wiidptr[entry.Name].Deref<sbyte>(game)) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<bool>: LittleEndianWatchers.Add(new FakeMemoryWatcher<bool>(() => _wiidptr[entry.Name].Deref<bool>(game)) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<short>: LittleEndianWatchers.Add(new FakeMemoryWatcher<short>(() => _wiidptr[entry.Name].Deref<short>(game).SwapEndianess()) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<ushort>: LittleEndianWatchers.Add(new FakeMemoryWatcher<ushort>(() => _wiidptr[entry.Name].Deref<ushort>(game).SwapEndianess()) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<int>: LittleEndianWatchers.Add(new FakeMemoryWatcher<int>(() => _wiidptr[entry.Name].Deref<int>(game).SwapEndianess()) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<uint>: LittleEndianWatchers.Add(new FakeMemoryWatcher<uint>(() => _wiidptr[entry.Name].Deref<uint>(game).SwapEndianess()) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<long>: LittleEndianWatchers.Add(new FakeMemoryWatcher<long>(() => _wiidptr[entry.Name].Deref<long>(game).SwapEndianess()) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<ulong>: LittleEndianWatchers.Add(new FakeMemoryWatcher<ulong>(() => _wiidptr[entry.Name].Deref<ulong>(game).SwapEndianess()) { Name = entry.Name }); break;
                    //case FakeMemoryWatcher<IntPtr>: LittleEndianWatchers.Add(new FakeMemoryWatcher<IntPtr>(() => _wiidptr[entry.Name].Deref<IntPtr>(game).SwapEndianess()) { Name = entry.Name }); break;
                    //case FakeMemoryWatcher<UIntPtr>: LittleEndianWatchers.Add(new FakeMemoryWatcher<UIntPtr>(() => _wiidptr[entry.Name].Deref<UIntPtr>(game).SwapEndianess()) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<float>: LittleEndianWatchers.Add(new FakeMemoryWatcher<float>(() => _wiidptr[entry.Name].Deref<float>(game).SwapEndianess()) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<double>: LittleEndianWatchers.Add(new FakeMemoryWatcher<double>(() => _wiidptr[entry.Name].Deref<double>(game).SwapEndianess()) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<char>: LittleEndianWatchers.Add(new FakeMemoryWatcher<char>(() => _wiidptr[entry.Name].Deref<char>(game)) { Name = entry.Name }); break;
                    case FakeMemoryWatcher<string>: var numBytes = tempWatchers[entry.Name].GetField<int>("_numBytes"); LittleEndianWatchers.Add(new FakeMemoryWatcher<string>(() => _wiidptr[entry.Name].DerefString(game, numBytes)) { Name = entry.Name }); break;
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
                var success = process.ReadPointer(ptr + _offsets[i], false, out ptr);
                if (!success || ptr == IntPtr.Zero)
                    return false;

                var _intptr = BitConverter.ToUInt32(BitConverter.GetBytes((long)ptr).Take(4).ToArray(), 0).SwapEndianess();
                ptr = (IntPtr)_intptr;

                if ((long)ptr < 0x90000000L)
                    ptr = (IntPtr)((long)MEM1 + ((long)ptr - 0x80000000));
                else
                    ptr = (IntPtr)((long)MEM2 + ((long)ptr - 0x90000000));
            }

            ptr += _offsets[_offsets.Count - 1];
            return true;
        }

        private void InitializeOffsets(params int[] offsets)
        {
            _offsets = new List<int>{ 0 };
            _offsets.AddRange(offsets);
        }

        public T Deref<T>(Process process, T default_ = default) where T : struct
        {
            if (!Deref<T>(process, out var value))
            {
                return default_;
            }

            return value;
        }

        public bool Deref<T>(Process process, out T value) where T : struct
        {
            if (!DerefOffsets(process, out var ptr) || !process.ReadValue<T>(ptr, out value))
            {
                value = default;
                return false;
            }

            return true;
        }

        public byte[] DerefBytes(Process process, int count)
        {
            if (!DerefBytes(process, count, out var value))
            {
                return null;
            }

            return value;
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

        public string DerefString(Process process, int numBytes, string default_ = null)
        {
            if (!DerefString(process, ReadStringType.AutoDetect, numBytes, out var str))
            {
                return default_;
            }

            return str;
        }

        public string DerefString(Process process, ReadStringType type, int numBytes, string default_ = null)
        {
            if (!DerefString(process, type, numBytes, out var str))
            {
                return default_;
            }

            return str;
        }

        public bool DerefString(Process process, int numBytes, out string str)
        {
            return DerefString(process, ReadStringType.AutoDetect, numBytes, out str);
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

        public bool DerefString(Process process, StringBuilder sb)
        {
            return DerefString(process, ReadStringType.AutoDetect, sb);
        }

        public bool DerefString(Process process, ReadStringType type, StringBuilder sb)
        {
            if (!DerefOffsets(process, out var ptr) || !process.ReadString(ptr, type, sb))
            {
                return false;
            }

            return true;
        }
    }
}



using System;
using System.Diagnostics;
using System.Threading.Tasks;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;

public class Megadrive : Genesis { }
public class MegaDrive : Genesis { }

public partial class Genesis
{
    // Stuff that need to be defined in the ASL
    public string[] Gamecodes { get; set; }
    public Func<IntPtr, MemoryWatcherList> Load { get; set; }

    // Other stuff
    private ProcessHook GameProcess { get; }
    private Func<bool> KeepAlive { get; set; }
    private Process game => GameProcess.Game;
    public MemoryWatcherList Watchers { get; private set; }
    public FakeMemoryWatcherList LittleEndianWatchers { get; private set; }
    public Endianess Endianess = Endianess.LittleEndian;
    private IntPtr WRAM { get; set; }


    public Genesis()
    {
        var processNames = new string[]
        {
            "retroarch",
            "SEGAGameRoom",
            "SEGAGenesisClassics",
            "Fusion",
            "gens",
            "blastem",
            "EmuHawk",
        };

        GameProcess = new ProcessHook(processNames);
    }

    public bool Update()
    {
        if (/*Gamecodes == null ||*/ Load == null)
            return false;

        if (!Init())
            return false;

        if (!KeepAlive())
        {
            GameProcess.InitStatus = GameInitStatus.NotStarted;
            return false;
        }

        Watchers.UpdateAll(game);
        if (Endianess == Endianess.BigEndian) LittleEndianWatchers.UpdateAll();

        //if (!Gamecodes.Contains(game.ReadString(MEM1, 6, " ")))
        //    return false;

        return true;
    }

    private bool Init()
    {
        // This "init" function checks if the autosplitter has connected to the game
        // (if it has not, there's no point in going further) and starts a Task to
        // get the needed memory addresses for the other methods.
        if (!GameProcess.IsGameHooked)
            return false;

        // The purpose of this task is to limit the update cycle to 1 every 1.5 seconds
        // (instead of the usual one every 16 msec) in order to avoid wasting resources
        if (GameProcess.InitStatus == GameInitStatus.NotStarted)
            Task.Run(() =>
            {
                GameProcess.InitStatus = GameInitStatus.InProgress;
                try
                {
                    var Init = GetWRAM();
                    WRAM = Init.Item1;
                    KeepAlive = Init.Item2;
                    Watchers = Load(WRAM);

                    if (Endianess == Endianess.LittleEndian)
                    {
                        foreach (var watcher in Watchers)
                        {
                            if (watcher is MemoryWatcher<byte> || watcher is MemoryWatcher<sbyte> || watcher is MemoryWatcher<bool>)
                            {
                                var Address = (long)watcher.GetProperty<IntPtr>("Address") - (long)WRAM;
                                Address = (Address & 1) == 0 ? Address + 1 : Address - 1;
                                watcher.SetProperty("Address", (IntPtr)((long)WRAM + Address));
                            }
                        }
                    }

                    if (Endianess == Endianess.BigEndian)
                        LittleEndianWatchers = ToLittleEndian.SetFakeWatchers(Watchers);

                    GameProcess.InitStatus = GameInitStatus.Completed;
                }
                catch
                {
                    Task.Delay(2000).Wait();
                    GameProcess.InitStatus = GameInitStatus.NotStarted;
                }
                // I'm running this manually because the signature scanner, especially
                // if it runs several times, can take A LOT of memory, to the point of
                // filling your RAM with several GB of useless data that doesn't get
                // collected for some reason.
                GC.Collect();
            });

        // At this point, if init has not been completed yet, return
        // false to avoid running the rest of the splitting logic.
        return GameProcess.InitStatus == GameInitStatus.Completed;
    }

    public void Dispose()
    {
        GameProcess.Dispose();
    }

    public dynamic this[string index] => Endianess == Endianess.BigEndian ? LittleEndianWatchers[index] : Watchers[index];

    private Tuple<IntPtr, Func<bool>> GetWRAM()
    {
        switch (game.ProcessName.ToLower())
        {
            case "retroarch": return Retroarch();
            case "segagenesisclassics": case "segagameroom": return SEGAClassics();
            case "fusion": return Fusion();
            case "gens": return Gens();
            case "blastem": return BlastEm();
            case "emuhawk": return EmuHawk();
        }

        Debugs.Info("  => Unrecognized emulator. Autosplitter will be disabled");
        return new Tuple<IntPtr, Func<bool>>(IntPtr.Zero, () => true);
    }
}
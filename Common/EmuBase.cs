using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public abstract class EmuBase
{
    // Stuff related to the emulator
    protected string[] EmulatorNames { get; set; }
    protected ProcessHook GameProcess { get; set; }
    protected Process game => GameProcess.Game;
    public Endianess Endianess { get; protected set; } = Endianess.LittleEndian;

    // Watchers
    public Func<IntPtr, MemoryWatcherList> Load { get; set; }
    public MemoryWatcherList Watchers { get; protected set; }
    public FakeMemoryWatcherList LittleEndianWatchers { get; protected set; }
    protected Func<bool> KeepAlive { get; set; }
    public dynamic this[string index] => Endianess == Endianess.LittleEndian ? Watchers[index] : LittleEndianWatchers[index];

    protected abstract void InitActions();

    public bool Update()
    {
        if (Load == null || !Init())
            return false;

        if (!KeepAlive())
        {
            GameProcess.InitStatus = GameInitStatus.NotStarted;
            return false;
        }

        Watchers.UpdateAll(game);

        if (Endianess == Endianess.BigEndian)
            LittleEndianWatchers.UpdateAll();

        return true;
    }

    protected bool Init()
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
                    InitActions();
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
        GameProcess?.Dispose();
    }
}

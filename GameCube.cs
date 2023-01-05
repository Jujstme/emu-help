using System;
using System.Diagnostics;
using System.Threading.Tasks;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;
using System.Linq;

public class Gamecube : GCN { }
public class GameCube : GCN { }

public partial class GCN
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
    public bool IsBigEndian { get; set; } = false;
    private IntPtr MEM1 { get; set; }


    public GCN()
    {
        var processNames = new string[]
        {
            "Dolphin",
        };

        GameProcess = new ProcessHook(processNames);
    }

    public bool Update()
    {
        if (Load == null)
            return false;

        if (!Init())
            return false;

        if (!KeepAlive())
        {
            GameProcess.InitStatus = GameInitStatus.NotStarted;
            return false;
        }

        Watchers.UpdateAll(game);
        LittleEndianWatchers.UpdateAll();

        if (Gamecodes != null && !Gamecodes.Contains(game.ReadString(MEM1, 6, " ")))
            return false;

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
                    MEM1 = Init.Item1;
                    KeepAlive = Init.Item2;
                    Watchers = Load(MEM1);
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

    public dynamic this[string index] => IsBigEndian ? LittleEndianWatchers[index] : Watchers[index];

    private Tuple<IntPtr, Func<bool>> GetWRAM()
    {
        switch (game.ProcessName)
        {
            case "Dolphin": return Dolphin();
        }

        Debugs.Info("  => Unrecognized emulator. Autosplitter will be disabled");
        return new Tuple<IntPtr, Func<bool>>(IntPtr.Zero, () => true);
    }
}
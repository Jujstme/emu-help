using System;
using System.Diagnostics;
using Helper.Data.AutoSplitter;
using System.Threading.Tasks;
using LiveSplit.EMUHELP;

public abstract partial class HelperBase
{
    internal ProcessHook GameProcess { get; set; }
    internal Process game => GameProcess.Game;
    internal virtual Endianness.Endian Endian => Endianness.Endian.Little;
    protected abstract void InitActions();
    protected Func<bool> KeepAlive { get; set; } = () => false;

    public bool Update()
    {
        if (Load == null || !Init())
            return false;

        if (!KeepAlive())
        {
            GameProcess.InitStatus = GameInitStatus.NotStarted;
            return false;
        }

        Watchers.UpdateAll();

        if (isASLCodeGenerating)
            foreach (var entry in Watchers)
                Script.current[entry.Name] = entry.Current;

        return true;
    }

    protected bool Init()
    {
        // This "init" function checks if the autosplitter has connected to the game
        // (if it has not, there's no point in going further) and starts a Task to
        // get the needed memory addresses for the other methods.
        if (!GameProcess.IsGameHooked)
            return false;

        // The purpose of this task is to limit the update cycle to 1 every 2 seconds
        // (instead of the usual one every 16 msec) in order to avoid wasting resources
        if (GameProcess.InitStatus == GameInitStatus.NotStarted)
            Task.Run(() =>
            {
                GameProcess.InitStatus = GameInitStatus.InProgress;
                try
                {
                    _load.Clear();
                    _stringLoad.Clear();
                    if (!Load(this))
                        throw new Exception();

                    game.ResetModulesWow64Cache();
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
}

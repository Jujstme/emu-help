using System;
using System.Diagnostics;
using Helper.Data.AutoSplitter;
using System.Threading.Tasks;
using LiveSplit.EMUHELP;

public abstract partial class HelperBase
{
    protected ProcessHook GameProcess { get; set; }
    internal Process Game => GameProcess.Game;
    internal EmuBase Emu { get; set; }
    protected abstract void InitActions();

    public bool Update()
    {
        if (Load == null || !Init())
            return false;

        if (!Emu.KeepAlive())
        {
            GameProcess.InitStatus = GameInitStatus.NotStarted;
            return false;
        }

        _watchers.UpdateAll();

        if (isASLCodeGenerating)
            foreach (var entry in _watchers)
                Script.current[entry.Name] = entry.Current;

        return true;
    }

    private bool Init()
    {
        // This "init" function checks if the autosplitter has connected to the game
        // (if it has not, there's no point in going further) and starts a Task to
        // get the needed memory addresses for the other methods.
        if (!GameProcess.IsGameHooked)
            return false;

        // The purpose of this task is to limit the update cycle to 1 every 2 seconds
        // (instead of the usual one every 16 msec) in order to avoid wasting resources
        if (GameProcess.InitStatus == GameInitStatus.NotStarted)
        {
            GameProcess.InitStatus = GameInitStatus.InProgress;

            _load.Clear();
            _stringLoad.Clear();

            Task.Run(() =>
            {
                void ManageFail()
                {
                    Task.Delay(2000).Wait();
                    GameProcess.InitStatus = GameInitStatus.NotStarted;
                }

                try
                {
                    if (Load(this))
                    {
                        Game.ResetModulesWow64Cache();
                        InitActions();
                        MakeWatchers();
                        GameProcess.InitStatus = GameInitStatus.Completed;
                    }
                    else
                    {
                        ManageFail();
                    }
                }
                catch
                {
                    ManageFail();
                }

                // I'm running this manually because the signature scanner, especially
                // if it runs several times, can take A LOT of memory, to the point of
                // filling your RAM with several GB of useless data that doesn't get
                // collected for some reason.
                GC.Collect();
            });
        }

        // At this point, if init has not been completed yet, return
        // false to avoid running the rest of the splitting logic.
        return GameProcess.InitStatus == GameInitStatus.Completed;
    }
}

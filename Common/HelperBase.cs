using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Helper.Data.AutoSplitter;
using LiveSplit.EMUHELP;

public abstract class HelperBase
{
    private bool isASLCodeGenerating { get; set; }

    // Stuff related to the emulator
    internal ProcessHook GameProcess { get; set; }
    internal Process game => GameProcess.Game;

    // Watchers
    protected Dictionary<string, Tuple<TypeCode, uint, uint[]>> _load = new();
    protected Dictionary<string, int> _stringLoad { get; set; } = new();
    public Func<HelperBase, bool> Load { get; set; }
    internal FakeMemoryWatcherList Watchers { get; set; }

    // Abstracts
    public FakeMemoryWatcher this[string index] => Watchers[index];
    protected abstract void InitActions();
    protected Func<bool> KeepAlive { get; set; } = () => false;

    /// <summary>
    /// Creates a new instance of the Helper class with code generation.
    /// </summary>
    public HelperBase()
        : this(true) { }

    /// <summary>
    /// Creates a new instance of the Helper class, optionally enabling the code generation.
    /// Code generation must be kept disabled if using the helper in a component or outside 
    /// an .asl script.
    /// </summary>
    /// <param name="generateCode"></param>
    public HelperBase(bool generateCode)
    {
        //Manager = this;
        isASLCodeGenerating = generateCode;

        if (generateCode)
            ASL_Startup();
        else
        {
            Debugs.Welcome();
            Debugs.Info();
            Debugs.Info("Loading emu-help...");
        }
    }

    protected virtual void ASL_Startup()
    {
        if (Actions.Current != "startup")
        {
            string msg = "The helper may only be instantiated in the 'startup {}' action.";
            throw new InvalidOperationException(msg);
        }

        Debugs.Welcome();
        Debugs.Info();
        Debugs.Info("Loading emu-help...");
            
        Debugs.Info("  => Generating code...");

        Script.vars.Helper = this;
        Debugs.Info("    => Set helper to vars.Helper.");
        Actions.update.Prepend("if (!vars.Helper.Update()) return false;\n");
        Actions.shutdown.Prepend("vars.Helper.Dispose();\n");
    }

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

    public void Make<T>(string name, uint offset, params uint[] offsets) where T : struct
    {
        _load[name] = Tuple.Create(Type.GetTypeCode(typeof(T)), offset, offsets);
    }

    public void MakeString(string name, int length, uint offset, params uint[] offsets)
    {
        _stringLoad[name] = length;
        _load[name] = Tuple.Create(Type.GetTypeCode(typeof(string)), offset, offsets);
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

    public void Dispose()
    {
        GameProcess?.Dispose();
    }
}

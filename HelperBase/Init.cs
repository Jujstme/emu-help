using System;
using Helper.Data.AutoSplitter;
using LiveSplit.EMUHELP;

public abstract partial class HelperBase
{
    private readonly bool isASLCodeGenerating;

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
        isASLCodeGenerating = generateCode;

        if (isASLCodeGenerating)
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
        else
        {
            Debugs.Welcome();
            Debugs.Info();
            Debugs.Info("Loading emu-help...");
        }
    }

    public void Dispose()
    {
        GameProcess?.Dispose();
    }
}
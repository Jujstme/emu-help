#define DEBUG
using System.Diagnostics;

namespace LiveSplit.EMUHELP
{
    internal static class Debugs
    {
        public static void Info()
        {
            Debug.WriteLine("[EMU-HELP]");
        }

        public static void Info(object output)
        {
            Debug.WriteLine($"[EMU-HELP] {output}");
        }

        public static void Welcome()
        {
            Debug.WriteLine("""
        Thank you for using emu-help! For more information, see https://github.com/Jujstme/emu-help.
        This library is mainly aimed at helping out ASL developers who wish to write autosplitters
        for console emulators.

        If you would like to opt out of code generation, please use the following code in 'startup {}' instead.
        Make sure to call GetType() with the name of the specific helper you would like to use:

            var type = Assembly.Load(File.ReadAllBytes("Components\emu-help-v2")).GetType("PS1");
            vars.Helper = Activator.CreateInstance(type, args: false);

        If you have any questions, please tag jujstme in the #auto-splitters channel
        of the Speedrun Tool Development Discord server: https://discord.gg/cpYsxz7.
        """);
        }
    }
}
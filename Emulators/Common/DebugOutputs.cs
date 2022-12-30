namespace LiveSplit.EMUHELP
{
    internal static class Debugs
    {
        public static void Info()
        {
            System.Diagnostics.Debug.WriteLine("[EMU-HELP]");
        }

        public static void Info(object output)
        {
            System.Diagnostics.Debug.WriteLine($"[EMU-HELP] {output}");
        }
    }
}

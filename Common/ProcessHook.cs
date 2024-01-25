using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LiveSplit.EMUHELP
{
    /// <summary>
    /// Custom class with the ability to automatically hook to a target process using Tasks
    /// </summary>
    public class ProcessHook
    {
        // Internal stuff
        private readonly string[] processNames;
        private readonly CancellationTokenSource CancelToken;

        /// <summary>
        /// Game process
        /// </summary>
        public Process Game { get; protected set; }
        public bool IsGameHooked => Game != null && !Game.HasExited;


        /// <summary>
        /// Reports the current status for the init function. The autosplitter will check if
        /// it's set to GameInitStatus.Completed before performing any actual splitting logic.
        /// </summary>
        public GameInitStatus InitStatus { get; set; }

        public ProcessHook(string exeName)
        {
            processNames = new string[] { exeName };
            InitStatus = GameInitStatus.NotStarted;
            CancelToken = new CancellationTokenSource();
            Task.Run(TryConnect, CancelToken.Token);
        }

        public ProcessHook(string[] exeNames)
        {
            processNames = exeNames;
            InitStatus = GameInitStatus.NotStarted;
            CancelToken = new CancellationTokenSource();
            Task.Run(TryConnect, CancelToken.Token);
        }

        public ProcessHook(List<string> exeNames)
        {
            processNames = exeNames.ToArray();
            InitStatus = GameInitStatus.NotStarted;
            CancelToken = new CancellationTokenSource();
            Task.Run(TryConnect, CancelToken.Token);
        }

        private async Task TryConnect()
        {
            while (!CancelToken.IsCancellationRequested)
            {
                foreach (var entry in processNames)
                {
                    try { Game = Process.GetProcessesByName(entry).OrderByDescending(p => p.StartTime).FirstOrDefault(p => !p.HasExited); }
                    catch { break; }

                    if (Game != null)
                    {
                        Game.Exited += CallBackTryConnect;
                        return;
                    }
                }
                await Task.Delay(1500, CancelToken.Token);
            }
        }

        private void CallBackTryConnect(object sender, EventArgs e)
        {
            Game.Exited -= CallBackTryConnect;
            InitStatus = GameInitStatus.NotStarted;
            Game?.Dispose();
            Game = null;
            Task.Run(TryConnect, CancelToken.Token);
        }

        public void Dispose()
        {
            CancelToken?.Cancel();
            CancelToken?.Dispose();
            Game?.Dispose();
        }
    }

    public enum GameInitStatus
    {
        NotStarted,
        InProgress,
        Completed
    }
}

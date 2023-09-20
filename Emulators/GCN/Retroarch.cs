﻿using LiveSplit.ComponentUtil;
using System;
using System.Linq;

namespace LiveSplit.EMUHELP.GCN
{
    internal class Retroarch : GCNBase
    {
        private readonly IntPtr core_base_address;

        public Retroarch(HelperBase helper) : base(helper)
        {
            if (!Helper.game.Is64Bit())
                throw new Exception();

            string[] supportedCores =
            {
                "dolphn_libretro.dll",
            };
            ProcessModuleWow64Safe currentCore = Helper.game.ModulesWow64Safe().First(m => supportedCores.Any(e => e == m.ModuleName));
            core_base_address = currentCore.BaseAddress;

            var dolphin = new Dolphin(Helper);
            MEM1 = dolphin.MEM1;
            Endian = dolphin.Endian;

            Debugs.Info("  => Hooked to emulator: Retroarch");
            Debugs.Info($"  => MEM1 address found at 0x{MEM1.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            return Helper.game.ReadBytes(core_base_address, 1, out _);
        }
    }
}
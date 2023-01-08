# emu-help

Emu-help is a C# library intended to provide easy access to memory addresses in console games being run via emulators.
Its primary intended use case is in conjunction with a .asl script for LiveSplit autosplitters on Windows systems.

## Supported systems and emulators

The library is still on a preliminary phase. Support is currently provided for the following systems:
- Sega Master System
- Sega Megadrive / Sega Genesis
- Playstation 1
- Playstation 2
- Nintendo Gamecube
- Nintendo Wii

Each system has separate support for various emulators. For example, Sega Genesis games are supported on Retroarch, BizHawk, Gens, BlastEm, Fusion / Kega Fusion and the official Steam release of SEGA Genesis Classics. Or, just to make another example, Playstation 1 games are supported on BizHawk, Retroarch, ePSXe, PCSX_Redux, Xebra, pSX and Duckstation.

Supported emulators for each system can be inferred by looking at the source code. However, as the main objective of this library is to provide support for as many emulators as possible, if you want support added for other systems or emulators, please submit a new issue.

## Examples

The following example creates a persistent instance of the class of your choice. In this example we'll pretend we want to write an autosplitter for a PS1 game.

```cs
state("LiveSplit") {}

startup
{
    vars.Helper = Assembly.Load(File.ReadAllBytes("Components/emu-help")).CreateInstance("PS1");
}
```

The helper needs to call its `Update()` function at the beginning of the `update` block in your .asl script:

```cs
update
{
    // If the update cycle fails (for example because the emulator is not supported
    // or the base addresses cannot be found), this will also disable the autosplitter.
    if (!vars.Helper.Update())
        return false;
    
    // Your code goes here
    ...
}
```

The following code also needs to be added in `shutdown` in order to allow the routines inside emu-help to terminate properly when the autosplitter is disposed.

```cs
shutdown
{
    vars.Helper.Dispose();
}
```

## Defining memory addresses

In older game systems, addresses are static, which means our values of interest can easily be recovered by knowing the offset from the base WRAM addresses in the original system's memory. In order to do this, we need to define `vars.Helper.Load`.
In most cases, the following example will be enough:

```cs
startup
{
    ...

    vars.Helper.Load = (Func<IntPtr, MemoryWatcherList>)(wram => new MemoryWatcherList
    {
        new MemoryWatcher<byte>(wram + 0x12AA) { Name = "Lives" },
        new StringWatcher(wram + 0x2AA10) { Name = "Level" },
    });
}
```

Only for Nintendo Wii: the Wii uses 2 non-contiguous memory regions (usually called `MEM1` and `MEM2`) so you need to know in which of those 2 regions your value is stored:

```cs
startup
{
    ...

    vars.Helper.Load = (Func<IntPtr, IntPtr, MemoryWatcherList>)((MEM1, MEM2) => new MemoryWatcherList
    {
        new MemoryWatcher<int>(MEM1 + 0xA1670) { Name = "IGT" },
        new StringWatcher(MEM2 + 0x2000, 10) { Name = "Status" },
    });
}
```

Playstation 1, Playstation 2, Gamecube and Wii also have an internal `Gamecode` that can be defined in order to differentiate between PAL / NTSC releases. For a couple of examples, have a look at those:
- <a href=https://github.com/Jujstme/Autosplitters/blob/master/Kula%20World/LiveSplit.KulaWorld.asl>Kula World</a> (PS1)
- <a href=https://github.com/Jujstme/Autosplitters/blob/master/TimeSplitters%20-%20Future%20Perfect/LiveSplit.TimeSplittersFuturePerfect.asl>Timesplitters: Future Perfect</a> (PS2 and GameCube)

Other scripts using emu-help that show how simple it is to set up:
- <a href=https://github.com/SonicSpeedrunning/LiveSplit.SonicTripleTrouble/blob/main/LiveSplit.SonicTripleTrouble.asl>Sonic Triple Trouble</a> (Game Gear)

## Endianess

Some systems, mostly older ones and especially Nintendo's, use and store memory values as Big Endian. When accessing any value (eg. `vars.Helper["IGT"].Current`) Emu-help will automatically convert Big Endian values to Little Endian.
If, for any reason, you need to reach the original, untouched value, it's still easily reachable without much issue (eg. `vars.Helper.Watchers["IGT"].Current`).

Other systems, notably 16-bit ones like the SEGA Genesis, use Big Endian but some emulators (while others do not) tend to byte-swap the internal WRAM. this means that an offset of `0x1000` on the original system is translated to `0x1001` on emulators (and vice versa). In order to let the helper work properly, it's important to use the offsets as they are in the original system: the helper will take care of these scenarios automatically.

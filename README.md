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

Each system has separate support for various emulators. For example, Sega Genesis games are supported on Retroarch, BizHawk, Gens, BlastEm, Fusion / Kega Fusion and the official Steam release of SEGA Genesis Classics. Playstation 1 games are supported on BizHawk, Retroarch, ePSXe, PCSX_Redux, Xebra, pSX and Duckstation.

The source code clearly shows the supported emulators for each system. If you need or want support added for currenty unsupported systems, please submit a new issue.

## Examples

The following example creates a persistent instance of the class of your choice. In this example we are going to use the PS1 class (for Playstation 1 games/emulators).

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

In older game systems, addresses are static, which means they can easily be recovered by knowing the offset from the base WRAM addresses in the original system's memory. In order to do this, we need to define `vars.Helper.Load`.
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

    vars.Helper.Load = (Func<IntPtr, IntPtr, MemoryWatcherList>)(MEM1, MEM2 => new MemoryWatcherList
    {
        new MemoryWatcher<int>(MEM1 + 0xA1670) { Name = "IGT" },
        new StringWatcher(MEM2 + 0x2000) { Name = "Status" },
    });
}
```

## Endianess

Some systems (notably Nintendo ones) use and store memory values as Big Endian. When accessing any value (eg. `vars.Helper["IGT"].Current`) Emu-help will automatically convert Big Endian values to Little Endian.
If, for any reason, you need to reach the original, untouched value, it's still easily reachable (eg. `vars.Helper.Watchers["IGT"].Current`).

Other systems, notably 16-bit ones like the SEGA Genesis, use Big Endian but some emulators (while other do not) tend to byte-swap the internal WRAM. this means that on offset of `0x1000` on the original systems is translated to `0x1001` on emulators (and vice versa). In order to let the helper work properly, it's important to use the offsets as they are in the original system.

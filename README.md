# emu-help

Emu-help is a C# library intended to provide easy access to memory addresses in console games being run via emulators.
Its primary intended use case is in conjunction with a .asl script for LiveSplit autosplitters on Windows systems.

## Supported systems and emulators

Support is currently provided for the following systems:
- Sega Master System
- Sega Megadrive / Sega Genesis
- Playstation 1
- Playstation 2
- Nintendo GameBoy Advance
- Nintendo Gamecube
- Nintendo Wii

Each system has separate support for various emulators. For example, Sega Genesis games are supported on Retroarch, Gens, BlastEm, Fusion / Kega Fusion and the official Steam release of SEGA Genesis Classics. Or, just to make another example, Playstation 1 games are supported on Retroarch, ePSXe, PCSX_Redux, Xebra, pSX and Duckstation.

Supported emulators for each system can be inferred by looking at the source code. In case of need, please tag Jujstme in the `#auto-splitters` channel of the Speedrun Tool Development Discord server: https://discord.gg/cpYsxz7.

## Examples

The following example creates a persistent instance of the class of your choice. In this example we'll pretend we want to write an autosplitter for a PS1 game.
The library will automatically generate some code needed for the autosplitter to work and then load the new class instance as `vars.Helper`.

```cs
state("LiveSplit") {}

startup
{
    Assembly.Load(File.ReadAllBytes("Components/emu-help-v2")).CreateInstance("PS1");
}
```

## Defining memory addresses

Our values of interest can easily be recovered by knowing the mapped RAM addresses in the original system's memory. In order to do this, we will define `vars.Helper.Load`.
In most cases, the following example will be enough:

```cs
startup
{
    ...

    vars.Helper.Load = (Func<dynamic, bool>)(emu => 
    {
        emu.Make<int>("IGT", 0x800A36A0);
        emu.Make<byte>("Lives", 0x8002AAC);
        emu.MakeString("Map", 15, 0x800B6000);
        return true;
    });
}
```

In case of systems using pointer (eg. PS2 or Wii), you can easily provide the entire pointer path. The library will automatically dereference the pointer path for you:

```cs
startup
{
    ...

    vars.Helper.Load = (Func<dynamic, bool>)(emu => 
    {
        emu.Make<int>("IGT", 0x800A36A0, 0x20, 0xC0);
        emu.Make<byte>("Lives", 0x8002AAC);
        emu.MakeString("Map", 15, 0x800B2010, 0x34);
        return true;
    });
}
```

You can then use your values directly in your code:

```cs
update
{
    // Both ways are valid
    print(vars.Helper["IGT"].Current.ToString());
    print(current.IGT.ToString());
}

split
{
    return current.Map != old.Map;
}
```

## Endianess

Some systems, mostly older ones and especially Nintendo's, use and store memory values as Big Endian. The library is programmed to automatically convert big-endian values to little-endian, for supported emulators.
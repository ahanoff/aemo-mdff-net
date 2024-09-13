# AEMO.MDFF

[![Build](https://github.com/ahanoff/aemo-mdff-net/actions/workflows/build.yaml/badge.svg)](https://github.com/ahanoff/aemo-mdff-net/actions/workflows/build.yaml)
![NuGet Version](https://img.shields.io/nuget/vpre/AEMO.MDFF)
![NuGet Downloads](https://img.shields.io/nuget/dt/AEMO.MDFF)

Parser for Australian Energy Market Operator (AEMO) Meter Data File Format (MDFF) specification

## Getting started

Install Nuget package

```shell
dotnet add package AEMO.MDFF
```

Use Nem12Reader to parse csv file

```csharp
using AEMO.MDFF.NEM12;

var nem12Reader = new Nem12Reader();
await using var fs = new FileStream("nem12.csv", FileMode.Open, FileAccess.Read);
await foreach (var r in nem12Reader.ReadAsync(fs, CancellationToken.None))
{
    switch (r)
    {
        case HeaderRecord { VersionHeader: var vh }:
            Console.WriteLine(vh);
            break;
        case NMIDataDetailsRecord { NextScheduledReadDate: var nsrd }:
            Console.WriteLine(nsrd.ToLongDateString());
            break;
    }
}
```

## References

 - https://www.aemo.com.au/
 - https://www.aemo.com.au/-/media/Files/Electricity/NEM/Retail_and_Metering/Metering-Procedures/2018/MDFF-Specification-NEM12--NEM13-v106.pdf
 - https://en.wikipedia.org/wiki/Australian_Energy_Market_Operator
 - https://en.wikipedia.org/wiki/National_Electricity_Market

![FSharp Social Preview](images/social.png)

This is a PowerShell module to enable use of the F# language in PowerShell. It is currently only supported with Powershell 7.2+ at the moment.

There are two separate modules that are published from this repo:

1. `FSharp`: Provides the `Add-FSharpType` command which enables dynamic compilation of F# code to be loaded into PowerShell.
1. `FSharp.Core`: Provides the core fsharp libraries for FSharp-based modules. It is binary compatible so you should always set your modules to require the latest version of this module, even if you wrote for an older version, so that the latest version is always used.

## FSharp Quickstart

```ps
Install-Module FSharp
Add-FSharpType -TypeDefinition @'
namespace Test
type Car = {
  model: string
  name: string
  speed: int
}
'@
[Test.Car]::new('Mazda','Miata',65)
```

```text
model name  speed
----- ----  -----
Mazda Miata    65
```

## Creating PowerShell Modules with F#

1. Create a new F# powershell module project as normal and set both `FSharp.Core` and `System.Management.Automation` as privateassets so they are not published.
1. Your .psd1 file *must* have a dependency on the `FSharp.Core` module. Set your dependency to the minimumversion of F# code you are using or higher **always**, do not pin this dependency directly or it will cause problems.

namespace PoshFSharp

module AddFSharpType =
  open System
  open System.IO
  open FSharp.Compiler.CodeAnalysis

  /// This is basically the compiler so we name it that way
  let compiler = FSharpChecker.Create()

  let AddFromFile (sourceFile : string) =
    let destination = "ThisIsIgnoredForDynamicAssemblyCompliation.dll"
    let initializeOpts = Some(stderr, stdout)
    let errors, exitCode, assembly =
      compiler.CompileToDynamicAssembly([|
        "fsc.dll"
        "--platform:x64"
        "--target:library"
        "--targetprofile:netcore"
        "-a"; sourceFile
        "-o"; destination
      |], initializeOpts)
      |> Async.RunSynchronously
    // TODO: Better error handling
    if exitCode > 0 then raise (InvalidOperationException(errors.[0].ToString()))
    assembly.Value

  let AddType (typeDefinition : string) =
    let tempName = Path.GetTempFileName()
    let tempFile = Path.ChangeExtension(tempName, ".fsx")
    File.WriteAllText(tempFile, typeDefinition)
    AddFromFile(tempFile)

/// The PowerShell Cmdlet to implement the above
open System.Management.Automation

// [<Cmdlet("Add","Type",DefaultParameterSetName="string")>]
[<Cmdlet("Add","Type")>]
type AddFSharpTypeCommand () =
  inherit PSCmdlet ()
  [<Parameter(Mandatory=true, ParameterSetName="string")>]
  member val TypeDefinition : string = "" with get, set
  [<Parameter(Mandatory=true, ParameterSetName="file", ValueFromPipelineByPropertyName=true)>]
  // Enables the command to accept FileInfo objects implicitly from the pipeline
  [<Alias("FullName")>]
  member val Path : string = "" with get, set
  [<Parameter>]
  member val PassThru : SwitchParameter = SwitchParameter(false) with get, set

  override this.EndProcessing() =
    let assembly =
      if this.ParameterSetName = "string"
        then AddFSharpType.AddType(this.TypeDefinition)
        else AddFSharpType.AddFromFile(this.Path)
    if this.PassThru.IsPresent then this.WriteObject(assembly.DefinedTypes)
    base.EndProcessing()

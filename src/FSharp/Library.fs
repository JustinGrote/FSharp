namespace PoshFSharp

module AddFSharpType =
  open FSharp.Compiler.CodeAnalysis
  open System
  open type System.IO.Path
  open type System.IO.File
  open type System.Reflection.Assembly

  /// This is basically the compiler so we name it that way
  let compiler = FSharpChecker.Create()

  let GetTempFilePathWithExtension (extension: string) =
    let tempPath = GetTempPath()
    let fileName = ChangeExtension(Guid.NewGuid().ToString(), extension)
    let tempFilePath = Combine(tempPath, fileName)
    tempFilePath.ToString()

  let AddFromFile (sourceFile : string) =
    let destination = GetTempFilePathWithExtension ".dll"
    let errors, exitCode =
      compiler.Compile([|
        "fsc.dll"
        "--platform:x64"
        "-a"; sourceFile
        "-o"; destination
      |])
      |> Async.RunSynchronously
    // TODO: Better error handling
    if exitCode > 0 then raise (InvalidOperationException(errors.[0].ToString()))
    let assembly = LoadFrom(destination)
    assembly

  let AddType (typeDefinition : string) =
    let tempFile = GetTempFilePathWithExtension ".fsx"
    WriteAllText(tempFile, typeDefinition)
    AddFromFile(tempFile)

/// The PowerShell Cmdlet to implement the above
open System.Management.Automation

// [<Cmdlet("Add","Type",DefaultParameterSetName="string")>]
[<Cmdlet("Add","FSharpType")>]
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

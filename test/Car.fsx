#r "Newtonsoft.Json.dll"
namespace Test
open Newtonsoft.Json
module Don =
  let data = {| Name = "Don Syme"; Occupation = "F# Creator" |}
  let json = JsonConvert.SerializeObject(data)

type FsxCar = {
  name: string
  speed: int
  model: string
}

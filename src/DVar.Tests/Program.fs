module DVar.Tests.Program

open Expecto
open Utilities

[<Tests>]
let tests =
  testList "dvar memory" [
    testCase "hello world" <| fun _ ->
      let myService (config:string) : unit -> string =
        fun () -> sprintf "config=%s" config

      let myConfig : DVar<string> =
        DVar.create "A"

      let myService' = 
        myConfig 
        |> DVar.map myService
        |> DVar.toFun

      Expect.equal (myService' ()) "config=A" "Should have original"

      DVar.put "B" myConfig

      Expect.equal (myService' ()) "config=B" "Should have original"

    testCase "gc" <| fun _ ->
      ()
  ]


[<EntryPoint>]
let main argv =
  Tests.runTestsInAssembly defaultConfig argv
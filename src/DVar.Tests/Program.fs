module DVar.Tests.Program

open System
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
      let myService (config:string) : unit -> string =
        fun () -> sprintf "config=%s" config

      let myConfig : DVar<string> =
        DVar.create "A"

      let myService' =
        myConfig
        |> DVar.map myService
        |> DVar.toFun

      let mem1 = GC.GetTotalMemory(true) |> float
      for i in 0..100000 do
        DVar.put (new String(char i, 512)) myConfig // 1 KiB, each char is two bytes
      let mem2 = GC.GetTotalMemory(true) |> float // myService' still in scope, closed over DVar
      Expect.floatEqual mem1 mem2 (Some (float sizeof<char> * 512. * 100.))
                        "Should not differ by more than a thousandth"
  ]


[<EntryPoint>]
let main argv =
  Tests.runTestsInAssembly defaultConfig argv
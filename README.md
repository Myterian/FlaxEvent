# FlaxEvent 3

![image](Images/Preview_3.jpg "FlaxEvents - Editor-Configureable Events for the Flax Engine")
![image](Images/Parameter_Preview.jpg "Type integration for event parameters")

## How to use in Editor

1. Drag an object into the target selector
![image](Images/Drag_Preview.jpg "Drag an object into the target selector")

2. Select a target method, either from the actor or one if its scripts
![image](Images/Selection_Preview.jpg "Select the target method you want to invoke")

3. Set any parameters to the desired value
![image](Images/Done_Preview.jpg "(Optional) Set the parameters to the desired value")


## How to use in Code

Example:
```cs
public class MyScript : Script
{
    // Simple Event
    public FlaxEvent MyEvent = new();

    // Event with parameters
    public FlaxEvent<string, int> MyLargeEvent = new();
    ...

    public override void OnUpdate()
    {
        // Simple invoke
        if (Input.GetKeyDown(KeyboardKeys.Spacebar))
            MyEvent?.Invoke();

        // Invoke with parameters
        if (Input.GetKeyDown(KeyboardKeys.Return))
            MyLargeEvent?.Invoke("some cool text", 7);
    }
}
```
\
Add the desired FlaxEvent type to your `Actor` or `Script`. FlaxEvents supports up to four generic arguments:

```cs
public FlaxEvent MySimpleEvent = new();
public FlaxEvent<T> MySmallEvent = new();
public FlaxEvent<T0, T1> MyMediumEvent = new();
public FlaxEvent<T0, T1, T2> MyLargeEvent = new();
public FlaxEvent<T0, T1, T2, T3> MyHugeEvent = new();
```

\
To Invoke an event, simply call:

```cs
MyEvent?.Invoke();
```

\
You can add and remove Listeners at runtime thru code. Note that runtime listeners are separate from editor-configured listeners.

```cs
public FlaxEvent MyEvent = new();
...

MyEvent?.AddListener(MyMethod)
MyEvent?.RemoveListener(MyMethod)
...

public void MyMethod()
{
    Debug.Log("Hello World");
}
```

## Benchmark

Test setup: 
- 5.000 Cube Actors
- The test events invoke three method on each actor, making a single invoke to 15.000 listeners total
- 20 iterations of invokes, amounting to 300.000 invokes total


|Invokation Source|Avg. time first Invoke|Avg. time per Invoke  |Total Time|
|-----------------|---------------------|-----------------|----------|
|C# Action Delegate |???| ???             | ???      |
|FlaxEvent Persistent Call|???|0.018ms - 0.019ms | ~5580ms  |
|FlaxEvent Runtime Call|???|???| ??? |

\
Benchmark script
```cs
public class Benchmark : Script
{
    public FlaxEvent<int> OnBenchmarkEvent = new();

    private void BenchmarkTest()
    {
        // Warm up
        for (int i = 0; i < 2; i++)
            OnBenchmarkEvent?.Invoke(7);

        // Cleanup
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // 20 iterations of 15.000 listener invokes
        Stopwatch stopwatch = new();
        stopwatch.Start();

        for (int i = 0; i < 20; i++)
            OnBenchmarkEvent?.Invoke(7);
        
        stopwatch.Stop();

        FlaxEngine.Debug.Log($"ms per invoke: {stopwatch.Elapsed.TotalMilliseconds / 300_000}"); // 20 iterations * 15k listeners
        FlaxEngine.Debug.Log($"Total benchmark time: {stopwatch.Elapsed.TotalMilliseconds}");
    }
}

```

## How to Set Up
- [Install](#how-to-install) the plugin
- Add the dependency to the `*.Build.cs` file of every module where you want to use FlaxEvents
```cs
/// <inheritdoc />
public override void Setup(BuildOptions options)
{
    ...

    options.PublicDependencies.Add(nameof(FlaxEvent));
    // or
    options.PublicDependencies.Add("FlaxEvent");
    ...
}
```


## How to Install
The easy method: 
- In the Flax Editor, go to `Tools > Plugins > Clone Project` and paste this repo link `https://github.com/Myterian/FlaxEvent.git` into the `Git Path`, then `Clone`
- Restart the Editor
- Done

Alternatively, you can manually add this plugin:
- Close the Editor
- Clone this repo into `<your-game-project-folder>\Plugins\FlaxEvent\`
- Add a reference to FlaxEvent to your game, by modifying the `<your-game>.flaxproj` file
```
...
"References": [
    {
        "Name": "$(EnginePath)/Flax.flaxproj"
    },
    {
        "Name": "$(ProjectPath)/Plugins/FlaxEvent/FlaxEvent.flaxproj"
    }
]
```
- Restart and done


## Known Issues
- None, but bug reports are open

![image](Images/Preview.jpg "FlaxEvents - Editor-Configureable Events for the Flax Engine")
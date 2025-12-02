# FlaxEvent 3
Editor-Configureable Events for the Flax Engine

![image](Images/Preview_3.jpg "FlaxEvents - Editor-Configureable Events for the Flax Engine")
![image](Images/Parameter_Preview_2.jpg "Type integration for event parameters")

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
FlaxEvents support up to four types of parameters:

```cs
public FlaxEvent MySimpleEvent = new();
public FlaxEvent<T> MySmallEvent = new();
public FlaxEvent<T0, T1> MyMediumEvent = new();
public FlaxEvent<T0, T1, T2> MyLargeEvent = new();
public FlaxEvent<T0, T1, T2, T3> MyHugeEvent = new();
```

\
FlaxEvents also support runtime listeners, so you can dynamically add and remove methods, when the game is running:

```cs
public FlaxEvent MyEvent = new();

public override OnEnable()
{
    MyEvent?.AddListener(MyMethod);
    ...
}

public override OnDisable()
{
    MyEvent?.RemoveListener(MyMethod);
    ...
}

public void MyMethod()
{
    Debug.Log("Hello World");
}
```

## Benchmark

This Benchmark is meant to give you an idea, how FlaxEvents compare to regular C# delegates. Take these values with a grain of salt, as I only tested this on my old FX-8350 Cpu.


|Event Type        |(Editor) Avg. First uncached Invoke     |(Editor) Avg. Subsequent cached Invoke|(Game) Avg. First uncached Invoke     |(Game) Avg. Subsequent cached Invoke|
|-------------------------|-------------------------------|-------------------------------|-----------------------------|-----------------------------|
|FlaxEvent Editor Call    | ~0.02ms                       | ~0.01ms                       | ~0.02ms                     | ~0.01ms                     |
|FlaxEvent Runtime Call   | ~0.005ms                      | ~0.0007 ms                    | ~0.0005ms                   | < 0.0001ms                  |
|C# Action Delegate       | ~0.005ms                      | ~0.0007 ms                    | ~0.0005ms                   | < 0.0001ms                  |

\
Test setup: 
- 500 Cube Actors in one scene
- 3 different events were measured: FlaxEvent with only editor configured calls, FlaxEvent with only runtime calls and a C# Action
- The test events invoked `Actor.OnActiveChanged` for every actor
- First invoke is a single event invoke, amounting to 500 method invokes total, per event
- Subsequent invoke are 1.000 event invokes, amounting to 500.000 method invokes total, per event


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
- Tested on Flax v. 1.11

![image](Images/Preview.jpg "FlaxEvents - Editor-Configureable Events for the Flax Engine")

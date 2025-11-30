using Flax.Build;

public class FlaxEventTarget : GameProjectTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // Reference the modules for game
        Modules.Add(nameof(FlaxEvent));
    }
}

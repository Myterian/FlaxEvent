// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using FlaxEngine;

namespace FlaxEvents;

/// <summary>FlaxEvent GamePlugin.</summary>
public class FlaxEventPlugin : GamePlugin
{
    public static Version PluginVersion = new(3, 0, 168);

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

    }

    /// <inheritdoc/>
    public override void Deinitialize()
    {
        base.Deinitialize();

    }

    public FlaxEventPlugin()
    {
        _description = new()
        {
            Name = "FlaxEvent",
            Description = "Editor-Configurable Events for the Flax Engine",
            Category = "FlaxEvent",
            RepositoryUrl = "https://github.com/Myterian/FlaxEvent.git",
            Author = "Thomas Jungclaus",
            AuthorUrl = "https://github.com/Myterian/",
            IsAlpha = false,
            IsBeta = false,
            Version = PluginVersion
        };
    }
}
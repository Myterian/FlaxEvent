// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

#if FLAX_EDITOR

namespace FlaxEvents;

/// <summary>Available drag types. Used in <see cref="DragOperation"/></summary>
internal enum DragType : byte
{
    None = 0,
    Shift = 1,
    Move = 2
}

#endif
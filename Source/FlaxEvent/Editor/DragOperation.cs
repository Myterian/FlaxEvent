// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

#if FLAX_EDITOR

using FlaxEditor;
using FlaxEngine;

namespace FlaxEvents;

/// <summary>Handles Drag and Drop logic for <see cref="PersistentCallEditor"/></summary>
internal class DragOperation
{
    private PersistentCallListEditor parentEditor;
    private PersistentCallEditor activeTarget;
    private int elementIndex;

    /// <summary>Value indicating if this <see cref="DragOperation"/> is still in use</summary>
    public bool IsDisposed { get; private set; } = false;

    /// <summary>Updates the drag state</summary>
    private void UpdateDrag()
    {
        int copyToInt = GetChildFromMouse();
        DragType dragType = DragType.Shift;

        // Reset colors at the start of each update
        activeTarget?.SetDragSelectionColor(DragType.None);
        parentEditor.SetDragSelectionColor(DragType.None);

        // Case: We're outside the index range of the child editors, meaning the mouse is outside the editors elements.
        // This is treated as either insert as first or last element. Handy shortcut, too.
        if (Mathf.IsNotInRange(copyToInt, 0, parentEditor.ChildrenEditors.Count - 1))
        {
            parentEditor.SetDragSelectionColor(dragType);

            if (parentEditor.IsMouseInBottomArea())
                copyToInt = parentEditor.ChildrenEditors.Count;
            else
                copyToInt = 0;
        }

        // Case: We're inside the index range of child editors, meaning the mouse hovers inbetween the group elements
        // of the child editors
        if (Mathf.IsInRange(copyToInt, 0, parentEditor.ChildrenEditors.Count - 1))
        {
            activeTarget = parentEditor.ChildrenEditors[copyToInt] as PersistentCallEditor;

            if (Input.GetKey(KeyboardKeys.Shift))
                dragType = DragType.Move;

            activeTarget?.SetDragSelectionColor(dragType);
        }

        // Mouse release
        if (!Input.GetMouseButtonUp(MouseButton.Left))
            return;

        Dispose();

        // This shouldn't happen, but just in case
        if (elementIndex == -1)
            return;

        // Set persistent call list values
        if (dragType == DragType.Move)
            parentEditor.MovePersistentCall(elementIndex, copyToInt);

        if (dragType == DragType.Shift)
            parentEditor.ShiftPersistentCall(elementIndex, copyToInt);

        activeTarget?.SetDragSelectionColor(DragType.None);
    }

    /// <summary>Gets the child editor index from the current mouse position</summary>
    /// <returns>index of child editor. -1 if none were found.</returns>
    private int GetChildFromMouse()
    {
        int targetIndex = -1;

        for (int i = 0; i < parentEditor.ChildrenEditors.Count; i++)
        {
            if (Mathf.IsNotInRange(i, 0, parentEditor.ChildrenEditors.Count - 1))
                continue;
            
            if (parentEditor.ChildrenEditors[i] == null)
                continue;

            PersistentCallEditor callEditor = parentEditor.ChildrenEditors[i] as PersistentCallEditor;

            if (callEditor == null)
                continue;

            if (!callEditor.IsMouseInBounds())
                continue;

            targetIndex = callEditor.Index;
        }

        return targetIndex;
    }

    /// <summary>Removes this <see cref="DragOperation"/> from the EditorUpdate</summary>
    public void Dispose()
    {
        IsDisposed = true;
        Editor.Instance.EditorUpdate -= UpdateDrag;
    }

    /// <summary>Creates a new <see cref="DragOperation"/></summary>
    /// <param name="parent">The parent editor. <see cref="DragOperation"/> are done in the <see cref="PersistentCallEditor"/></param>
    /// <param name="dragFromIndex">The editors index</param>
    public DragOperation(PersistentCallListEditor parent, int dragFromIndex)
    {
        parentEditor = parent;
        elementIndex = dragFromIndex;
        IsDisposed = false;
        Editor.Instance.EditorUpdate += UpdateDrag;
    }
}

#endif
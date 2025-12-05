// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

#if FLAX_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using FlaxEditor.GUI.ContextMenu;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxEvents;

/// <summary>Child menu that is used in the method selection menu in <see cref="PersistentCallEditor"/>. It shows what methods are available and highlights existing selection.</summary>
[HideInEditor]
public class FlaxEventContextChildMenu : FlaxEventContextButton
{
    public readonly ContextMenu ContextMenu = new ContextMenu();

    private readonly TextBox SearchBox;


    /// <summary>Simple search method. Orders child buttons by how good they match the users search term.</summary>
    private void OrderBySearch()
    {
        ContextMenu.LockChildrenRecursive();
        List<Control> orderedList = null;

        // Box is empty, so we reset to original order
        if (string.IsNullOrEmpty(SearchBox.Text))
            orderedList = ContextMenu.ItemsContainer.Children.OrderBy(x => (x as FlaxEventContextButton)?.InitialIndex ?? int.MaxValue).ToList();
        
        // Box contains a search term
        else
        {
            // This ranks the buttons by how much the search term overlaps with an indiviual method name. The lower the rank, the closer the match.
            for (int i = 0; i < ContextMenu.ItemsContainer.ChildrenCount; i++)
            {
                FlaxEventContextButton child = ContextMenu.ItemsContainer.Children[i] as FlaxEventContextButton;

                if (child == null)
                    continue;

                string normalizedMethodName = child.MethodName.ToLowerInvariant();
                string normalizedSearchTerm = SearchBox.Text.ToLowerInvariant();

                // Similarity in length (don't get into the fucking details)
                child.CurrentRanking = LevenshteinDistance(normalizedSearchTerm, normalizedMethodName);

                // 0 in LevenshteinDistance is a perfect match, so it's getting boosted
                if (child.CurrentRanking == 0)
                    child.CurrentRanking -= 1000;

                // Otherwise any other similarity will get boosted, whilst not outrank the top candidates
                else if (normalizedMethodName.StartsWith(normalizedSearchTerm))
                    child.CurrentRanking -= 200 - normalizedSearchTerm.Length;

                else if (normalizedMethodName.Contains(normalizedSearchTerm))
                    child.CurrentRanking -= 50;

            }

            orderedList = ContextMenu.ItemsContainer.Children.OrderBy(x => (x as FlaxEventContextButton)?.CurrentRanking ?? int.MaxValue).ToList();
        }

        // The buttons list cannot be set in one go, so this is the best reorder right now
        for (int q = 0; q < ContextMenu.ItemsContainer.ChildrenCount; q++)
            ContextMenu.ItemsContainer.Children[q] = orderedList[q];

        ContextMenu.ItemsContainer.ScrollViewTo(new Float2(0, 0), true);
        ContextMenu.UnlockChildrenRecursive();
        ContextMenu.PerformLayout();
    }


    /// <summary>
    /// Creates a value indicating how much the string filter has to be transformed to become string text. 
    /// 0 means they're the same, where as higher values indicate differences.
    /// </summary>
    /// <param name="filter">Filter string, like search term</param>
    /// <param name="text">The text to compare the filter to</param>
    /// <returns>int. 0 if filter and text are the same, higher values with differences</returns>
    private int LevenshteinDistance(string filter, string text)
    {
        int[,] matrix = new int[filter.Length + 1, text.Length + 1];

        for (int i = 0; i <= filter.Length; i++)
            matrix[i, 0] = i;

        for (int i = 0; i <= text.Length; i++)
            matrix[0, i] = i;

        for (int i = 1; i <= filter.Length; i++)
            for (int x = 1; x <= text.Length; x++)
            {
                int cost = (filter[i - 1] == text[x - 1]) ? 0 : 1;

                matrix[i, x] = Mathf.Min(Mathf.Min(matrix[i - 1, x] + 1, matrix[i, x - 1] + 1), matrix[i - 1, x - 1] + cost);
            }

        return matrix[filter.Length, text.Length];
    }


    private void ShowChild(ContextMenu parentContextMenu)
    {
        float top = parentContextMenu.ItemsAreaMargin.Top;
        Float2 location = new Float2(Width, 0f - top);

        location = PointToParent(parentContextMenu, location);
        parentContextMenu.ShowChild(ContextMenu, location);
    }

    public override void Draw()
    {
        Style current = Style.Current;
        Rectangle rect = new Rectangle(0f - X + 3f, 0f, Parent.Width - 6f, Height);

        bool isOpened = ContextMenu.IsOpened;

        if (isOpened)
            Render2D.FillRectangle(rect, current.LightBackground);


        base.Draw();

        if (ContextMenu.HasChildren)
            Render2D.DrawSprite(current.ArrowRight, new Rectangle(Width - 15f, (Height - 12f) / 2f, 12f, 12f), (!Enabled) ? current.ForegroundDisabled : (isOpened ? current.BackgroundSelected : current.Foreground));

    }

    public override void OnMouseEnter(Float2 location)
    {
        if (ContextMenu.HasChildren)
        {
            ContextMenu parentContextMenu = base.ParentContextMenu;
            if (parentContextMenu != ContextMenu && !ContextMenu.IsOpened)
            {
                base.OnMouseEnter(location);
                ShowChild(parentContextMenu);
            }
        }
    }

    public override bool OnMouseUp(Float2 location, MouseButton button)
    {
        ContextMenu parentContextMenu = ParentContextMenu;
        if (parentContextMenu == ContextMenu)
            return true;

        if (ContextMenu.IsOpened)
            return true;

        ShowChild(parentContextMenu);
        return base.OnMouseUp(location, button);
    }


    [Obsolete("Don't use!")]
    public FlaxEventContextChildMenu(ContextMenu parent, string text, string shortKeys = "") : base(parent, text, shortKeys)
    {
        Text = text;
        CloseMenuOnClick = false;
    }

    public FlaxEventContextChildMenu(ContextMenu parent, string text, bool isActiveTarget, string shortKeys = "") : base(parent, text, null, null, -1,null, null, shortKeys)
    {
        Text = text;
        CloseMenuOnClick = false;
        IsActiveTarget = isActiveTarget;
        ContextMenu.ItemsAreaMargin = new(0, 0, 30, 0);

        SearchBox = ContextMenu.AddChild<TextBox>();
        SearchBox.WatermarkText = "Search a Method Name";
        SearchBox.AnchorPreset = AnchorPresets.HorizontalStretchTop;
        SearchBox.Height = 20;

        SearchBox.TextChanged -= OrderBySearch;
        SearchBox.TextChanged += OrderBySearch;
    }
}

#endif
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Content;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Hosting;
using WinRT;

namespace AlAnvar.Common;

public partial class ComboBoxHelper
{
    private static ContentExternalBackdropLink? backdropLink;
    private static DesktopAcrylicController? desktopAcrylicController;
    private static MicaController? micaController;
    private static SystemBackdropConfiguration? systemBackdropConfiguration;
    private static bool comboboxconnected;

    public static BackdropType GetSystemBackdropKind(DependencyObject obj)
    {
        return (BackdropType) obj.GetValue(SystemBackdropKindProperty);
    }

    public static void SetSystemBackdropKind(DependencyObject obj, BackdropType value)
    {
        obj.SetValue(SystemBackdropKindProperty, value);
    }

    public static readonly DependencyProperty SystemBackdropKindProperty =
        DependencyProperty.RegisterAttached("SystemBackdropKind", typeof(BackdropType), typeof(ComboBoxHelper), new PropertyMetadata(BackdropType.Mica));


    public static bool GetUseSystemBackdrop(DependencyObject obj)
    {
        return (bool) obj.GetValue(UseSystemBackdropProperty);
    }

    public static void SetUseSystemBackdrop(DependencyObject obj, bool value)
    {
        obj.SetValue(UseSystemBackdropProperty, value);
    }

    public static readonly DependencyProperty UseSystemBackdropProperty =
        DependencyProperty.RegisterAttached("UseSystemBackdrop", typeof(bool), typeof(ComboBoxHelper), new PropertyMetadata(false, OnUseSystemBackdropChanged));

    private static void OnUseSystemBackdropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ComboBox comboBox)
        {
            comboBox.Loaded -= OnLoaded;
            comboBox.Loaded += OnLoaded;

            void OnLoaded(object sender, RoutedEventArgs e)
            {
                if (comboBox.FindDescendant("Popup") is not Popup popup)
                {
                    return;
                }

                popup.Opened -= OnPopupOpened;
                popup.Opened += OnPopupOpened;
                popup.ActualThemeChanged -= OnPopupActualThemeChanged;
                popup.ActualThemeChanged += OnPopupActualThemeChanged;

                if (!comboBox.IsEditable)
                {
                    comboBox.IsDropDownOpen = true;
                }
            }

            void OnPopupActualThemeChanged(FrameworkElement sender, object args)
            {
                if (systemBackdropConfiguration is not null)
                {
                    systemBackdropConfiguration.Theme = ElementToSystemBackdrop(sender.ActualTheme);
                }
            }

            void OnPopupOpened(object? sender, object e)
            {
                if (sender is not Popup popup)
                {
                    return;
                }

                if (popup.FindName("PopupBorder") is not Border border)
                {
                    return;
                }

                Vector2 size = border.ActualSize;
                Compositor compositor = ElementCompositionPreview.GetElementVisual(border).Compositor;
                Vector2 cornerRadius = new(8, 8);

                if (!comboboxconnected)
                {
                    comboboxconnected = true;

                    UIElement child = border.Child;
                    Grid rootGrid = new();
                    border.Child = rootGrid;
                    Grid visualGrid = new();
                    rootGrid.Children.Add(visualGrid);
                    rootGrid.Children.Add(child);

                    backdropLink = ContentExternalBackdropLink.Create(compositor);
                    backdropLink.ExternalBackdropBorderMode = CompositionBorderMode.Soft;

                    // Modify PlacementVisual
                    Visual placementVisual = backdropLink.PlacementVisual;
                    placementVisual.Size = size;
                    placementVisual.Clip = compositor.CreateRectangleClip(0, 0, size.X, size.Y, cornerRadius, cornerRadius, cornerRadius, cornerRadius);
                    placementVisual.BorderMode = CompositionBorderMode.Soft;

                    ElementCompositionPreview.SetElementChildVisual(visualGrid, placementVisual);

                    systemBackdropConfiguration = new()
                    {
                        IsInputActive = true,
                        Theme = ElementToSystemBackdrop(popup.ActualTheme)
                    };

                    var backdropKind = GetSystemBackdropKind(comboBox);
                    switch (backdropKind)
                    {
                        case BackdropType.Acrylic:
                            desktopAcrylicController = new();
                            desktopAcrylicController.SetSystemBackdropConfiguration(systemBackdropConfiguration);
                            desktopAcrylicController.AddSystemBackdropTarget(backdropLink.As<ICompositionSupportsSystemBackdrop>());
                            break;
                        case BackdropType.AcrylicThin:
                            desktopAcrylicController = new();
                            desktopAcrylicController.Kind = DesktopAcrylicKind.Thin;
                            desktopAcrylicController.SetSystemBackdropConfiguration(systemBackdropConfiguration);
                            desktopAcrylicController.AddSystemBackdropTarget(backdropLink.As<ICompositionSupportsSystemBackdrop>());
                            break;
                        case BackdropType.Mica:
                            micaController = new();
                            micaController.SetSystemBackdropConfiguration(systemBackdropConfiguration);
                            micaController.AddSystemBackdropTarget(backdropLink.As<ICompositionSupportsSystemBackdrop>());
                            break;
                        case BackdropType.MicaAlt:
                            micaController = new();
                            micaController.Kind = MicaKind.BaseAlt;
                            micaController.SetSystemBackdropConfiguration(systemBackdropConfiguration);
                            micaController.AddSystemBackdropTarget(backdropLink.As<ICompositionSupportsSystemBackdrop>());
                            break;
                    }

                    popup.IsOpen = false;
                }
                else if (backdropLink is not null && systemBackdropConfiguration is not null)
                {
                    // Update PlacementVisual
                    Visual placementVisual = backdropLink.PlacementVisual;
                    placementVisual.Size = size;
                    placementVisual.Clip = compositor.CreateRectangleClip(0, 0, size.X, size.Y, cornerRadius, cornerRadius, cornerRadius, cornerRadius);
                }
            }
        }
    }

    private static SystemBackdropTheme ElementToSystemBackdrop(ElementTheme elementTheme)
    {
        return elementTheme switch
        {
            ElementTheme.Default => SystemBackdropTheme.Default,
            ElementTheme.Light => SystemBackdropTheme.Light,
            ElementTheme.Dark => SystemBackdropTheme.Dark,
            _ => SystemBackdropTheme.Default,
        };
    }
}

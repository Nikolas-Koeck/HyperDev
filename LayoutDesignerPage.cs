using HyperDev.src.Models;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using System.Collections.ObjectModel;

namespace HyperDev;

public class PlacedFeature
{
    public required Feature Feature { get; init; }
    public double X { get; set; } = 20;
    public double Y { get; set; } = 20;
    public double Width { get; set; } = 200;
    public double Height { get; set; } = 120;

}

// ── Page ─────────────────────────────────────────────────────────────────────
public class LayoutDesignerPage : ContentPage
{
    // State
    private readonly ObservableCollection<Feature> _availableFeatures;
    private readonly List<PlacedFeature> _placedFeatures = [];
    private Feature? _draggedFeature;
    private AbsoluteLayout _canvas = null!;
    private double _oldCanvasSizeX, _oldCanvasSizeY = 0;


    // Resize state
    private PlacedFeature? _resizing;
    private double _resizeStartX, _resizeStartY;
    private double _resizeOrigW, _resizeOrigH;

    // Move state
    private PlacedFeature? _moving;
    private double _moveOffsetX, _moveOffsetY;

    private double? _placingX, _placingY = null;

    public LayoutDesignerPage()
    {
        Title = "Layout Designer";

        _availableFeatures =
        [
            new() { Name = "Chart Feature",   ViewModel = new ChartFeatureViewModel   { Title = "Chart", Value = 75 } },
            new() { Name = "Table Feature",   ViewModel = new TableFeatureViewModel   { Rows = 5 } },
            new() { Name = "Metrics Feature", ViewModel = new MetricsFeatureViewModel { Metric1 = 42, Metric2 = 89 } },
            new() { Name = "Image Feature",   ViewModel = new ImageFeatureViewModel   { ImageUrl = "https://picsum.photos/200/150" } },
        ];

        Content = BuildRootGrid();
    }

    private Grid BuildRootGrid()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(220)),
                new ColumnDefinition(GridLength.Star),
            }
        };
        // Feature Elemente
        grid.Add(BuildSidePanel(), column: 0);
        // Layout Designer
        grid.Add(BuildCanvasPanel(), column: 1);
        return grid;
    }

    private View BuildSidePanel()
    {
        var header = new Label
        {
            Text = "Features",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#333333"),
            Margin = new Thickness(0, 0, 0, 8),
        };

        var featureList = new VerticalStackLayout { Spacing = 8 };
        BindableLayout.SetItemsSource(featureList, _availableFeatures);
        BindableLayout.SetItemTemplate(featureList, new DataTemplate(BuildDraggableButton));

        var hint = new Label
        {
            Text = "Drag onto canvas →\nThen move & resize freely.",
            FontSize = 11,
            TextColor = Color.FromArgb("#999999"),
            Margin = new Thickness(0, 12, 0, 0),
        };

        return new VerticalStackLayout
        {
            Padding = new Thickness(12),
            BackgroundColor = Color.FromArgb("#F0F2F5"),
            Children = { header, new ScrollView { Content = featureList }, hint }
        };
    }

    private View BuildCanvasPanel()
    {
        _canvas = new AbsoluteLayout
        {
            BackgroundColor = Colors.White,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
        };

        _oldCanvasSizeX = _canvas.Width;
        _oldCanvasSizeY = _canvas.Height;

        _canvas.SizeChanged += (_, _) =>
        {
            // Force redraw of placed items to update position based on new canvas size
            foreach (var child in _canvas.Children)
            {
                if (child is Border card && card.Content is Grid grid && grid.Children[1] is Label body)
                {
                    // grid.Children[0] ist titleBar (ein Grid), dessen erstes Kind ist das titleLabel
                    var titleLabel = (grid.Children[0] as Grid)?.Children.OfType<Label>().FirstOrDefault();
                    if (titleLabel != null)
                    {
                        var placed = _placedFeatures.FirstOrDefault(p => p.Feature.Name == titleLabel.Text);
                        if (placed != null)
                        {
                            double difX25 = _canvas.Width * 0.25 - _oldCanvasSizeX * 0.25;
                            double difY25 = _canvas.Height * 0.25 - _oldCanvasSizeY * 0.25;
                            setXPosition(new Point(placed.X + (placed.X > _oldCanvasSizeX * 0.25 ? difX25 : 0), placed.Y));
                            setYPosition(new Point(placed.X, placed.Y + (placed.Y > _oldCanvasSizeY * 0.25 ? difY25 : 0)));

                            AbsoluteLayout.SetLayoutBounds(card, new Rect((double)_placingX, (double)_placingY, _canvas.Width * 0.25, _canvas.Height * 0.25));
                            body.Text = FormatBodyText(placed);
                        }
                    }
                }
            }
            _oldCanvasSizeX = _canvas.Width;
            _oldCanvasSizeY = _canvas.Height;

        };

        var dropGesture = new DropGestureRecognizer { AllowDrop = true };
        dropGesture.DragOver += OnCanvasOver;
        dropGesture.Drop += OnCanvasDrop;
        _canvas.GestureRecognizers.Add(dropGesture);

        // Placeholder — removed after first drop
        var placeholder = new Label
        {
            Text = "Drop features here",
            TextColor = Color.FromArgb("#BBBBBB"),
            FontSize = 14,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        AbsoluteLayout.SetLayoutFlags(placeholder, AbsoluteLayoutFlags.PositionProportional);
        AbsoluteLayout.SetLayoutBounds(placeholder, new Rect(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
        _canvas.Children.Add(placeholder);

        _removePlaceholder = () =>
        {
            if (_canvas.Children.Contains(placeholder))
                _canvas.Children.Remove(placeholder);
        };

        return new Border
        {
            Margin = new Thickness(12),
            Padding = new Thickness(0),
            Stroke = Color.FromArgb("#D0D4DA"),
            StrokeThickness = 1,
            Content = _canvas,
        };
    }

    private void OnCanvasOver(object? sender, DragEventArgs args)
    {
        if (args.Data.Properties.TryGetValue("feature", out var obj) && obj is not Feature feature)
        {
            return;
        }

        Point? position = args.GetPosition(_canvas);
        if (position == null)
        {
            //outside bounds
            return;
        }

        setYPosition(position);

        setXPosition(position);


        //args.AcceptedOperation = DataPackageOperation.Copy;

    }

    private void setXPosition(Point? position)
    {
        if (position.Value.X >= _canvas.Width * 0.75)
        {
            _placingX = _canvas.Width * 0.75;
        }
        else if (position.Value.X >= _canvas.Width * 0.5)
        {
            _placingX = _canvas.Width * 0.5;
        }
        else if (position.Value.X >= _canvas.Width * 0.25)
        {
            _placingX = _canvas.Width * 0.25;
        }
        else
        {
            _placingX = 0;
        }
    }

    private void setYPosition(Point? position)
    {
        if (position.Value.Y >= _canvas.Height * 0.75)
        {
            _placingY = _canvas.Height * 0.75;
        }
        else if (position.Value.Y >= _canvas.Height * 0.5)
        {
            _placingY = _canvas.Height * 0.5;
        }
        else if (position.Value.Y >= _canvas.Height * 0.25)
        {
            _placingY = _canvas.Height * 0.25;
        }
        else
        {
            _placingY = 0;
        }
    }

    private Action _removePlaceholder = () => { };

    private View BuildDraggableButton()
    {
        var label = new Label
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TextColor = Colors.White,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
        };
        label.SetBinding(Label.TextProperty, "Name");

        var border = new Border
        {
            HorizontalOptions = LayoutOptions.Fill,
            Padding = new Thickness(12, 10),
            BackgroundColor = Color.FromArgb("#3A7BDB"),
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Content = label,
        };
        border.SetBinding(BindableObject.BindingContextProperty, ".");

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => border.BackgroundColor = Color.FromArgb("#2A5FBB");
        pointer.PointerExited += (_, _) => border.BackgroundColor = Color.FromArgb("#3A7BDB");
        border.GestureRecognizers.Add(pointer);

        var drag = new DragGestureRecognizer { CanDrag = true };
        drag.DragStarting += (_, args) =>
        {
            if (border.BindingContext is not Feature feature) return;
            _draggedFeature = feature;
            args.Data.Properties["feature"] = feature;
        };
        drag.DropCompleted += (_, _) => _draggedFeature = null;
        border.GestureRecognizers.Add(drag);

        return border;
    }

    private void OnCanvasDrop(object? sender, DropEventArgs args)
    {
        Feature? feature =
            args.Data.Properties.TryGetValue("feature", out var obj) && obj is Feature f
                ? f
                : _draggedFeature;

        if (feature is null) return;
        if (_placingX is null || _placingY is null) return;

        _removePlaceholder();

        var placed = new PlacedFeature
        {
            Feature = feature,
            X = (double)_placingX,
            Y = (double)_placingY,
            Width = _canvas.Width * 0.25,
            Height = _canvas.Height * 0.25,
        };
        _placedFeatures.Add(placed);
        AddCanvasCard(placed);
    }

    // ── Canvas card — moveable + resizeable ───────────────────────────────────
    private void AddCanvasCard(PlacedFeature placed)
    {
        // Title bar
        var titleLabel = new Label
        {
            Text = placed.Feature.Name,
            TextColor = Colors.White,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(8, 0, 0, 0),
        };

        var closeBtn = new Border
        {
            WidthRequest = 20,
            HeightRequest = 20,
            BackgroundColor = Color.FromArgb("#E05555"),
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 6, 0),
            Content = new Label
            {
                Text = "✕",
                FontSize = 9,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            }
        };

        var titleBar = new Grid
        {
            HeightRequest = 32,
            BackgroundColor = Color.FromArgb("#3A7BDB"),
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        titleBar.Add(titleLabel, column: 0);
        titleBar.Add(closeBtn, column: 1);

        // Body — shows live position + size, replace with real feature content as needed
        var body = new Label
        {
            Text = FormatBodyText(placed),
            TextColor = Color.FromArgb("#555555"),
            FontSize = 11,
            Margin = new Thickness(8),
            VerticalOptions = LayoutOptions.Start,
        };

        // Resize handle (bottom-right corner grip)
        var resizeHandle = new Border
        {
            WidthRequest = 18,
            HeightRequest = 18,
            BackgroundColor = Color.FromArgb("#3A7BDB"),
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = 3 },
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 0, 2, 2),
            Content = new Label
            {
                Text = "⤡",
                FontSize = 10,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            }
        };

        // Card inner grid: title bar row + body row
        var cardInner = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(32)),
                new RowDefinition(GridLength.Star),
            }
        };
        cardInner.Add(titleBar, row: 0);
        cardInner.Add(body, row: 1);
        cardInner.Add(resizeHandle, row: 1); // overlaid at bottom-right

        var card = new Border
        {
            Stroke = Color.FromArgb("#D0D4DA"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            BackgroundColor = Color.FromArgb("#FAFAFA"),
            Shadow = new Shadow
            {
                Brush = Colors.Black,
                Offset = new Point(2, 2),
                Radius = 6,
                Opacity = 0.12f,
            },
            Content = cardInner,
        };

        // Set initial position and size on AbsoluteLayout
        AbsoluteLayout.SetLayoutFlags(card, AbsoluteLayoutFlags.None);
        AbsoluteLayout.SetLayoutBounds(card, new Rect(placed.X, placed.Y, placed.Width, placed.Height));
        _canvas.Children.Add(card);

        // ── Move: drag the title bar ─────────────────────────────────────────
        var moveGesture = new PointerGestureRecognizer();
        moveGesture.PointerPressed += (_, e) =>
        {
            _moving = placed;
            var pos = e.GetPosition(_canvas)!.Value;
            _moveOffsetX = pos.X - placed.X;
            _moveOffsetY = pos.Y - placed.Y;
        };
        moveGesture.PointerMoved += (_, e) =>
        {
            if (_moving != placed) return;
            var pos = e.GetPosition(_canvas)!.Value;
            placed.X = Math.Max(0, pos.X - _moveOffsetX);
            placed.Y = Math.Max(0, pos.Y - _moveOffsetY);
            AbsoluteLayout.SetLayoutBounds(card, new Rect(placed.X, placed.Y, placed.Width, placed.Height));
            body.Text = FormatBodyText(placed);
        };
        moveGesture.PointerReleased += (_, _) =>
        {
            if (_moving == placed) _moving = null;
        };
        titleBar.GestureRecognizers.Add(moveGesture);

        // ── Resize: drag the bottom-right handle ─────────────────────────────
        var resizeGesture = new PointerGestureRecognizer();
        resizeGesture.PointerPressed += (_, e) =>
        {
            _resizing = placed;
            var pos = e.GetPosition(_canvas)!.Value;
            _resizeStartX = pos.X;
            _resizeStartY = pos.Y;
            _resizeOrigW = placed.Width;
            _resizeOrigH = placed.Height;
        };
        resizeGesture.PointerMoved += (_, e) =>
        {
            if (_resizing != placed) return;
            var pos = e.GetPosition(_canvas)!.Value;
            placed.Width = Math.Max(120, _resizeOrigW + (pos.X - _resizeStartX));
            placed.Height = Math.Max(80, _resizeOrigH + (pos.Y - _resizeStartY));
            AbsoluteLayout.SetLayoutBounds(card, new Rect(placed.X, placed.Y, placed.Width, placed.Height));
            body.Text = FormatBodyText(placed);
        };
        resizeGesture.PointerReleased += (_, _) =>
        {
            if (_resizing == placed) _resizing = null;
        };
        resizeHandle.GestureRecognizers.Add(resizeGesture);

        // ── Close button ─────────────────────────────────────────────────────
        var closeTap = new TapGestureRecognizer();
        closeTap.Tapped += (_, _) =>
        {
            _canvas.Children.Remove(card);
            _placedFeatures.Remove(placed);
        };
        closeBtn.GestureRecognizers.Add(closeTap);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static string FormatBodyText(PlacedFeature p) =>
        $"[{p.Feature.Name}]\nPosition : ({p.X:F0}, {p.Y:F0})\nSize     : {p.Width:F0} × {p.Height:F0}";
}
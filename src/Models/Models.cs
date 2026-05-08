// Feature.cs - Erweiterte Version
using Microsoft.Maui.Controls;

namespace HyperDev.src.Models;

public class Feature : BindableObject
{
    private string _name = string.Empty;
    private FeatureType _type;
    private object? _viewModel;
    private string _description = string.Empty;
    private Color _accentColor = Colors.SteelBlue;

    public Feature()
    {
        // Standardkonstruktor
    }

    public Feature(string name, FeatureType type, object? viewModel = null)
    {
        _name = name;
        _type = type;
        _viewModel = viewModel;
    }

    public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;
            _name = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public FeatureType Type
    {
        get => _type;
        set
        {
            if (_type == value) return;
            _type = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(Icon));
        }
    }

    public object? ViewModel
    {
        get => _viewModel;
        set
        {
            if (_viewModel == value) return;
            _viewModel = value;
            OnPropertyChanged();
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            if (_description == value) return;
            _description = value;
            OnPropertyChanged();
        }
    }

    public Color AccentColor
    {
        get => _accentColor;
        set
        {
            if (_accentColor == value) return;
            _accentColor = value;
            OnPropertyChanged();
        }
    }

    public string DisplayName => $"{Icon} {_name}";

    public string Icon => Type switch
    {
        FeatureType.Chart => "📊",
        FeatureType.Table => "📋",
        FeatureType.Metrics => "📈",
        FeatureType.Image => "🖼️",
        FeatureType.Text => "📝",
        FeatureType.Button => "🔘",
        FeatureType.Custom => "✨",
        _ => "📦"
    };

    // Factory-Methode zum Erstellen vorkonfigurierter Features
    public static Feature CreateChartFeature(string name = "Chart Feature")
    {
        return new Feature(name, FeatureType.Chart, new ChartFeatureViewModel())
        {
            Description = "Interaktives Diagramm mit Datenvisualisierung",
            AccentColor = Colors.SteelBlue
        };
    }

    public static Feature CreateTableFeature(string name = "Table Feature")
    {
        return new Feature(name, FeatureType.Table, new TableFeatureViewModel())
        {
            Description = "Daten-Tabelle mit sortierbaren Spalten",
            AccentColor = Colors.SeaGreen
        };
    }

    public static Feature CreateMetricsFeature(string name = "Metrics Feature")
    {
        return new Feature(name, FeatureType.Metrics, new MetricsFeatureViewModel())
        {
            Description = "Metriken und KPIs auf einen Blick",
            AccentColor = Colors.Orange
        };
    }

    public static Feature CreateImageFeature(string name = "Image Feature")
    {
        return new Feature(name, FeatureType.Image, new ImageFeatureViewModel())
        {
            Description = "Bild mit Anpassungsoptionen",
            AccentColor = Colors.Purple
        };
    }

    public static Feature CreateTextFeature(string name = "Text Feature")
    {
        return new Feature(name, FeatureType.Text, new TextFeatureViewModel())
        {
            Description = "Text-Block mit Formatierungsoptionen",
            AccentColor = Colors.Teal
        };
    }

    // Clone-Methode für Kopien
    public Feature Clone()
    {
        return new Feature
        {
            Name = this.Name,
            Type = this.Type,
            ViewModel = this.ViewModel, // Hinweis: ViewModel wird nicht tief kopiert
            Description = this.Description,
            AccentColor = this.AccentColor
        };
    }
}

// Feature-Typen Enum
public enum FeatureType
{
    Chart,
    Table,
    Metrics,
    Image,
    Text,
    Button,
    Custom
}

// Erweiterte Feature ViewModels
public class ChartFeatureViewModel : BindableObject
{
    private string _title = "Chart";
    private double _value = 75;
    private List<double> _dataPoints = new() { 30, 75, 45, 90, 60, 85 };
    private string _chartType = "Bar";

    public string Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(); }
    }

    public double Value
    {
        get => _value;
        set { _value = value; OnPropertyChanged(); }
    }

    public List<double> DataPoints
    {
        get => _dataPoints;
        set { _dataPoints = value; OnPropertyChanged(); }
    }

    public string ChartType
    {
        get => _chartType;
        set { _chartType = value; OnPropertyChanged(); }
    }
}

public class TableFeatureViewModel : BindableObject
{
    private int _rows = 5;
    private int _columns = 3;
    private List<List<string>> _data = new();

    public TableFeatureViewModel()
    {
        GenerateSampleData();
    }

    public int Rows
    {
        get => _rows;
        set
        {
            _rows = value;
            OnPropertyChanged();
            GenerateSampleData();
        }
    }

    public int Columns
    {
        get => _columns;
        set
        {
            _columns = value;
            OnPropertyChanged();
            GenerateSampleData();
        }
    }

    public List<List<string>> Data
    {
        get => _data;
        set { _data = value; OnPropertyChanged(); }
    }

    private void GenerateSampleData()
    {
        _data.Clear();
        for (int i = 0; i < _rows; i++)
        {
            var row = new List<string>();
            for (int j = 0; j < _columns; j++)
            {
                row.Add($"Zelle {i + 1},{j + 1}");
            }
            _data.Add(row);
        }
        OnPropertyChanged(nameof(Data));
    }
}

public class MetricsFeatureViewModel : BindableObject
{
    private int _metric1 = 42;
    private int _metric2 = 89;
    private string _metric1Label = "Umsatz";
    private string _metric2Label = "Kunden";
    private string _metric1Unit = "Mio €";
    private string _metric2Unit = "Tsd.";

    public int Metric1
    {
        get => _metric1;
        set { _metric1 = value; OnPropertyChanged(); }
    }

    public int Metric2
    {
        get => _metric2;
        set { _metric2 = value; OnPropertyChanged(); }
    }

    public string Metric1Label
    {
        get => _metric1Label;
        set { _metric1Label = value; OnPropertyChanged(); }
    }

    public string Metric2Label
    {
        get => _metric2Label;
        set { _metric2Label = value; OnPropertyChanged(); }
    }

    public string Metric1Unit
    {
        get => _metric1Unit;
        set { _metric1Unit = value; OnPropertyChanged(); }
    }

    public string Metric2Unit
    {
        get => _metric2Unit;
        set { _metric2Unit = value; OnPropertyChanged(); }
    }

    public string FormattedMetric1 => $"{_metric1} {_metric1Unit}";
    public string FormattedMetric2 => $"{_metric2} {_metric2Unit}";
}

public class ImageFeatureViewModel : BindableObject
{
    private string _imageUrl = "dotnet_bot.png";
    private string _caption = "Bildbeschreibung";
    private bool _showCaption = true;
    private double _opacity = 1.0;

    public string ImageUrl
    {
        get => _imageUrl;
        set { _imageUrl = value; OnPropertyChanged(); }
    }

    public string Caption
    {
        get => _caption;
        set { _caption = value; OnPropertyChanged(); }
    }

    public bool ShowCaption
    {
        get => _showCaption;
        set { _showCaption = value; OnPropertyChanged(); }
    }

    public double Opacity
    {
        get => _opacity;
        set { _opacity = value; OnPropertyChanged(); }
    }
}

public class TextFeatureViewModel : BindableObject
{
    private string _text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";
    private string _fontFamily = "Arial";
    private double _fontSize = 14;
    private Color _textColor = Colors.Black;
    private bool _isBold = false;
    private bool _isItalic = false;

    public string Text
    {
        get => _text;
        set { _text = value; OnPropertyChanged(); }
    }

    public string FontFamily
    {
        get => _fontFamily;
        set { _fontFamily = value; OnPropertyChanged(); }
    }

    public double FontSize
    {
        get => _fontSize;
        set { _fontSize = value; OnPropertyChanged(); }
    }

    public Color TextColor
    {
        get => _textColor;
        set { _textColor = value; OnPropertyChanged(); }
    }

    public bool IsBold
    {
        get => _isBold;
        set { _isBold = value; OnPropertyChanged(); }
    }

    public bool IsItalic
    {
        get => _isItalic;
        set { _isItalic = value; OnPropertyChanged(); }
    }

    public FontAttributes FontAttributes
    {
        get
        {
            var attributes = FontAttributes.None;
            if (_isBold) attributes |= FontAttributes.Bold;
            if (_isItalic) attributes |= FontAttributes.Italic;
            return attributes;
        }
    }
}

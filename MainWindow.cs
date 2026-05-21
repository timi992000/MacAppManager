using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

class MainWindow : Window
{
    private ListBox _listBox;
    private List<AppItem> _allApps;
    private TextBox _searchBox;
    private ComboBox _filterBox;
    private ComboBox _langCombo;
    private TextBlock _countLabel;
    private Button _startBtn, _killBtn, _uninstallBtn, _refreshBtn;

    public MainWindow()
    {
        var sysLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        L.Current = Array.IndexOf(L.Codes, sysLang) >= 0 ? sysLang : "en";

        Width  = 680;
        Height = 720;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        _allApps = AppHelper.GetApps();

        _searchBox = new TextBox { Margin = new Thickness(10, 10, 5, 10) };

        _filterBox = new ComboBox
        {
            SelectedIndex = 0,
            Margin = new Thickness(5, 10, 5, 10),
            Width = 160,
        };

        _langCombo = new ComboBox
        {
            ItemsSource   = L.Codes.Select(c => { var (f, n) = L.Languages[c]; return $"{f} {n}"; }).ToList(),
            SelectedIndex = Array.IndexOf(L.Codes, L.Current),
            Margin        = new Thickness(5, 10, 10, 10),
            Width         = 130,
        };

        _countLabel = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0),
        };

        var topPanel = new DockPanel();
        DockPanel.SetDock(_langCombo,   Dock.Right);
        DockPanel.SetDock(_filterBox,   Dock.Right);
        DockPanel.SetDock(_countLabel,  Dock.Right);
        topPanel.Children.Add(_langCombo);
        topPanel.Children.Add(_filterBox);
        topPanel.Children.Add(_countLabel);
        topPanel.Children.Add(_searchBox);

        _listBox = new ListBox
        {
            SelectionMode = SelectionMode.Multiple,
            Margin        = new Thickness(10, 0, 10, 10),
        };
        UpdateList(_allApps);

        _startBtn     = new Button { Margin = new Thickness(5), Background = Brushes.DarkGreen,  Foreground = Brushes.White };
        _killBtn      = new Button { Margin = new Thickness(5), Background = Brushes.DarkOrange, Foreground = Brushes.White };
        _uninstallBtn = new Button { Margin = new Thickness(5), Background = Brushes.DarkRed,    Foreground = Brushes.White };
        _refreshBtn   = new Button { Margin = new Thickness(5) };

        var buttonPanel = new StackPanel
        {
            Orientation         = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin              = new Thickness(10),
        };
        buttonPanel.Children.Add(_startBtn);
        buttonPanel.Children.Add(_killBtn);
        buttonPanel.Children.Add(_uninstallBtn);
        buttonPanel.Children.Add(_refreshBtn);

        var root = new DockPanel();
        DockPanel.SetDock(topPanel,    Dock.Top);
        DockPanel.SetDock(buttonPanel, Dock.Bottom);
        root.Children.Add(topPanel);
        root.Children.Add(buttonPanel);
        root.Children.Add(_listBox);
        Content = root;

        _searchBox.TextChanged        += (_, _) => ApplyFilters();
        _filterBox.SelectionChanged   += (_, _) => ApplyFilters();
        _langCombo.SelectionChanged   += (_, _) =>
        {
            if (_langCombo.SelectedIndex >= 0)
            {
                L.Current = L.Codes[_langCombo.SelectedIndex];
                ApplyLanguage();
            }
        };

        _refreshBtn.Click   += (_, _) => Refresh();
        _startBtn.Click     += (_, _) => ExecuteAction("start");
        _killBtn.Click      += (_, _) => ExecuteAction("kill");
        _uninstallBtn.Click += (_, _) => ExecuteAction("uninstall");

        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        Title = L.Get("title");
        _searchBox.PlaceholderText = L.Get("search");
        _filterBox.ItemsSource = new[] { L.Get("filterAll"), L.Get("filterRunning"), L.Get("filterStopped") };
        if (_filterBox.SelectedIndex < 0) _filterBox.SelectedIndex = 0;
        _startBtn.Content     = L.Get("start");
        _killBtn.Content      = L.Get("kill");
        _uninstallBtn.Content = L.Get("uninstall");
        _refreshBtn.Content   = L.Get("refresh");
        UpdateItemCount(_allApps.Count);
    }

    private void ApplyFilters()
    {
        var term  = _searchBox.Text?.ToLower() ?? "";
        var query = _allApps.Where(a => a.Name.ToLower().Contains(term));

        if (_filterBox.SelectedIndex == 1) query = query.Where(a =>  a.IsRunning);
        if (_filterBox.SelectedIndex == 2) query = query.Where(a => !a.IsRunning);

        var list = query.ToList();
        UpdateList(list);
        UpdateItemCount(list.Count);
    }

    private void UpdateList(List<AppItem> apps)
    {
        _listBox.ItemsSource = apps.Select(a =>
            $"{a.Name}  {(a.IsRunning ? "●" : "○")}").ToList();
        _listBox.Tag = apps;
    }

    private void UpdateItemCount(int count) =>
        _countLabel.Text = $"{count} {L.Get("apps")}";

    private void Refresh()
    {
        _allApps = AppHelper.GetApps();
        ApplyFilters();
    }

    private void ExecuteAction(string action)
    {
        if (_listBox.SelectedItems == null || _listBox.Tag is not List<AppItem> sourceApps) return;

        var selectedIndices = _listBox.SelectedItems.Cast<object>()
            .Select(item => _listBox.ItemsSource!.Cast<object>().ToList().IndexOf(item))
            .Where(i => i >= 0)
            .ToList();

        var selected = selectedIndices.Select(i => sourceApps[i]).ToList();

        foreach (var app in selected)
        {
            if (action == "start")     AppActions.Start(app);
            else if (action == "kill") AppActions.Kill(app);
            else                       AppActions.Uninstall(app);
        }

        Thread.Sleep(600);
        Refresh();
    }
}

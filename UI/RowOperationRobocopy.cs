using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Linq;

namespace robocopy_gui.UI {
  internal class RowOperationRobocopy {
    public int Index { get; set; }
    public Button SearchSourceButton { get; set; }
    public TextBox SourceText { get; set; }
    public Button SearchDestButton { get; set; }
    public TextBox DestText { get; set; }
    public Button ExclFilesButton { get; set; }
    public Button ExclFoldersButton { get; set; }
    public CheckBox Mirror { get; set; }
    public CheckBox Move { get; set; }
    public CheckBox OnlyNewer { get; set; }
    public CheckBox FATFileTime { get; set; }

    public RowOperationRobocopy(int operationIndex) {
      Index = operationIndex;

      PathIcon searchSourceIcon = new PathIcon{
        Data = Geometry.Parse(Icons.Search)
      };
      SearchSourceButton = new Button {
        Content = searchSourceIcon,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Width = 60,
        Tag = operationIndex
      };

      SourceText = new TextBox {
        Name = "source",
        Watermark = "Source",
        Text = MainWindow.OperationsList[operationIndex].SourceFolder,
        VerticalAlignment = VerticalAlignment.Top,
        Margin = new Thickness(0, 5, 0, 0),
        TextWrapping = TextWrapping.NoWrap,
        Tag = operationIndex
      };

      PathIcon searchDestIcon = new PathIcon {
        Data = Geometry.Parse(Icons.Search)
      };
      SearchDestButton = new Button {
        Content = searchDestIcon,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Width = 60,
        Tag = operationIndex
      };

      DestText = new TextBox {
        Name = "dest",
        Watermark = "Destination",
        Text = MainWindow.OperationsList[operationIndex].DestinationFolder,
        VerticalAlignment = VerticalAlignment.Top,
        Margin = new Thickness(0, 5, 0, 0),
        TextWrapping = TextWrapping.NoWrap,
        Tag = operationIndex
      };

      ExclFilesButton = new Button {
        Content = "Exclude\nFiles: " + MainWindow.OperationsList[operationIndex].ExcludeFiles.Count,
        FontSize = 10,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Width = 100,
        Tag = operationIndex
      };
      ExclFilesButton.Click += async (s, e) => {
        Button sender = s as Button ?? throw new Exception("Sender is null");
        int index = Convert.ToInt32(sender.Tag);
        DialogExclusions dialog = new DialogExclusions(MainWindow.OperationsList[index].ExcludeFiles);
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
          await dialog.ShowDialog(desktop.MainWindow);
        }
        if (!dialog.IsCancel) {
          MainWindow.OperationsList[index].ExcludeFiles = dialog.ReturnExclusions;
          sender.Content = "Exclude\nFiles: " + dialog.ReturnExclusions.Count;
        }
      };

      ExclFoldersButton = new Button {
        Content = "Exclude\nFolders: " + MainWindow.OperationsList[operationIndex].ExcludeFolders.Count,
        FontSize = 10,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Width = 100,
        Tag = operationIndex
      };
      ExclFoldersButton.Click += async (s, e) => {
        Button sender = s as Button ?? throw new Exception("Sender is null");
        int index = Convert.ToInt32(sender.Tag);
        DialogExclusions dialog = new DialogExclusions(MainWindow.OperationsList[index].ExcludeFolders);
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
          await dialog.ShowDialog(desktop.MainWindow);
        }
        if (!dialog.IsCancel) {
          MainWindow.OperationsList[index].ExcludeFolders = dialog.ReturnExclusions;
          sender.Content = "Exclude\nFiles: " + dialog.ReturnExclusions.Count;
        }
      };

      Mirror = new CheckBox {
        Name = "mirror",
        Content = "Mirror",
        IsChecked = MainWindow.OperationsList[operationIndex].IsMirror,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 5, 0),
        Tag = operationIndex
      };
      Mirror.SetValue(ToolTip.TipProperty,
        "Copy files to destination folder, removing files from the destination that are not present in the source folder.");
      Mirror.Unchecked += (sender, e) => {
        CheckBox s = sender as CheckBox ?? throw new Exception("Sender is null");
        int index = Convert.ToInt32(s.Tag);
        MainWindow.OperationsList[index].IsMirror = false;
      };

      Move = new CheckBox {
        Name = "move",
        Content = "Move",
        IsChecked = MainWindow.OperationsList[operationIndex].IsMove,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0,0,5,0),
        Tag = operationIndex
      };
      Move.SetValue(ToolTip.TipProperty,
        "Move files to the destination folder rather than copying them, removing them from the source folder.");
      Move.Unchecked += (sender, e) => {
        CheckBox s = sender as CheckBox ?? throw new Exception("Sender is null");
        int index = Convert.ToInt32(s.Tag);
        MainWindow.OperationsList[index].IsMove = false;
      };

      OnlyNewer = new CheckBox {
        Content = "Only newer",
        IsChecked = MainWindow.OperationsList[operationIndex].IsOnlyIfNewer,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 5, 0),
        Tag = operationIndex
      };
      OnlyNewer.SetValue(ToolTip.TipProperty,
        "Copy or move files to the destination folder only if the source file is newer than the target file.");
      OnlyNewer.Unchecked += (sender, e) => {
        CheckBox s = sender as CheckBox ?? throw new Exception("Sender is null");
        int index = Convert.ToInt32(s.Tag);
        MainWindow.OperationsList[index].IsOnlyIfNewer = false;
      };

      FATFileTime = new CheckBox {
        Name = "FATtime",
        Content = "FAT-Time",
        IsChecked = MainWindow.OperationsList[operationIndex].IsOnlyIfNewer,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Tag = operationIndex
      };
      FATFileTime.SetValue(ToolTip.TipProperty,
        "Use FAT-style time format when writing file. Useful when copying to another file system and when using \"only newer files\".");
      FATFileTime.Checked += (sender, e) => {
        CheckBox s = sender as CheckBox ?? throw new Exception("Sender is null");
        int index = Convert.ToInt32(s.Tag);
        MainWindow.OperationsList[index].IsUseFATTime = true;
      };
      FATFileTime.Unchecked += (sender, e) => {
        CheckBox s = sender as CheckBox ?? throw new Exception("Sender is null");
        int index = Convert.ToInt32(s.Tag);
        MainWindow.OperationsList[index].IsUseFATTime = false;
      };
    }
  }
}

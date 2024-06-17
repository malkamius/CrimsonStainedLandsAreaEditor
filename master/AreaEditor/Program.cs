using System.Data.Common;
using CrimsonStainedLands;
using Gdk;
using Gtk;
using MUDMapBuilder;
using SkiaSharp;

if(System.IO.Directory.GetCurrentDirectory().Contains(System.IO.Path.Join("bin", "Debug", "net8.0")))
    System.IO.Directory.SetCurrentDirectory(System.IO.Path.Join("..", "..", ".."));
AreaEditorProgram.Main();

class AreaEditorProgram {
    public static void Main() {
        Application.Init();
        MainWindow mainWindow = new MainWindow();
        mainWindow.ShowAll();
        Application.Run();
    }
}

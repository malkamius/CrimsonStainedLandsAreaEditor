using Gdk;
using Gtk;

partial class MainWindow : Gtk.Window {
    public TreeView NavigatorTreeView;
    public ScrolledWindow NavigatorScrollWindow;
    public Grid MainGrid;
    public ScrolledWindow MapScrollWindow;
    public Overlay OverlayControl;
    public PanEventBox MapImageEventBox;
    public Image MapImage;
    public Spinner MapSpinner;

    internal void InitializeComponents() {
        this.Resizable = true;
        
        NavigatorTreeView = new TreeView();
        
        NavigatorTreeView.HeadersVisible = false;
        
        
        NavigatorTreeView.ActivateOnSingleClick = true;
        NavigatorTreeView.RowActivated += NavigatorTreeView_RowActivated;
        NavigatorTreeView.Selection.Changed += NavigatorTreeView_SelectionChanged;

        NavigatorScrollWindow = new Gtk.ScrolledWindow();
        NavigatorScrollWindow.MaxContentHeight = 700;
        NavigatorScrollWindow.MaxContentWidth = 300;
        NavigatorScrollWindow.Hexpand = false;
        NavigatorScrollWindow.WidthRequest = 300;
        NavigatorScrollWindow.Vexpand = true;

        NavigatorScrollWindow.Add(NavigatorTreeView);
        
        MapScrollWindow = new Gtk.ScrolledWindow();
        
        MapScrollWindow.MaxContentHeight = 700;
        MapScrollWindow.MaxContentWidth = 500;
        MapScrollWindow.Vexpand = true;
        MapScrollWindow.WidthRequest = 500;
        
        MapImageEventBox = new PanEventBox(MapScrollWindow);
        MapImageEventBox.MouseClick += MapImageEventBox_Click;

        MapImage = new Image();
        MapImageEventBox.Add(MapImage);
        
        MapSpinner = new Spinner();
        MapSpinner.WidthRequest = 100;
        MapSpinner.HeightRequest = 100;
        MapSpinner.Halign = Align.Center;
        MapSpinner.Valign = Align.Center;
        MapSpinner.Visible = false;

        MainGrid = new Grid();
        OverlayControl = new Overlay();
        OverlayControl.Add(MapScrollWindow);
        OverlayControl.AddOverlay(MapSpinner);
        
        MainGrid.Add(NavigatorScrollWindow);
        MainGrid.Add(OverlayControl);
        MainGrid.WidthRequest = 900;
        MainGrid.HeightRequest = 800;
        
        this.Add(MainGrid);
    }
}
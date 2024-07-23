using Gdk;
using Gtk;

partial class MainWindow : Gtk.Window {
    private TreeView NavigatorTreeView;
    private ScrolledWindow NavigatorScrollWindow;
    private Grid MainGrid;
    private ScrolledWindow MapScrollWindow;
    private Overlay OverlayControl;
    private PanEventBox MapImageEventBox;
    private Image MapImage;
    private Spinner MapSpinner;

    private Button SaveAreasButton;

    private RoomEditorPanel RoomEditorPanel;
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
        MapScrollWindow.Hexpand = true;
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
        MapSpinner.NoShowAll = true;
        
        MainGrid = new Grid();
        OverlayControl = new Overlay();
        OverlayControl.Add(MapScrollWindow);
        OverlayControl.AddOverlay(MapSpinner);
        
        SaveAreasButton = new Button("Save Areas");
        SaveAreasButton.Clicked += SaveAreasButton_Clicked;        

        MainGrid.Attach(NavigatorScrollWindow, 0, 0, 1, 1);
        MainGrid.Attach(OverlayControl, 1, 0, 1, 2);
        MainGrid.AttachNextTo(SaveAreasButton, NavigatorScrollWindow, PositionType.Bottom, 1, 1);
        this.RoomEditorPanel = new RoomEditorPanel();
        MainGrid.Attach(this.RoomEditorPanel, 2, 0, 1, 1);
        this.RoomEditorPanel.PropertyChanged += RoomEditorPanel_PropertyChanged;
        //MainGrid.WidthRequest = 900;
        MainGrid.HeightRequest = 800;
        MainGrid.Margin = 5;
        this.Add(MainGrid);
    }

    
}
using System.ComponentModel;
using CrimsonStainedLands;
using CrimsonStainedLands.Extensions;
using Gtk;

public class RoomEditorPanel : Gtk.Grid, INotifyPropertyChanged {
    private Gtk.Grid RoomPropertiesGrid;

    private Gtk.Label VNumLabel;
    private Gtk.Entry VNumEntry;

    private Gtk.Label NameLabel;
    private Gtk.Entry NameEntry;

    private Gtk.Label DescriptionLabel;
    private Gtk.TextView DescriptionTextView;
    private Gtk.ScrolledWindow DescriptionScrollWindow;

    private Gtk.ComboBox ExitSelector;
    private Entry ExitDestinationVNum;
    private Label ExitDestinationVNumLabel;
    private Label ExitDescriptionLabel;
    private Entry ExitDescription;
    private Label ExitDisplayLabel;
    private Entry ExitDisplay;
    private Label ExitKeywordsLabel;
    private Entry ExitKeywords;
    private Label ExitFlagsLabel;
    private CheckedListBox ExitFlags;
    private Label ExitKeysLabel;
    private Entry ExitKeys;
    
    private Label ExitSizeLabel;
    private ComboBox ExitSizeSelector;

    private Gtk.ScrolledWindow ExitDestinationSelectorScrollWindow;
    private Gtk.TreeView ExitDestinationSelector;
    
    private RoomData? editingRoom;

    public event PropertyChangedEventHandler? PropertyChanged;

    public RoomData? EditingRoom {
        get => editingRoom; 
        set {
            if(editingRoom != value) {
                editingRoom = value;
                RoomChanged();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditingRoom"));
                //ExitSelector.Active = 0;
                
            }
        }
    }

    public RoomEditorPanel() {
        RoomPropertiesGrid = new Grid();

        VNumLabel = new Label("VNum: ");
        VNumLabel.Xalign = 0;

        VNumEntry = new Entry();
        //VNumEntry.Hexpand = false;

        NameLabel = new Label("Name: ");
        NameLabel.Xalign = 0;

        NameEntry = new Entry();
        NameEntry.FocusOutEvent += NameEntry_FocusOut;
        //NameEntry.Hexpand = true;

        DescriptionLabel = new Label("Description: ");
        DescriptionLabel.Xalign = 0;

        DescriptionTextView = new TextView();
        var textbuffer = new TextBuffer(new TextTagTable());
        DescriptionTextView.Buffer = textbuffer;
        
        DescriptionScrollWindow = new ScrolledWindow();
        DescriptionScrollWindow.MinContentHeight = 100;
        DescriptionScrollWindow.MaxContentHeight = 100;
        DescriptionScrollWindow.MinContentWidth = 300;
        DescriptionScrollWindow.MaxContentWidth = 300;
        
        Gtk.CssProvider cssProvider = new Gtk.CssProvider();
        cssProvider.LoadFromData(@"*.my-textview-border { border: 1px solid black;}");
        
        DescriptionScrollWindow.StyleContext.AddProvider(cssProvider, Gtk.StyleProviderPriority.Application);
        DescriptionScrollWindow.StyleContext.AddClass("my-textview-border");

        ExitSelector = new ComboBox(new string[] {"North", "East", "South", "West", "Up", "Down"});
        ExitSelector.Changed += ExitSelectorChanged;

        ExitDestinationVNum = new Entry();
        ExitDestinationVNum.FocusGrabbed += ExitDestinationVNum_FocusGrabbed;
        ExitDestinationVNum.FocusOutEvent += ExitDestinationVNum_FocusOut;
        ExitDestinationVNum.Changed += ExitDestinationVNum_Changed;
        ExitDestinationVNum.ButtonPressEvent += ExitDestinationVNum_ButtonPressEvent;
        ExitDestinationVNumLabel = new Label();

        ExitDestinationSelector = new TreeView();
        var renderer = new CellRendererText();
        var column = new TreeViewColumn();
        column.Title = "Name";
        column.PackStart(renderer, false);
        column.SetCellDataFunc(renderer, RenderMyNodeText);
        ExitDestinationSelector.AppendColumn(column);
        ExitDestinationSelector.Model = new TreeStore(typeof(MyRoomNode));
        ExitDestinationSelector.HeadersVisible = false;
        ExitDestinationSelector.Selection.Changed += ExitDestinationSelector_SelectionChanged;
        
        ExitDestinationSelectorScrollWindow = new ScrolledWindow();
        //ExitDestinationSelectorScrollWindow.MaxContentHeight = 100;
        //ExitDestinationSelectorScrollWindow.MinContentWidth = 100;
        ExitDestinationSelectorScrollWindow.Add(ExitDestinationSelector);
        ExitDestinationSelector.Vexpand = true;
        ExitDestinationSelectorScrollWindow.Visible = false;
        ExitDestinationSelectorScrollWindow.NoShowAll = true;
        DescriptionScrollWindow.Add(DescriptionTextView);

        ExitDescriptionLabel = new Label("Description");
        ExitDescription = new Entry();
        ExitDisplayLabel = new Label("Display");
        ExitDisplay = new Entry();
        ExitKeywordsLabel = new Label("Keywords");
        ExitKeywords = new Entry();
        ExitKeysLabel = new Label("Keys");
        ExitKeys = new Entry();
        ExitFlagsLabel = new Label("Flags");
        ExitFlags = new CheckedListBox();
        ExitSizeLabel = new Label("Exit Size");
        ExitSizeSelector = new ComboBox((from value in Enum.GetValues<CharacterSize>() select value.ToString()).ToArray());

        Enum.GetValues<ExitFlags>().ToList().ForEach(flag => ExitFlags.AddItem(flag.ToString()));
        
        RoomPropertiesGrid.Attach(VNumLabel, 0, 0, 1, 1);
        RoomPropertiesGrid.AttachNextTo(VNumEntry, VNumLabel, PositionType.Right, 1, 1);
        RoomPropertiesGrid.AttachNextTo(NameLabel, VNumLabel, PositionType.Bottom, 1, 1);
        RoomPropertiesGrid.AttachNextTo(NameEntry, NameLabel, PositionType.Right, 1, 1);

        RoomPropertiesGrid.AttachNextTo(DescriptionLabel, NameLabel, PositionType.Bottom, 2, 1);
        
        RoomPropertiesGrid.AttachNextTo(DescriptionScrollWindow, DescriptionLabel, PositionType.Bottom, 2, 2);
        RoomPropertiesGrid.AttachNextTo(ExitSelector, DescriptionScrollWindow, PositionType.Bottom, 2, 1);
        RoomPropertiesGrid.AttachNextTo(ExitDestinationVNum, ExitSelector, PositionType.Bottom, 2, 1);
        RoomPropertiesGrid.AttachNextTo(ExitDestinationVNumLabel, ExitDestinationVNum, PositionType.Bottom, 2, 1);
        
        RoomPropertiesGrid.AttachNextTo(ExitDescriptionLabel, ExitDestinationVNumLabel, PositionType.Bottom, 1, 1);
        RoomPropertiesGrid.AttachNextTo(ExitDescription, ExitDescriptionLabel, PositionType.Right, 1, 1);
        RoomPropertiesGrid.AttachNextTo(ExitDisplayLabel, ExitDescriptionLabel, PositionType.Bottom, 1, 1);
        RoomPropertiesGrid.AttachNextTo(ExitDisplay, ExitDescription, PositionType.Bottom, 1, 1);
        RoomPropertiesGrid.AttachNextTo(ExitKeywordsLabel, ExitDisplayLabel, PositionType.Bottom, 1, 1);
        RoomPropertiesGrid.AttachNextTo(ExitKeywords, ExitDisplay, PositionType.Bottom, 1, 1);
        RoomPropertiesGrid.AttachNextTo(ExitKeysLabel, ExitKeywordsLabel, PositionType.Bottom, 1, 1);
        RoomPropertiesGrid.AttachNextTo(ExitKeys, ExitKeywords, PositionType.Bottom, 1, 1);
        
        RoomPropertiesGrid.AttachNextTo(ExitSizeLabel, ExitKeysLabel, PositionType.Bottom, 1, 1);
        RoomPropertiesGrid.AttachNextTo(ExitSizeSelector, ExitKeys, PositionType.Bottom, 1, 1);

        RoomPropertiesGrid.AttachNextTo(ExitFlagsLabel, ExitSizeLabel, PositionType.Bottom, 2, 1);
        RoomPropertiesGrid.AttachNextTo(ExitFlags, ExitFlagsLabel, PositionType.Bottom, 2, 1);

        RoomPropertiesGrid.AttachNextTo(ExitDestinationSelectorScrollWindow, ExitDestinationVNum, PositionType.Bottom, 2, 5);
        

        RoomPropertiesGrid.Margin = 5;
        //RoomPropertiesGrid.Vexpand = true;
        //this.Vexpand = true;
        
        this.Margin = 5;
        this.Attach(RoomPropertiesGrid, 0, 0, 1, 1);
        
    }

    private void ExitDestinationVNum_ButtonPressEvent(object o, ButtonPressEventArgs args)
    {
         if (args.Event.Type == Gdk.EventType.TwoButtonPress)
        {
            ExitDestinationSelectorScrollWindow.NoShowAll = false;
            ExitDestinationSelectorScrollWindow.Visible = true;
            ExitDestinationSelectorScrollWindow.ShowAll();
            ScrollToExitVNum();
        }
    }

    private void ExitDestinationSelector_SelectionChanged(object? sender, EventArgs e)
    {
        ExitDestinationSelector.Selection.GetSelected(out var model, out var iter);
        if(model.GetValue(iter, 0) is MyRoomNode node && node?.Room != null) {
            ExitDestinationVNum.Text = node.Room.Vnum.ToString();
            ExitDestinationVNum.SelectRegion(0, ExitDestinationVNum.Text.Length);
            ExitDestinationVNum.GrabFocus();
        }
    }

    private void ExitSelectorChanged(object? sender, EventArgs e)
    {
        ExitSelector.GetActiveIter(out var iter);
        
        if(EditingRoom != null && 
            ExitSelector.Model.GetValue(iter, 0) is string directionstring && 
            CrimsonStainedLands.Extensions.Utility.GetEnumValueStrPrefixOut<Direction>(directionstring, out var outdirection)) {
            var exit = EditingRoom.GetExit(outdirection);

            if(exit != null) {
                ExitDestinationVNum.Text = $"{exit.destinationVnum}";
                ExitDisplay.Text = $"{exit.display}";
                ExitDescription.Text = $"{exit.description}";
                ExitKeys.Text = string.Join(", ", exit.keys);
                
                ExitFlags.ClearItems();
                Enum.GetValues<ExitFlags>().ToList().ForEach(flag => ExitFlags.AddItem(flag.ToString(), exit.flags.ISSET(flag)));

                ExitSizeSelector.Model.GetIterFirst(out var sizeiter);
                do {
                if(ExitSizeSelector.Model.GetValue(sizeiter, 0) is string ex && ex == exit.ExitSize.ToString()) {
                    ExitSizeSelector.SetActiveIter(sizeiter);
                } } while(ExitSizeSelector.Model.IterNext(ref sizeiter));
            } else {
                ExitDestinationVNum.Text = "";
                ExitDisplay.Text = "";
                ExitDescription.Text = "";
                ExitKeys.Text = "";
                ExitFlags.ClearItems();
                Enum.GetValues<ExitFlags>().ToList().ForEach(flag => ExitFlags.AddItem(flag.ToString()));

                ExitSizeSelector.Model.GetIterFirst(out var sizeiter);
                do {
                if(ExitSizeSelector.Model.GetValue(sizeiter, 0) is string ex && ex == CharacterSize.Medium.ToString()) {
                    ExitSizeSelector.SetActiveIter(sizeiter);
                } } while(ExitSizeSelector.Model.IterNext(ref sizeiter));
            }
        }
    }

    private void ExitDestinationVNum_Changed(object? sender, EventArgs e)
    {
        ScrollToExitVNum();
    }

    private void ExitDestinationVNum_FocusGrabbed(object? sender, EventArgs e)
    {        
        ScrollToExitVNum();
    }

    private void ProcessPendingEvents()
    {
        while (Gtk.Application.EventsPending())
            Gtk.Application.RunIteration(true);
    }

    private void ScrollToExitVNum()
    {
        if(int.TryParse(ExitDestinationVNum.Text, out var vnum) && RoomNodes.TryGetValue(vnum, out var roomNode))
        {
            if (!ExitDestinationSelector.IsRealized)
            {
                ExitDestinationSelector.Realize();
            }
            ProcessPendingEvents();
            ExitDestinationSelector.Selection.SelectIter(roomNode.Iter);
            var cellarea = ExitDestinationSelector.GetCellArea(roomNode.Path, ExitDestinationSelector.Columns.First());
            ExitDestinationSelector.GetAllocatedSize(out var selectorsize, out var navbaseline);
            //ExitDestinationSelectorScrollWindow.Vadjustment.Value += cellarea.Top - selectorsize.Height / 2;
            //ExitDestinationSelectorScrollWindow.Hadjustment.Value = 0;
            GLib.Idle.Add(() =>
            {
                ExitDestinationSelectorScrollWindow.Vadjustment.Value = 
                    Math.Max(0, Math.Min(cellarea.Top - selectorsize.Height / 2, 
                        ExitDestinationSelectorScrollWindow.Vadjustment.Upper - ExitDestinationSelectorScrollWindow.Vadjustment.PageSize));
                ExitDestinationSelectorScrollWindow.Hadjustment.Value = 0;
                return false; // return false to remove the idle handler
            });
            ExitDestinationVNumLabel.Text = $"({roomNode.Room.Vnum}) - {roomNode.Room.Name}";
        }
        else {
            ExitDestinationSelector.Selection.UnselectAll();
            ExitDestinationVNumLabel.Text = "";
        }
    }

    private void ExitDestinationVNum_FocusOut(object o, FocusOutEventArgs args)
    {
        ExitDestinationSelectorScrollWindow.NoShowAll = true;
        ExitDestinationSelectorScrollWindow.Visible = false;
    }

    private void NameEntry_FocusOut(object o, FocusOutEventArgs args)
    {
        
        if(PropertyChangedEvents && editingRoom != null) {
            var newValue = NameEntry.Text.Trim();
            if(newValue != editingRoom.Name) {
                editingRoom.Name = newValue;
                editingRoom.Area.saved = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditingRoom.Name"));
            }
        }
    }

    private bool PropertyChangedEvents = true;

    private void RoomChanged() {
        PropertyChangedEvents = false;
        if(editingRoom != null) {
            VNumEntry.Text = editingRoom.Vnum.ToString();
            NameEntry.Text = editingRoom.Name ?? string.Empty;
            DescriptionTextView.Buffer.Text = editingRoom.Description ?? string.Empty;
        } else {
            VNumEntry.Text = string.Empty;
            NameEntry.Text = string.Empty;
            DescriptionTextView.Buffer.Text = string.Empty;
        }
        PropertyChangedEvents = true;
        ExitSelectorChanged(this, new EventArgs() {});
    }
    public Dictionary<int, MyRoomNode> RoomNodes = new Dictionary<int, MyRoomNode>();

    public void UpdateRooms() {
        var store = (TreeStore) (ExitDestinationSelector.Model);
        var i = 0;
        if(store != null) {
            RoomNodes.Clear();
            store.Clear();
            foreach(var room in RoomData.Rooms) {
                var node = new MyRoomNode(room.Value);
                node.Iter = store.AppendValues(node);
                node.Path = store.GetPath(node.Iter);
                RoomNodes.Add(node.Room.Vnum, node);
                i++;
            }
        }
    }

    public class MyRoomNode {
        public RoomData Room {get; private set;}
        public TreeIter Iter {get;set;}
        public TreePath Path { get; internal set; }

        public MyRoomNode(RoomData value) {
            Room = value;
        }
    }

    
            
    private void RenderMyNodeText(TreeViewColumn tree_column, CellRenderer cell, ITreeModel tree_model, TreeIter iter)
    {
        if(cell is CellRendererText renderer && tree_model.GetValue (iter, 0) is MyRoomNode node)
        {
            
            renderer.Text = $"({node.Room.Vnum}) {node.Room.Name}";
            
        }
    }

}
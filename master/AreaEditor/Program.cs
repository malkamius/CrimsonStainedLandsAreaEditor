using CrimsonStainedLands;
using Gdk;
using Gtk;
using MUDMapBuilder;
using SkiaSharp;

Hello.Main();
    
class MyWindow : Gtk.Window {
    
    class MyNode {
        public object Value {get;set;}
        public TreePath? Path {get;set;}

        public MyNode(object value)
        {
            this.Value = value;
        }
    }
    public TreeView NavigatorTreeView;
    public ScrolledWindow NavigatorScrollWindow;
    public Grid MainGrid;
    public ScrolledWindow MapScrollWindow;
    public Overlay OverlayControl;
    public PanEventBox MapImageEventBox;
    public Image MapImage;
    public Spinner MapSpinner;
    private Dictionary<AreaData, MyNode> AreaNodes = new Dictionary<AreaData, MyNode>();
    private Dictionary<int, MyNode> RoomNodes = new Dictionary<int, MyNode>();
    public MyWindow() : base("Initializing") {
        CrimsonStainedLands.Settings.Load();
        WeaponDamageMessage.LoadWeaponDamageMessages();
        Race.LoadRaces();
        SkillSpellGroup.LoadSkillSpellGroups();
        GuildData.LoadGuilds();
        WeaponDamageMessage.LoadWeaponDamageMessages();
        AreaData.LoadAreas();
        this.Title = $"{AreaData.Areas.Count} areas loaded, ({RoomData.Rooms.Count} rooms)";
        this.Resizable = true;



        Gtk.TreeStore AreasStore = new Gtk.TreeStore (typeof (MyNode));
        
        AreaData.Areas.OrderBy(a => a.Name).ToList().ForEach(area => {
            var areaNode = new MyNode(area);
            var areaIter = AreasStore.AppendValues(areaNode);
            areaNode.Path = AreasStore.GetPath(areaIter);
            AreaNodes.Add(area, areaNode);
            area.Rooms.Values.ToList().ForEach(room => {
                    var roomNode = new MyNode(room);
                    var roomIter = AreasStore.AppendValues(areaIter, roomNode);
                    roomNode.Path = AreasStore.GetPath(roomIter);
                    RoomNodes.TryAdd(room.Vnum, roomNode);
                });

            AreasStore.AppendValues(areaIter, new MyNode("NPCs"));
            AreasStore.AppendValues(areaIter, new MyNode("Items"));
            AreasStore.AppendValues(areaIter, new MyNode("Resets"));
        });
        
        NavigatorTreeView = new TreeView(AreasStore);
        
        NavigatorTreeView.HeadersVisible = false;
        
        var name = new Gtk.CellRendererText();
        var column = new Gtk.TreeViewColumn();
        column.Title = "Text";
        column.PackStart(name, false);
        column.SetCellDataFunc(name, RenderAreaName);

        NavigatorTreeView.AppendColumn(column);
        NavigatorTreeView.ActivateOnSingleClick = true;
        NavigatorTreeView.RowActivated += NavigatorTreeView_RowActivated;
        NavigatorScrollWindow = new Gtk.ScrolledWindow();
        NavigatorScrollWindow.MaxContentHeight = 700;
        NavigatorScrollWindow.MaxContentWidth = 300;
        NavigatorScrollWindow.Hexpand = false;
        NavigatorScrollWindow.WidthRequest = 300;
        NavigatorScrollWindow.Vexpand = true;

        NavigatorScrollWindow.Add(NavigatorTreeView);

        var buffer = System.IO.File.ReadAllBytes("DroidSans-Bold.svg");
        var pixbuf = new Gdk.Pixbuf(buffer);
        MapImage = new Image();
        MapImage.Pixbuf = pixbuf;

        MapScrollWindow = new Gtk.ScrolledWindow();
        
        MapScrollWindow.MaxContentHeight = 700;
        MapScrollWindow.MaxContentWidth = 500;
        MapScrollWindow.Vexpand = true;
        MapScrollWindow.WidthRequest = 500;
        
        MapImageEventBox = new PanEventBox(MapScrollWindow);
        MapImageEventBox.MouseClick += MapImageEventBox_Click;
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

    private void MapImageEventBox_Click(object sender, ButtonReleaseEventArgs args)
    {
        if(AreaMap != null) {
            float xoffset = 0;
            float yoffset = 0;
            MapImageEventBox.GetAllocatedSize(out var alloc, out var baseline);
            
            MapImage.GetAllocatedSize(out var alloc2, out var baseline2);
            if(alloc2.Width > MapImage.Pixbuf.Width)
                xoffset = (alloc2.Width - MapImage.Pixbuf.Width) / 2;
            if(alloc2.Height > MapImage.Pixbuf.Height)
                yoffset = (alloc2.Height - MapImage.Pixbuf.Height) / 2;
            //var r = AreaMap.Rooms.FirstOrDefault(ar => ar.Rectangle.Contains(new System.Drawing.Point((int)(args.Event.X * (float) MapImage.Pixbuf.Width / alloc.Width) , (int)(args.Event.Y * (float) MapImage.Pixbuf.Height / alloc.Height))));
            var r = AreaMap.Rooms.FirstOrDefault(ar => ar.Rectangle.Contains(new System.Drawing.Point((int)(args.Event.X - xoffset) , (int)(args.Event.Y - yoffset))));
            if(r != null && RoomNodes.TryGetValue(r.Room.Id, out var room))
            {
                NavigatorTreeView.ExpandToPath(room.Path);
                NavigatorTreeView.Selection.SelectPath(room.Path);
                var mmboptions = new MUDMapBuilder.BuildOptions();
                mmboptions.MaxSteps = 1000;
                mmboptions.RemoveRoomsWithSingleOutsideExit = false;
                mmboptions.RemoveSolitaryRooms = false;
                if(MarkedRoom != null)
                    MarkedRoom.MarkColor = OriginalColor;
                OriginalColor = r.Room.MarkColor;
                MarkedRoom = r.Room;
                r.Room.MarkColor = SKColors.Red;
                if(BuiltArea != null)
                {
                    if(MapImage.Pixbuf != null) {
                        MapImage.Pixbuf.Dispose();
                    }

                    AreaMap = BuiltArea.BuildPng(mmboptions, true);
                    MapImage.Pixbuf = new Pixbuf(AreaMap.PngData);
                }
            }
        }
    }
    private MMBRoom MarkedRoom;
    private SKColor? OriginalColor;
    private MMBImageResult? AreaMap;
    private MMBArea? BuiltArea;

    private void BuildMap(AreaData area) {
        var mmboptions = new MUDMapBuilder.BuildOptions();
        mmboptions.MaxSteps = 1000;
        mmboptions.RemoveRoomsWithSingleOutsideExit = false;
        mmboptions.RemoveSolitaryRooms = false;

        var mmbarea = new MUDMapBuilder.MMBArea();

        var roomdict = new Dictionary<int, (RoomData, MMBRoom)>();
        var arearooms = new List<MMBRoom>();
        foreach(var room in area.Rooms.Values) {
            var mmbroom = new MUDMapBuilder.MMBRoom(room.Vnum, room.Name, false);
            //arearooms.Add(mmbroom);    
            roomdict.TryAdd(room.Vnum, (room, mmbroom));
            foreach(var exit in room.Exits) {
                if(exit != null && exit.destination != null) {
                    var mmbdest = new MUDMapBuilder.MMBRoom(exit.destination.Vnum, exit.destination.Name, exit.destination.Area != room.Area);
                    roomdict.TryAdd(exit.destination.Vnum, (exit.destination, mmbdest));
                    
                }
            }
        }
        var reversedir = new MMBDirection[] { 
            MMBDirection.South, 
            MMBDirection.West, 
            MMBDirection.North, 
            MMBDirection.East, 
            MMBDirection.Down, 
            MMBDirection.Up };
        foreach(var rval in roomdict.Values) {
            //arearooms.Add(rval.Item2);
            
            //if(rval.Item1.Vnum != 3760) {
            foreach(var exit in rval.Item1.Exits) {
                if(exit != null && exit.destination != null && roomdict.ContainsKey(exit.destination.Vnum)) {
                    var backexit = exit.destination.Exits[(int)reversedir[(int)exit.direction]];
                    
                    rval.Item2.Connections.Add((MMBDirection) exit.direction, new MMBRoomConnection() {
                            ConnectionType = backexit != null && backexit.destination == rval.Item1? MMBConnectionType.TwoWay : MMBConnectionType.Forward,
                            Direction = (MMBDirection) exit.direction, 
                            RoomId = exit.destination.Vnum
                    });
                }
            }
            mmbarea.Add(rval.Item2);
            //}
        }
        
        var mmbproj = new MUDMapBuilder.MMBProject(mmbarea, mmboptions);
        var log = new Action<string>((str) => {});
        var result = MapBuilder.MultiRun(mmbproj, log);
        
        BuiltArea = result.History.Last();
        AreaMap = BuiltArea.BuildPng(mmboptions, true);
        MapImage.Pixbuf = new Pixbuf(AreaMap.PngData);
        
    }

    private async void NavigatorTreeView_RowActivated(object o, RowActivatedArgs args)
    {
        var t = args.Path;
        if(NavigatorTreeView.Model.GetIter(out var iter, t))
        {
            if ( NavigatorTreeView.Model.GetValue( iter, 0 ) is MyNode node ) {
                var data = node.Value;

                if(data is AreaData area) {
                    
                    if(MapImage.Pixbuf != null)
                        MapImage.Pixbuf.Dispose();
                    MapImage.Pixbuf = null;
                    NavigatorTreeView.Sensitive = false;
                    MapSpinner.Visible = true;
                    MapSpinner.Start();
                    await Task.Run(() => BuildMap(area));
                    MapSpinner.Stop();
                    MapSpinner.Visible = false;
                    NavigatorTreeView.Sensitive = true;
                }
            }
            
        }
    }

    private void RenderAreaName(TreeViewColumn tree_column, CellRenderer cell, ITreeModel tree_model, TreeIter iter)
    {
        if(cell is CellRendererText renderer && tree_model.GetValue (iter, 0) is MyNode node)
        {
            var data = node.Value;
            if(data is AreaData area)
            {
                renderer.Text = area.Name;
            }
            else if(data is RoomData room) 
            {
                renderer.Text = $"({room.Vnum}) {room.Name}";
            }
            else if(data is string text) 
            {
                renderer.Text = text;
            }
        }
    }

    protected override bool OnDeleteEvent(Event e) {
        Application.Quit();
        return true;
    }
}

class Hello {
    public static void Main() {
        Application.Init();
        MyWindow w = new MyWindow();
        w.ShowAll();
        Application.Run();
    }
}

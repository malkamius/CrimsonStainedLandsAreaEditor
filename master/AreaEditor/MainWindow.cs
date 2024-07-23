using System.ComponentModel;
using System.Diagnostics;
using CrimsonStainedLands;
using CrimsonStainedLands.Extensions;
using Gdk;
using Gtk;
using MUDMapBuilder;
using SkiaSharp;

partial class MainWindow : Gtk.Window {

    
    class MyNode {
        public object Value {get;set;}
        public TreePath? Path {get;set;}

        public MyNode(object value)
        {
            this.Value = value;
        }
    }
    private MMBRoom? MarkedRoom;
    private SKColor? OriginalColor;
    private MMBImageResult? AreaMap;
    private MMBArea? BuiltArea;
    private Dictionary<AreaData, MyNode> AreaNodes = new Dictionary<AreaData, MyNode>();
    private Dictionary<int, MyNode> RoomNodes = new Dictionary<int, MyNode>();

    public MainWindow() : base("Initializing") {
        this.InitializeComponents();
        this.ShowNow();
        CrimsonStainedLands.Settings.Load();
        WeaponDamageMessage.LoadWeaponDamageMessages();
        Race.LoadRaces();
        SkillSpellGroup.LoadSkillSpellGroups();
        GuildData.LoadGuilds();
        WeaponDamageMessage.LoadWeaponDamageMessages();
        AreaData.LoadAreas();
        this.Title = $"{AreaData.Areas.Count} areas loaded, ({RoomData.Rooms.Count} rooms)";
        RoomEditorPanel.UpdateRooms();
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
        
        var name = new Gtk.CellRendererText();
        var column = new Gtk.TreeViewColumn();
        column.Title = "Text";
        column.PackStart(name, false);
        column.SetCellDataFunc(name, RenderMyNodeText);

        NavigatorTreeView.AppendColumn(column);

        NavigatorTreeView.Model = AreasStore;

        var buffer = System.IO.File.ReadAllBytes("DroidSans-Bold.svg");
        var pixbuf = new Gdk.Pixbuf(buffer);
        
        MapImage.Pixbuf = pixbuf;
        MapImage.SetSizeRequest(MapImage.Pixbuf.Width, MapImage.Pixbuf.Height);
        
    }

    private async void RoomEditorPanel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == "EditingRoom.Name" && RoomEditorPanel.EditingRoom != null) {
            await CallBuildMap(RoomEditorPanel.EditingRoom.Area, RoomEditorPanel.EditingRoom);
        }
    }

    private async void NavigatorTreeView_SelectionChanged(object? sender, EventArgs e)
    {
        if(NavigatorTreeView.Selection.GetSelected(out var iter))
        {
            var data = NavigatorTreeView.Model.GetValue(iter, 0);
            var column = NavigatorTreeView.Columns.First();
            if(data is MyNode myNode) {
                DoEvents();
                var area = NavigatorTreeView.GetCellArea(myNode.Path, column);
                NavigatorScrollWindow.GetAllocatedSize(out var navsize, out var navbaseline);

                //NavigatorTreeView.ScrollToCell(myNode.Path, null, false, 0.5f, 0);
                NavigatorScrollWindow.Vadjustment.Value += area.Top - navsize.Height / 2;
                NavigatorScrollWindow.Hadjustment.Value = 0;
                if(myNode.Value is RoomData roomData) {
                    if(BuiltArea == null || BuiltArea.Name != roomData.Area.Name) {
                        await CallBuildMap(roomData.Area, roomData);
                    }
                    else if(AreaMap != null && AreaMap.Rooms.AsEnumerable().FirstOrDefault(r => r.Room.Id == roomData.Vnum, out var selectedRoom)) {
                        RedrawMap(selectedRoom?.Room);
                        
                    }
                    
                    RoomEditorPanel.EditingRoom = roomData;
                }
            }
        }
    }
    private void ProcessPendingEvents()
    {
        while (Gtk.Application.EventsPending())
            Gtk.Application.RunIteration(true);
    }

    private void DoEvents()
    {
        var rect = new Gdk.Rectangle(0, 0, MapImage.Pixbuf.Width, MapImage.Pixbuf.Height);
        MapImage.SizeAllocate(rect);
        MapImage.QueueResize();
        MapImageEventBox.SizeAllocate(rect);
        MapImageEventBox.QueueResize();
        ProcessPendingEvents();
        
        
        MapImage.QueueComputeExpand();
        MapImage.QueueDraw();
        MapImageEventBox.QueueComputeExpand();
        MapImageEventBox.QueueDraw();


        MapScrollWindow.CheckResize();
        NavigatorTreeView.QueueDraw();
        NavigatorScrollWindow.QueueDraw();
        ProcessPendingEvents();
    }

    

    private void MapScrollRoomIntoView(RoomData room) {
        if(AreaMap != null) {
            float xoffset = 0;
            float yoffset = 0;
            DoEvents();
            MapImage.GetAllocatedSize(out var alloc, out var baseline);
            if(alloc.Width > MapImage.Pixbuf.Width)
                xoffset = (alloc.Width - MapImage.Pixbuf.Width) / 2;
            if(alloc.Height > MapImage.Pixbuf.Height)
                yoffset = (alloc.Height - MapImage.Pixbuf.Height) / 2;

            MapScrollWindow.GetAllocatedSize(out var viewSize, out var viewBaseline);

            if(AreaMap.Rooms.FirstOrDefault(ir => ir.Room.Id == room.Vnum, out var imageroom) && imageroom != null) {
                
                MapScrollWindow.Vadjustment.Value = Math.Max(MapScrollWindow.Vadjustment.Lower, Math.Min(MapScrollWindow.Vadjustment.Upper, imageroom.Rectangle.Bottom - imageroom.Rectangle.Height / 2 + yoffset - viewSize.Height / 2));
                MapScrollWindow.Hadjustment.Value = Math.Max(MapScrollWindow.Hadjustment.Lower, Math.Min(MapScrollWindow.Hadjustment.Upper, imageroom.Rectangle.Right - imageroom.Rectangle.Width / 2 + xoffset - viewSize.Width / 2));
                Debug.Print($"MapScroll {imageroom.Rectangle.ToString()} :: {alloc} :: {MapScrollWindow.Hadjustment.Upper} x {MapScrollWindow.Vadjustment.Upper}");
            }
        }
    }

    private void RedrawMap(MMBRoom? newMarkedRoom = null)
    {
        var mmboptions = new MUDMapBuilder.BuildOptions();
        mmboptions.MaxSteps = 1000;

        if(MarkedRoom != null)
            MarkedRoom.MarkColor = OriginalColor;
        if(newMarkedRoom != null) {
            OriginalColor = newMarkedRoom.MarkColor;
            newMarkedRoom.MarkColor = SKColors.Red;
        }
        MarkedRoom = newMarkedRoom;

        if(BuiltArea != null)
        {
            if(MapImage.Pixbuf != null) {
                MapImage.Pixbuf.Dispose();
            }

            AreaMap = BuiltArea.BuildPng(mmboptions);
            MapImage.Pixbuf = new Pixbuf(AreaMap.PngData);

            if(MarkedRoom != null && RoomData.Rooms.TryGetValue(MarkedRoom.Id, out var roomData))
                MapScrollRoomIntoView(roomData);
        }
        
    }

    private void BuildMap(AreaData area, RoomData? selectedRoom = null) {
        //if(BuiltArea != null && area.Name == BuiltArea.Name) return;

        BuiltArea = null;
        AreaMap = null;
        if(MapImage.Pixbuf != null)
                MapImage.Pixbuf.Dispose();
        MapImage.Pixbuf = null;

        var mmboptions = new MUDMapBuilder.BuildOptions();
        mmboptions.MaxSteps = 1000;

        var mmbarea = new MUDMapBuilder.MMBArea() { Name = area.Name };

        var roomdict = new Dictionary<int, (RoomData, MMBRoom)>();
        var arearooms = new List<MMBRoom>();
        area.ResetArea();
        var POIs = "";
        foreach(var room in area.Rooms.Values) {
            POIs = "";

            foreach (var ch in room.Characters)
            {
                if(ch.Flags.ISSET(ActFlags.Train))
                    POIs = POIs + "   (TRAIN) " + ch.ShortDescription + "\n";
                if (ch.Flags.ISSET(ActFlags.Practice))
                    POIs = POIs + "   (PRACTICE) " + ch.ShortDescription + "\n";
                if (ch.Flags.ISSET(ActFlags.Shopkeeper))
                    POIs = POIs + "   (SHOP) " + ch.ShortDescription + "\n";
            }

            var mmbroom = new MUDMapBuilder.MMBRoom(room.Vnum, room.Name, false, POIs.TrimEnd());
            //arearooms.Add(mmbroom);    
            roomdict.TryAdd(room.Vnum, (room, mmbroom));
            foreach(var exit in room.Exits) {
                if(exit != null && exit.destination != null) {
                    POIs = "";
                    if (exit.destination.Area == area)
                    {
                        foreach (var ch in exit.destination.Characters)
                        {
                            if (ch.Flags.ISSET(ActFlags.Train))
                                POIs = POIs + "   (TRAIN) " + ch.ShortDescription + "\n";
                            if (ch.Flags.ISSET(ActFlags.Practice))
                                POIs = POIs + "   (PRACTICE) " + ch.ShortDescription + "\n";
                            if (ch.Flags.ISSET(ActFlags.Shopkeeper))
                                POIs = POIs + "   (SHOP) " + ch.ShortDescription + "\n";
                        }
                    }
                    var mmbdest = new MUDMapBuilder.MMBRoom(exit.destination.Vnum, exit.destination.Name, exit.destination.Area != room.Area, POIs.TrimEnd());
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

        if(selectedRoom != null && BuiltArea.Rooms.FirstOrDefault(r => r.Id == selectedRoom.Vnum, out var mmb_selected_room) && mmb_selected_room != null) {
            if(this.MarkedRoom != null)
                this.MarkedRoom.MarkColor = OriginalColor;
            OriginalColor = mmb_selected_room.MarkColor;
            this.MarkedRoom = mmb_selected_room;
            mmb_selected_room.MarkColor = SKColors.Red;
        }

        AreaMap = BuiltArea.BuildPng(mmboptions);
        MapImage.Pixbuf = new Pixbuf(AreaMap.PngData);
        try
        {
            System.IO.File.WriteAllBytes(area.Name + ".png", AreaMap.PngData.ToArray());
        }
        catch { }
        if (MarkedRoom != null && RoomData.Rooms.TryGetValue(MarkedRoom.Id, out var roomData))
            MapScrollRoomIntoView(roomData);
    }

    private async Task CallBuildMap(AreaData area, RoomData? selectRoom = null) {

        NavigatorTreeView.Sensitive = false;
        MapSpinner.Visible = true;
        MapSpinner.Start();
        await Task.Run(() => BuildMap(area, selectRoom));
        MapSpinner.Stop();
        MapSpinner.Visible = false;
        NavigatorTreeView.Sensitive = true;
    }

    private async void NavigatorTreeView_RowActivated(object o, RowActivatedArgs args)
    {
        var t = args.Path;
        if(NavigatorTreeView.Model.GetIter(out var iter, t))
        {
            if ( NavigatorTreeView.Model.GetValue( iter, 0 ) is MyNode node ) {
                var data = node.Value;

                if(data is AreaData area) {
                    
                    await CallBuildMap(area);
                }
            }
            
        }
    }

    private void MapImageEventBox_Click(object sender, ButtonReleaseEventArgs args)
    {
        if(AreaMap != null) {
            float xoffset = 0;
            float yoffset = 0;
            
            MapImage.GetAllocatedSize(out var alloc, out var baseline2);
            if(alloc.Width > MapImage.Pixbuf.Width)
                xoffset = (alloc.Width - MapImage.Pixbuf.Width) / 2;
            if(alloc.Height > MapImage.Pixbuf.Height)
                yoffset = (alloc.Height - MapImage.Pixbuf.Height) / 2;
            //var r = AreaMap.Rooms.FirstOrDefault(ar => ar.Rectangle.Contains(new System.Drawing.Point((int)(args.Event.X * (float) MapImage.Pixbuf.Width / alloc.Width) , (int)(args.Event.Y * (float) MapImage.Pixbuf.Height / alloc.Height))));
            var r = AreaMap.Rooms.FirstOrDefault(ar => ar.Rectangle.Contains(new System.Drawing.Point((int)(args.Event.X - xoffset) , (int)(args.Event.Y - yoffset))));
            if(r != null && RoomNodes.TryGetValue(r.Room.Id, out var room))
            {
                NavigatorTreeView.ExpandToPath(room.Path);
                NavigatorTreeView.Selection.SelectPath(room.Path);

                // room selection change on navigator tree view drives redraw map

            }
        }
    }
    private void RenderMyNodeText(TreeViewColumn tree_column, CellRenderer cell, ITreeModel tree_model, TreeIter iter)
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

    private void SaveAreasButton_Clicked(object sender, EventArgs e)
    {
        var areastosave = (from area in AreaData.Areas where area.saved == false select area).ToList();
        areastosave.ForEach(area => area.Save());
        using(var dialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, $"{areastosave.Count} areas saved."))
        {
            dialog.Title = "Saved Areas";
            dialog.Run();
        }
    }

    protected override bool OnDeleteEvent(Event e) {
        Application.Quit();
        return true;
    }
}
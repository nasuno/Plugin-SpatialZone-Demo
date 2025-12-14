&nbsp;&nbsp;SpatialZone Plugin API Cheatsheet

&nbsp;&nbsp;Margins

**Create individual margins**<br>
```vb
api.CreateMargin(marginId As String, marginType As MarginType, panel As PanelType, 
                                              row As Integer?, column As Integer?, locked As Boolean)
```
&nbsp;&nbsp;&nbsp;&nbsp;`RowMargin`: supply `row`, set `column` to `Nothing`<br>
&nbsp;&nbsp;&nbsp;&nbsp;`ColumnMargin`: supply `column`, set `row` to `Nothing`

**Example**<br>
```vb
api.CreateMargin("Compact_TopRow", MarginType.RowMargin, PanelType. TopPanel, 10, Nothing, False)
api.CreateMargin("Compact_LeftColumn", MarginType.ColumnMargin, PanelType. TopPanel, Nothing, 5, False)
```

**Bundle margins into a named set**<br>
```vb
api.CreateMarginSet(setName As String, topRowMarginId As String, bottomRowMarginId As String,
                                   leftColumnMarginId As String, rightColumnMarginId As String)
```

**Example**<br>
```vb
api.CreateMarginSet("CompactLayout", "Compact_TopRow", "Compact_BottomRow", 
                                 "Compact_LeftColumn", "Compact_RightColumn")
```

**List all margin set names**<br>
```vb
Dim names As List(Of String) = api.GetAllMarginSetNames()
Console.WriteLine("Available margin sets: " & String. Join(", ", names))
```

**Retrieve a margin set's contents**<br>
```vb
Dim setDict As Dictionary(Of String, String) = api.GetMarginSet("MyLayout")
' Keys: "TopRow", "BottomRow", "LeftColumn", "RightColumn"
```

**Get all margin IDs by type**<br>
```vb
Dim allMargins = api.GetAllMarginIDs()
Dim rowMargins As List(Of String) = allMargins("row")
Dim columnMargins As List(Of String) = allMargins("column")
```

**Get margin properties**<br>
```vb
Dim info As Dictionary(Of String, Object) = api.GetMarginInfoSnapshot("MyMarginId")
' Keys: "ID", "Type", "Panel", "Locked", "StructureID", "Row", "Column"
```

**Remove / lock margins**<br>
```vb
api.RemoveMargin("MyMarginId")
api.SetMarginLock("MyMarginId", True)   ' lock
api.SetMarginLock("MyMarginId", False)  ' unlock
```

**Move margins**<br>
```vb
api.MarginPlusOne("MyMarginId")   ' increment by one (wraps across panels)
api.MarginJump("MyMarginId", PanelType.TopPanel, row: =5, column:=Nothing)
```

---

## Spatial Zones

**Create / retrieve**<br>
```vb
Dim zone As ISpatialZone = api.CreateSpatialZone(zoneId As String)
Dim zone As ISpatialZone = api.GetSpatialZone(zoneId As String)
```

**List all zones**<br>
```vb
For Each zone As ISpatialZone In api. GetAllSpatialZones()
    Console.WriteLine(zone.ID)
Next
```

**Remove** WIP (see zone pool)<br>
```vb
api.RemoveSpatialZone(zoneId As String)
```

---

&nbsp;&nbsp;ISpatialZone Members

  Property          |  Type                            | Description 
--------------------|----------------------------------|-------------
 `ID`               | `String`                         | Zone identifier
 `Left`             | `Integer`                        | Left column boundary
 `Right`            | `Integer`                        | Right column boundary
 `Top`              | `Integer`                        | Top row boundary
 `Bottom`           | `Integer`                        | Bottom row boundary
 `Text`             | `String`                         | Read/write – displayed text (triggers redraw on change)
 `BoundingBoxAABB`  | `((Int,Int,Int),(Int,Int,Int))`  | 3D bounding box (min, max corners)
 `WrappedCharIndex` | `Dictionary(Of (Int,Int), Char)` | Maps `(row, col)` to rendered character

  Method                                           | Description
---------------------------------------------------|-------------
 `UpdateMargins(leftId, rightId, topId, bottomId)` | Reposition zone using margin IDs
 `GetAllFontSegments() As List(Of (Int, Int))`     | Returns all `(row, col)` font-cell positions
 `SetGutterVisible(row, col, side, visible)`       | Show/hide gutter; side = `"above"`, `"below"`, `"left"`, `"right"`
 `DisposeZone()`                                   | **WIP (see zone pools)

**Usage**<br>
```vb
Dim zone As ISpatialZone = api. CreateSpatialZone("MyZone")
zone.Text = "Hello World"

Dim aabb = zone.BoundingBoxAABB
Dim minCorner = aabb.Item1  ' (minX, minY, minZ)
Dim maxCorner = aabb.Item2  ' (maxX, maxY, maxZ)

For Each kvp In zone.WrappedCharIndex
    Dim row As Integer = kvp.Key. Item1
    Dim col As Integer = kvp.Key.Item2
    Dim ch As Char = kvp.Value
Next

zone.SetGutterVisible(0, 1, "above", True)
```

---

&nbsp;&nbsp;Margin Set Slots (Primary / Secondary)

Each zone has two slots for margin sets.  The **A/B in method names refers to the slot**, not your margin set name.  Your margin sets can have ANY name that makes sense for your application. 

  Method                                 | Purpose
-----------------------------------------|---------
 `AssignZoneMarginSetA(zoneId, setName)` | Store any set in **Primary Slot**
 `AssignZoneMarginSetB(zoneId, setName)` | Store any set in **Secondary Slot**
 `SwitchZoneToMarginSetA(zoneId)`        | Activate Primary Slot
 `SwitchZoneToMarginSetB(zoneId)`        | Activate Secondary Slot
 `SwapZoneMarginSets(zoneId)`            | Toggle active slot
 `GetZoneAssignedMarginSet(zoneId)`      | Returns currently active set name

**Usage**<br>
```vb
' Assign margin sets to slots
api.AssignZoneMarginSetA("zone1", "CompactLayout")
api.AssignZoneMarginSetB("zone1", "SpacedLayout")

' Activate Primary Slot
api.SwitchZoneToMarginSetA("zone1")
Console.WriteLine("Currently assigned:  " & api.GetZoneAssignedMarginSet("zone1"))

' Swap to Secondary Slot
api.SwapZoneMarginSets("zone1")
Console.WriteLine("Currently assigned: " & api.GetZoneAssignedMarginSet("zone1"))

' Swap back to Primary Slot
api. SwapZoneMarginSets("zone1")
```

**Wrapper naming suggestions**

 Wrapper Name            | Underlying API         | Good for...
-------------------------|------------------------|-------------
 SetPrimaryLayout        | AssignZoneMarginSetA   | Storing main/default layout
 SetSecondaryLayout      | AssignZoneMarginSetB   | Storing alternate layout
 ActivatePrimaryLayout   | SwitchZoneToMarginSetA | "go to primary layout"
 ActivateSecondaryLayout | SwitchZoneToMarginSetB | "go to alternate layout"
 ToggleActiveLayout      | SwapZoneMarginSets     | "flip between two layouts"

Swapping only toggles which SLOT is currently active; it does not change what margin set names are stored in those slots.

---

&nbsp;&nbsp;Swapping Layouts Between Multiple Zones

When multiple zones are configured with the same pair of margin set names in opposite slots, calling `SwapZoneMarginSets` on each zone makes them effectively "swap appearances."

```
Zone 1:
    Primary Slot (A)   = "CompactLayout"
    Secondary Slot (B) = "SpacedLayout"
    (currently active:  Primary -> displays "CompactLayout")

Zone 2:
    Primary Slot (A)   = "SpacedLayout"
    Secondary Slot (B) = "CompactLayout"
    (currently active: Primary -> displays "SpacedLayout")
```

```vb
api.SwapZoneMarginSets("zone1")
api.SwapZoneMarginSets("zone2")
```

Result:<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Zone 1: now active = "SpacedLayout" (via Secondary Slot)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Zone 2: now active = "CompactLayout" (via Secondary Slot)

Visually, the contents appear to have swapped zones. This works because the same set names are shared across zones, and each zone's slots are configured in opposite roles.

---

&nbsp;&nbsp;Zone Pooling

Pre-create zones and store references for later use. 

**Create a pool**<br>
```vb
Private Const ZonePoolSize As Integer = 5
Private zonePool As New Dictionary(Of String, ISpatialZone)()

Private Sub CreateZonePool(api As ICurrentApi)
    For i As Integer = 1 To ZonePoolSize
        Dim pluginId = "plugin-zone-" & i
        api.CreateSpatialZone(pluginId)
        Dim zone = api.GetSpatialZone(pluginId)
        If zone IsNot Nothing Then
            zonePool.Add(pluginId, zone)
        End If
    Next
End Sub
```

**Draw from the pool**<br>
```vb
Private Function GetNextZone() As ISpatialZone
    If zonePool.Count = 0 Then Return Nothing
    Dim first = zonePool.First()
    zonePool.Remove(first. Key)
    Return first.Value
End Function
```

**Use pooled zones**<br>
```vb
Dim zone1 = GetNextZone()
Dim zone2 = GetNextZone()

If zone1 IsNot Nothing AndAlso zone2 IsNot Nothing Then
    zone1.Text = "Zone 1 content"
    zone2.Text = "Zone 2 content"
Else
    Console.WriteLine("Not enough spatial zones in the pool.")
End If
```

---

&nbsp;&nbsp;Zone References & Disposal WIP (see zone pool)

**Removing a zone**<br>
```vb
api.RemoveSpatialZone("zone1")   ' Removes from host tracking
```

**Post-removal behavior**

After calling `api.RemoveSpatialZone(zoneId)`:<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The zone is removed from the host's management dictionary<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Cleanup logic executes (removing 3D objects from the global dictionary) - **WIP**<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Plugin references to the zone object remain valid .NET objects until garbage collected

**"Zombie" zone behavior**

These removed-but-referenced zones can still be used:<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Methods like `UpdateMargins`, `Text` assignment may still create or modify shared host state<br><br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The host and other plugins will no longer be aware of or manage the zone<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Any further actions are entirely the responsibility of your plugin<br>

**Risks**<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Resource leaks (untracked objects, orphaned state)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Inconsistent or surprising behavior if zones are manipulated after disposal<br>

This design intentionally does not prevent post-disposal usage to encourage creativity and experimentation.  Use this power with care.

---

&nbsp;&nbsp;Enums

```vb
Public Enum MarginType
    RowMargin
    ColumnMargin
End Enum

Public Enum PanelType
    BottomPanel
    NorthPanel
    EastPanel
    SouthPanel
    WestPanel
    TopPanel
End Enum
```

---

&nbsp;&nbsp;Panel Bounds

```vb
Dim leftCol   As Integer = api.GetPanelFurthestLeftColumn(PanelType.TopPanel)
Dim topRow    As Integer = api.GetPanelFurthestTopRow(PanelType.TopPanel)
Dim rightCol  As Integer = api.GetPanelFurthestRightColumn(PanelType.TopPanel)
Dim bottomRow As Integer = api.GetPanelFurthestBottomRow(PanelType.TopPanel)
```

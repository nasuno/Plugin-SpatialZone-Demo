Imports Current.PluginApi

<PluginMetadata("SpatialZone Plugin Demo", "1.0", "Nasuno", "Usage examples.")>
Public Class PluginSpatialZoneDemo
    Implements IPlugin

    Private Const ZonePoolSize As Integer = 5
    Private zonePool As New Dictionary(Of String, ISpatialZone)()

    Public Sub Execute(api As ICurrentApi) Implements IPlugin.Execute

        '===================
        ' DEMO:  Margin sets + spatial zone slot (Primary/Secondary) usage
        '===================

        ' Create margins for the first set - using a descriptive name for its purpose. 
        ' NOTE: The margin set name can be anything meaningful to your application.
        ' Here we use "CompactLayout" as an example - you might use "MyFirstLayout",
        ' "LayoutOne", "CustomMargins", etc.
        api.CreateMargin("Compact_TopRow", MarginType.RowMargin, PanelType.TopPanel, 10, Nothing, False)
        api.CreateMargin("Compact_BottomRow", MarginType.RowMargin, PanelType.TopPanel, 20, Nothing, False)
        api.CreateMargin("Compact_LeftColumn", MarginType.ColumnMargin, PanelType.TopPanel, Nothing, 5, False)
        api.CreateMargin("Compact_RightColumn", MarginType.ColumnMargin, PanelType.TopPanel, Nothing, 15, False)
        api.CreateMarginSet("CompactLayout", "Compact_TopRow", "Compact_BottomRow", "Compact_LeftColumn", "Compact_RightColumn")

        ' Create margins for the second set - again, using a descriptive name.
        ' This could be "SpacedLayout", "LayoutTwo", "AnotherLayout", etc.
        api.CreateMargin("Spaced_TopRow", MarginType.RowMargin, PanelType.TopPanel, 30, Nothing, False)
        api.CreateMargin("Spaced_BottomRow", MarginType.RowMargin, PanelType.TopPanel, 40, Nothing, False)
        api.CreateMargin("Spaced_LeftColumn", MarginType.ColumnMargin, PanelType.TopPanel, Nothing, 25, False)
        api.CreateMargin("Spaced_RightColumn", MarginType.ColumnMargin, PanelType.TopPanel, Nothing, 35, False)
        api.CreateMarginSet("SpacedLayout", "Spaced_TopRow", "Spaced_BottomRow", "Spaced_LeftColumn", "Spaced_RightColumn")

        ' List available sets for user visibility
        Dim marginSets = api.GetAllMarginSetNames()
        Console.WriteLine("Available margin sets:  " & String.Join(", ", marginSets))

        ' Create a spatial zone named "zone1"
        api.CreateSpatialZone("zone1")

        ' IMPORTANT: AssignZoneMarginSetA places your margin set into the zone's "Primary Slot."
        ' The "A" in the method name refers to the SLOT, not the name of your margin set.
        ' You can assign ANY margin set name to either slot. 
        ' Here we assign "CompactLayout" to the Primary Slot (A) and make it active. 
        api.AssignZoneMarginSetA("zone1", "CompactLayout")
        api.SwitchZoneToMarginSetA("zone1")
        Console.WriteLine("Assigned 'CompactLayout' to Primary Slot (A) for 'zone1' and activated it.")
        Console.WriteLine("Currently assigned set for zone1: " & api.GetZoneAssignedMarginSet("zone1"))

        ' AssignZoneMarginSetB places your margin set into the zone's "Secondary Slot."
        ' Again, the "B" refers to the SLOT - you can put any margin set name here.
        ' This does not change the currently displayed margins until we switch or swap.
        api.AssignZoneMarginSetB("zone1", "SpacedLayout")
        Console.WriteLine("Assigned 'SpacedLayout' to Secondary Slot (B) for 'zone1' (not yet active).")

        ' Swap between Primary Slot (A) and Secondary Slot (B).
        ' The zone now displays "SpacedLayout", and GetZoneAssignedMarginSet returns "SpacedLayout".
        api.SwapZoneMarginSets("zone1")
        Console.WriteLine("Swapped zone1 between slots (now using SpacedLayout from Secondary Slot).")
        Console.WriteLine("Currently assigned set for zone1: " & api.GetZoneAssignedMarginSet("zone1"))

        ' Swap again to return to Primary Slot.  The zone now displays "CompactLayout".
        api.SwapZoneMarginSets("zone1")
        Console.WriteLine("Swapped zone1 again (now using CompactLayout from Primary Slot).")
        Console.WriteLine("Currently assigned set for zone1: " & api.GetZoneAssignedMarginSet("zone1"))

        '------------------------------------------------------------------------------
        ' Understanding Margin Set Slots:  Primary (A) and Secondary (B)
        '
        ' Each SpatialZone maintains two logical margin-set SLOTS:
        '   - Primary Slot (accessed via methods ending in "... SetA")
        '   - Secondary Slot (accessed via methods ending in "...SetB")
        '
        ' CRITICAL: The "A" and "B" in the API method names refer to SLOTS, not to
        ' what you should name your margin sets! Your margin sets can have ANY name
        ' that makes sense for your application (e.g., "CompactLayout", "MyMargins",
        ' "LayoutOne", "CustomSet", etc.).
        '
        ' The host API exposes: 
        '   - AssignZoneMarginSetA(zoneId, setName)  ' Assign any set to Primary Slot
        '   - AssignZoneMarginSetB(zoneId, setName)  ' Assign any set to Secondary Slot
        '   - SwitchZoneToMarginSetA(zoneId)         ' Activate Primary Slot
        '   - SwitchZoneToMarginSetB(zoneId)         ' Activate Secondary Slot
        '   - SwapZoneMarginSets(zoneId)             ' Toggle between slots
        '
        ' Think of it like two drawers (Primary and Secondary) that can each hold
        ' one margin set. You decide what goes in each drawer, and you can switch
        ' which drawer is "open" (active) at any time.
        '
        ' Suggested naming patterns if you wrap these in your own plugin helpers: 
        '
        ' | Wrapper Name                  | Underlying API              | Good for...                  |
        ' |-------------------------------|-----------------------------|------------------------------|
        ' | SetPrimaryLayout              | AssignZoneMarginSetA        | Storing main/default layout  |
        ' | SetSecondaryLayout            | AssignZoneMarginSetB        | Storing alternate layout     |
        ' | ActivatePrimaryLayout         | SwitchZoneToMarginSetA      | "go to primary layout"       |
        ' | ActivateSecondaryLayout       | SwitchZoneToMarginSetB      | "go to alternate layout"     |
        ' | ToggleActiveLayout            | SwapZoneMarginSets          | "flip between two layouts"   |
        '
        ' Remember: Swapping only toggles which SLOT is currently active; it does not
        ' change what margin set names are stored in those slots. 
        '
        '------------------------------------------------------------------------------
        ' Usage Note – Swapping Layouts Between Multiple Zones (Primary/Secondary Model)
        '
        ' When multiple spatial zones are configured with the same pair of margin set
        ' names in opposite slots, you can make zones effectively "swap appearances"
        ' by calling SwapZoneMarginSets on each zone.
        '
        ' Example Scenario:
        '
        '   Zone 1:
        '       Primary Slot (A)   = "CompactLayout"
        '       Secondary Slot (B) = "SpacedLayout"
        '       (currently active: Primary -> displays "CompactLayout")
        '
        '   Zone 2:
        '       Primary Slot (A)   = "SpacedLayout"
        '       Secondary Slot (B) = "CompactLayout"
        '       (currently active: Primary -> displays "SpacedLayout")
        '
        '   ' Both zones share the same margin sets, but each zone is showing
        '   ' a different one because their slot assignments are reversed.
        '
        '   api.SwapZoneMarginSets("zone1")
        '   api.SwapZoneMarginSets("zone2")
        '
        ' Result:
        '   Zone 1: now active = "SpacedLayout" (via its Secondary Slot)
        '   Zone 2: now active = "CompactLayout" (via its Secondary Slot)
        '
        ' Visually, the contents of "CompactLayout" and "SpacedLayout" appear to have
        ' swapped zones.  This works because the same set names are shared across zones,
        ' and each zone's slots are configured in opposite roles.
        '
        '------------------------------------------------------------------------------

        '===================
        ' DEMO: Zone pool usage
        '===================

        CreateZonePool(api)
        UseTwoZonesInWork()
    End Sub

    ' Fill the pool with ISpatialZone objects
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

    ' Method focused only on using two zones
    Private Sub UseTwoZonesInWork()
        Dim zone2 = GetNextZone()
        Dim zone3 = GetNextZone()

        If zone2 IsNot Nothing AndAlso zone3 IsNot Nothing Then
            Dim infoText1 As String =
                "Plugin zone 1 real ID: " & zone2.ID
            Dim infoText2 As String =
                "Plugin zone 2 real ID: " & zone3.ID

            zone2.Text = infoText1
            zone3.Text = infoText2

            Console.WriteLine(infoText1)
            Console.WriteLine(infoText2)
        Else
            Console.WriteLine("Not enough spatial zones in the pool.")
        End If
    End Sub

    ' Removes and returns the next zone from the pool
    Private Function GetNextZone() As ISpatialZone
        If zonePool.Count = 0 Then Return Nothing
        Dim first = zonePool.First()
        zonePool.Remove(first.Key)
        Return first.Value
    End Function
End Class

' You can also:
'   Dim zone1 = api.CreateSpatialZone("zone1")
'   ' zone1 is an ISpatialZone reference (adapter), not a copy.
'   ' You can hold it in a local variable, remove the zone from the host,
'   ' and still invoke methods on the adapter – see notes below.
'
' Example: Remove a spatial zone when done
'   api.RemoveSpatialZone("zone1") ' This removes and disposes the zone from the host.
'
' ==============================
' INFORMATION FOR PLUGIN DEVELOPERS:
'
' After calling api.RemoveSpatialZone(zoneId) or zone.DisposeZone(), the spatial zone
' is removed from the host's management/tracking dictionary and its cleanup logic
' (such as removing 3D objects from the global object dictionary) is executed. WIP
'
' HOWEVER:
'   - Any plugin references (variables) to the zone object (such as those returned from
'     GetSpatialZone or CreateSpatialZone) will still exist and remain valid .NET
'     objects until they go out of scope and are garbage collected.
'
'   - These "zombie" zone objects can still be used: methods like UpdateMargins, Text
'     assignment, or other operations may still create or modify shared host state
'     (for example, recreating 3D objects in the global object dictionary), even
'     though the host no longer tracks the zone as a managed entity.
'
'   - The host and other plugins will no longer be aware of or manage the zone after
'     disposal, so any further actions performed through these references are entirely
'     the responsibility of your plugin.
'
' This behavior allows for advanced or experimental uses—such as resurrecting zones,
' debugging, or custom visualization flows—but it comes with risks:
'   - Resource leaks (untracked objects, orphaned state)
'   - Inconsistent or surprising behavior if zones are manipulated after disposal
'
' Best Practice:
'   - Use zone references only while the zone is managed by the host.
'   - Avoid using zone references after disposal, unless you have a specific,
'     well-understood purpose and you are prepared to manage the consequences.
'
' This design intentionally does not prevent post-disposal usage, in order to
' encourage creativity and experimentation in plugins. Use this power with care.

' ==============================

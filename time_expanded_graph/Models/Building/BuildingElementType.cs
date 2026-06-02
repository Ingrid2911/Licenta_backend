namespace time_expanded_graph.Models.Building
{
    public enum BuildingElementType
    {
        Room,           // Camera (dreptunghi cu pereti)
        Stairs,         // Scari (bloc cu trepte)
        Elevator,       // Lift (dreptunghi cu X)
        ExitDoor,       // Usa de evacuare (simbol EXIT pe perete)
        StartPoint,     // Punct de start / sursa persoanelor
        Hallway         // Rezervat pentru compatibilitate
    }
}
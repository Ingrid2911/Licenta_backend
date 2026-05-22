namespace time_expanded_graph.Models.Building
{
    public enum BuildingElementType
    {
        Room,           // Camera (dreptunghi cu pereti)
        Door,           // Usa interna (arc 90 grade)
        Stairs,         // Scari (bloc cu trepte)
        Elevator,       // Lift (dreptunghi cu X)
        ExitDoor,       // Usa de evacuare (simbol EXIT pe perete)
        StartPoint,     // Punct de start / sursa persoanelor
        Hallway         // Rezervat pentru compatibilitate
    }
}
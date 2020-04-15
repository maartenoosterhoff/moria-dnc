namespace Moria.Core.Structures.Enumerations
{
    // Type 2 unusual rooms all have an inner room:
    //   1 - Just an inner room with one door
    //   2 - An inner room within an inner room
    //   3 - An inner room with pillar(s)
    //   4 - Inner room has a maze
    //   5 - A set of four inner rooms
    public enum InnerRoomTypes
    {
        Plain = 1,
        TreasureVault,
        Pillars,
        Maze,
        FourSmallRooms,
    };
}

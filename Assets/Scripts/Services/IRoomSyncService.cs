namespace Services
{
    public interface IRoomSyncService
    {
        void GenerateTestRoom();
        void SynchronizeRooms(RoomData arRoom);
    }
}

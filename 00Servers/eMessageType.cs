namespace AGSyncCS
{
    enum eMessageType
    {
        Push = 10,
        Response = 20
    }

    public enum eRoomState {
        Idle,
        Waiting,
        Playing
    }

    public enum eItemState {
        Idle,
        InUse
    }
}
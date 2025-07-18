namespace AGSyncCS
{
    enum eMessageType
    {
        Push = 0,
        Response = 1
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
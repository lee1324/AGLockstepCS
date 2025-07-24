using System.Runtime.CompilerServices;

public class ErrorCode
{
    public const int None = 0;
    public const int VerNotMatch = 1;
    public const int Unknown_Protocal = 02;//as 404, or Malicious

    public const int PortTaken = 10;//�˿ڱ�ռ�û�����ԭ����TCP����������ʧ��
    public const int UDPServerStart_Failed = 11;//ͬ��

    /// <summary>
    /// ����ԭ�� ����ͬһ���������������δ����
    /// </summary>
    public const int ConnectServer_Failed = 12;
    public const int InvalidPosIndex = 13;

    /// <summary>
    /// ��Ҫ�Ŷ�
    /// </summary>
    public const int NoRoomsAvailable = 30;
    public const int RoomNotFound = 31;
    public const int RoomFull = 32;
    public const int PosTaken = 33; //position already taken by another user
    public const int InvalidPosition = 34;//invalid position, e.g. 0 or > Config.MaxPlayersPerRoom
    public const int PositionOccupied = 35; //position occupied by other user

    public static int PositionNotFound = 36; //position not found in the room
}
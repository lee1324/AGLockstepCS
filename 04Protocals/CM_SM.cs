using System.IO;
namespace AGSyncCS
{
    //客户端请求
    public abstract partial class CM
    {
        public abstract void readFrom(BinaryReader reader);
        public abstract void writeTo(BinaryWriter writer);
    }
    //server 返回
    public abstract class SM
    {
        public abstract void readFrom(BinaryReader reader);
        public abstract void writeTo(BinaryWriter writer);
    }

}
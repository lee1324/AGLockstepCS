using System.IO;
namespace AGSyncCS
{
    //客户端请求
    public partial abstract class CM
    {
        public abstract void readFrom(BinaryReader reader);
        public abstract void writeTo(BinaryWriter writer);

        //only log for debug
        public virtual string toString() { return ""; }
    }
    //server 返回
    public abstract class SM
    {
        public abstract void readFrom(BinaryReader reader);
        public abstract void writeTo(BinaryWriter writer);

        //only log for debug
        public virtual string toString() { return "";  }
    }
    

}
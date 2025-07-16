namespace AGSyncCS {
    //客户端请求
    public class CM{
        public virtual void readFrom(Binaryreader reader){}
        public virtual void writeTo(BinaryWriter writer){}
    }
    //server 返回
    public class SM{
        public int errorCode;
        public virtual void readFrom(Binaryreader reader){}
        public virtual void writeTo(BinaryWriter writer){}
    }
}
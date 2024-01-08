using System;
using System.Runtime.InteropServices;
using System.Text;




namespace CrfSpace
{
    static class Program
    {
        const int Version_2 = 0;
        const int Version_3 = 1;
        static UInt32 DTU_handle=0;
        public static void Main()
        {

            //Create file handle 
            const byte TYPE_CRF = 0;//crf文件类型

            UInt32 DTU_handle = CANDTU_DLL.DTU_Create(TYPE_CRF);
            //Get CRF file path
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "test2.CRF";

            Console.WriteLine("{0}", filePath);

            //load CRF file
            CANDTU_DLL._FileCreatedInfo fileCreatedInfo = new CANDTU_DLL._FileCreatedInfo();
            Console.Write(Marshal.SizeOf(fileCreatedInfo));
            byte[] temp = Encoding.GetEncoding("gb2312").GetBytes(filePath);//trun to gb2312 encoding.
            IntPtr ptr = Marshal.AllocHGlobal(temp.Length);//trun to gb2312 encoding. 
            Marshal.Copy(temp, 0, ptr, temp.Length);//byte[]转成intptr
            fileCreatedInfo.strFilePath = ptr;
            byte MODE_READ = 1;//only read mode
            fileCreatedInfo.dwDesiredAccess = MODE_READ;
            IntPtr P2_FileCreatedInfo = Marshal.AllocHGlobal(Marshal.SizeOf(fileCreatedInfo));
            Marshal.StructureToPtr(fileCreatedInfo, P2_FileCreatedInfo, true);
            if (CANDTU_DLL.DTU_Open(DTU_handle, P2_FileCreatedInfo))
            {
                Console.WriteLine("Load crf file succeed!");
            }
            else
            {
                Console.WriteLine("Load failed!");

            }

            //Get file size
            UInt32 fileSize = CANDTU_DLL.DTU_GetFileSize(DTU_handle);
            Console.Write("fileSize={0}\n", fileSize);

            //Get file type
            uint fileType = CANDTU_DLL.DTU_GetDataType(DTU_handle);
            if (fileType == 0)
            {
                Version2_Operation();
            }
            else if(fileType==1) 
            {
                Version3_Operation();
            }





        }

        public static void Version2_Operation()
        {
            //Get file header information
            CANDTU_DLL.Candtu_File_Info fileHeader = new CANDTU_DLL.Candtu_File_Info();
            IntPtr P2fileHeader = Marshal.AllocHGlobal(Marshal.SizeOf(fileHeader));
            if (CANDTU_DLL.DTU_GetVer2HeaderInfo(DTU_handle, P2fileHeader))
            {
                Console.WriteLine("Get header information succeed!");
            }
            else
            {
                Console.WriteLine("Get header information failed!");
            }
            Marshal.PtrToStructure(P2fileHeader, fileHeader);
            UInt32 oneFrameSize = 0;
            UInt16 fileLength   = fileHeader.len; //文件大小= len-24Bytes(file header)-段数量*段头大小(8 Bytes ）
            UInt64 fileBaseTime = fileHeader.time*1000;//candtu的时候*1000   //百微妙 100 *0.001s=0.1s

            //Read Crf file as frame.
            uint nNumberOfBytesToRead = 50*1024*1024 ; //预留的数据量 50mb
            byte[] bytes = new byte[nNumberOfBytesToRead];//没用的byte数组
            IntPtr P2dataBuffer = Marshal.AllocHGlobal(bytes.Length);//预留的内存 分配50MB内存 Allocate 50MB memories for receive crf frame data.  
            Marshal.Copy(bytes,0,P2dataBuffer,Marshal.SizeOf(P2dataBuffer));
            

            UInt32 nNumberOfByteRead = 0;//真正读到的数据长度;
            UInt32 validDataSize = 0;//真正读到的数据长度（减去头部后）;
            UInt32 isCompress = 0; //是否压缩版本
            if (CANDTU_DLL.DTU_ReadAsFrm(DTU_handle, P2dataBuffer, nNumberOfBytesToRead, out nNumberOfByteRead, out validDataSize, out isCompress))
            {
                Console.WriteLine("ReadAsFrame Succeed!");

                Console.Write("nNumberOfByteRead={0},validDataSize={1},isCompress={2}", nNumberOfByteRead, validDataSize, isCompress);
            }
            else
            {
                Console.WriteLine("ReadAsFrame Failed!");
            }
            
            //print the all data
            CANDTU_DLL.aw_can_std_msg awCanStdMsg = new CANDTU_DLL.aw_can_std_msg();
            oneFrameSize = (UInt32)Marshal.SizeOf(awCanStdMsg);//FrameSize is depend on the crf protocol. 

            UInt32 frameNum = validDataSize / oneFrameSize;
            CANDTU_DLL.aw_can_std_msg[] awCanStdMsgArray = new CANDTU_DLL.aw_can_std_msg[frameNum];
            //把数组中每个元素的地址对应转成结构体
            for (int i = 0; i < frameNum; i++)
            {
                try
                {
                    awCanStdMsgArray[i] = new CANDTU_DLL.aw_can_std_msg(); // 创建结构体实例
                    Marshal.PtrToStructure((IntPtr)(P2dataBuffer.ToInt64() + (i * Marshal.SizeOf(awCanStdMsgArray[i]))), awCanStdMsgArray[i]);
                }
                catch (Exception ex)
                {
                    Console.Write("error\n");
                    Console.Write("P2dataBuffer.ToInt64()={0}\n", P2dataBuffer.ToInt64());
                    //Console.WriteLine("(P2dataBuffer.ToInt64() + (i * Marshal.SizeOf(awCanStdMsgArray[i])={0}\n",(P2dataBuffer.ToInt64() + (i * Marshal.SizeOf(awCanStdMsgArray[i]));
                    Console.WriteLine("awCanStdMsgArray[i]={0}\n",awCanStdMsgArray[i]);
                    Console.Write("{0}....",ex.Message);
                    
                    throw;
                }
			    
            }
            
           

            
            
            
            if (validDataSize % oneFrameSize != 0) 
            {
                Console.Write("文件报文总长度无法整除单帧长度");//read valid length %d not enough for multi frames! 原本的注释为读取到的真实值不足以完成多帧传输。
            }

            if (validDataSize > nNumberOfBytesToRead)
            {
                Console.Write("预留的取值空间过小了");
            }
            
            Console.Write("frameNum={0}\n", frameNum);
            
            //clear unmanaged buffer 
            Marshal.FreeHGlobal(P2fileHeader);
            Marshal.FreeHGlobal(P2dataBuffer);

            for (UInt32 i = 0; i < frameNum; i++) 
            {

                Console.WriteLine("timestamp={0},channal={1},id={2},length={3},data={4}", 
                                    awCanStdMsgArray[i].can_msg._timestamp,
                                    awCanStdMsgArray[i].can_msg._channel,
                                    awCanStdMsgArray[i].can_msg._id,
                                    awCanStdMsgArray[i].can_msg._length,
                                    BitConverter.ToString(awCanStdMsgArray[i].MasgBuff).Replace("-"," "));
            }   
        }
        public static void Version3_Operation()
        {
            //Get file header information
            CANDTU_DLL._file_head fileHeader = new CANDTU_DLL._file_head();
            IntPtr P2fileHeader = Marshal.AllocHGlobal(Marshal.SizeOf(fileHeader));
            if (CANDTU_DLL.DTU_GetVer3HeaderInfo(DTU_handle, P2fileHeader))
            {
                Console.WriteLine("Get header information succeed!");
            }
            else
            {
                Console.WriteLine("Get header information failed!");
            }
            Marshal.PtrToStructure(P2fileHeader, fileHeader);
            UInt32 oneFrameSize = 0;
            UInt16 fileLength = (UInt16)fileHeader.len; //文件大小= len-24Bytes(file header)-段数量*段头大小(8 Bytes ）
            UInt64 fileBaseTime = fileHeader.tm_start * 1000*1000;//candtu的时候单位为百微秒*1000   canfddtu时候单位为微秒，所以*1000*1000

            //Read Crf file as frame.
            uint nNumberOfBytesToRead = 50 * 1024 * 1024*10; //预留的数据量 50mb
            byte[] bytes = new byte[nNumberOfBytesToRead];//
            IntPtr P2dataBuffer = Marshal.AllocHGlobal(bytes.Length);//
            Marshal.Copy(bytes, 0, P2dataBuffer, Marshal.SizeOf(P2dataBuffer));//把byte数组复制到内存里
           

            UInt32 nNumberOfByteRead = 0;//真正读到的数据长度;
            UInt32 validDataSize = 0;//真正读到的数据长度（减去头部后）;
            UInt32 isCompress = 0;//是否压缩版本
            if (CANDTU_DLL.DTU_ReadAsFrm(DTU_handle, P2dataBuffer, nNumberOfBytesToRead, out nNumberOfByteRead, out validDataSize, out isCompress))
            {
                Console.WriteLine("ReadAsFrame Succeed!");

                Console.Write("nNumberOfByteRead={0},validDataSize={1},isCompress={2}", nNumberOfByteRead, validDataSize, isCompress);
            }
            else
            {
                Console.WriteLine("ReadAsFrame Failed!");
            }

            //print the all data
            CANDTU_DLL.Frame CanfdMsg = new CANDTU_DLL.Frame();
            oneFrameSize = (UInt32)Marshal.SizeOf(CanfdMsg);//FrameSize is depend on the crf protocol. 

            UInt32 frameNum = validDataSize / oneFrameSize;
            CANDTU_DLL.Frame[] CanfdMsgArray = new CANDTU_DLL.Frame[frameNum];
            
            if (isCompress==1)
            {
                P2dataBuffer = CANDTU_DLL.DTU_GetUnCompressData(DTU_handle);
                Console.WriteLine("P2dataBuffer={0}\n", P2dataBuffer.ToInt64());
                //memcpy(dataBuf.get(), pData, validLen);
            }
            //把数组中每个元素的地址对应转成结构体
            for (int i = 0; i < frameNum; i++)
            {
                try
                {
                    CanfdMsgArray[i] = new CANDTU_DLL.Frame(); // 创建结构体实例
                    if (isCompress == 1)
                    {
                        Marshal.PtrToStructure((IntPtr)(P2dataBuffer.ToInt64() + (i * Marshal.SizeOf(CanfdMsgArray[i]))), CanfdMsgArray[i]);
                    }
                    else
                    {
                        Marshal.PtrToStructure((IntPtr)(P2dataBuffer.ToInt64() + (i * Marshal.SizeOf(CanfdMsgArray[i]))), CanfdMsgArray[i]);
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.Write("error\n");
                    Console.Write("P2dataBuffer.ToInt64()={0}\n", P2dataBuffer.ToInt64());
                    //Console.WriteLine("(P2dataBuffer.ToInt64() + (i * Marshal.SizeOf(awCanStdMsgArray[i])={0}\n",(P2dataBuffer.ToInt64() + (i * Marshal.SizeOf(awCanStdMsgArray[i]));
                    Console.WriteLine("awCanStdMsgArray[i]={0}\n", CanfdMsgArray[i]);
                    Console.Write("{0}....", ex.Message);

                    throw;
                }

            }

            //Marshal.PtrToStructure()


            if (validDataSize % oneFrameSize != 0)
            {
                Console.Write("文件报文总长度无法整除单帧长度");//read valid length %d not enough for multi frames! 原本的注释为读取到的真实值不足以完成多帧传输。
            }

            if (validDataSize > nNumberOfBytesToRead)
            {
                Console.Write("预留的取值空间过小了");
            }

            Console.Write("frameNum={0}\n", frameNum);


            //clear unmanaged buffer 
            Marshal.FreeHGlobal(P2fileHeader);
            Marshal.FreeHGlobal(P2dataBuffer);

            for (UInt32 i = 0; i < frameNum; i++)
            {

                Console.WriteLine("timestamp={0},channal={1},id={2},dir={3},echo={4},fd={5},brs={6},err={7},trigger={8},length={9},data={10}",
                                    CanfdMsgArray[i].Timestamp,
                                    CanfdMsgArray[i].Channel,
                                    CanfdMsgArray[i].Id,
                                    CanfdMsgArray[i].Tx,
                                    CanfdMsgArray[i].Echo,
                                    CanfdMsgArray[i].Fd,
                                    CanfdMsgArray[i].Brs,
                                    CanfdMsgArray[i].Err,
                                    CanfdMsgArray[i].Trigger,
                                    CanfdMsgArray[i].Len,
                                    BitConverter.ToString(CanfdMsgArray[i].Data).Replace("-", " "));
                              
            }
            System.Threading.Thread.Sleep(500);
        }

    }
    public class CANDTU_DLL
    {
        [DllImport("CANDTUFileAccess.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 DTU_Create(byte file_type);


        [DllImport("CANDTUFileAccess.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool DTU_Open(UInt32 handle, IntPtr info);

        [DllImport("CANDTUFileAccess.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 DTU_GetFileSize(UInt32 handle);

        [DllImport("CANDTUFileAccess.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool DTU_GetVer2HeaderInfo(UInt32 handle, IntPtr P2Header);

        [DllImport("CANDTUFileAccess.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool DTU_GetVer3HeaderInfo(UInt32 handle, IntPtr P2Header);

        [DllImport("CANDTUFileAccess.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte DTU_GetDataType(UInt32 handle);

        [DllImport("CANDTUFileAccess.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool DTU_ReadAsFrm(UInt32 handle, IntPtr dataBuffer, uint nNumberOfBytesToRead, out uint nNumberOfByteRead, out uint validDataSize, out uint isCompress);

        [DllImport("CANDTUFileAccess.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr DTU_GetUnCompressData(UInt32 handle);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class _FileCreatedInfo
        {
            //[MarshalAs(UnmanagedType.ByValArray,SizeConst=4)]
            public IntPtr strFilePath;
            public byte dwDesiredAccess; // MODE_READ--1 MODE_WRITE--2 MODE_RW--3    大小为1Byte
        }

        //Version2
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Candtu_File_Info
        {
            public UInt16 type;
            public UInt16 ver;
            public UInt16 len;//#文件大小-24(文件头)-段数量*段头大小(8)              #这个应该怎么理解？   #文件大小=len- 24Bytes' (file header)-段数量*段头大小(8 Bytes ）
            public UInt64 time;
            public UInt32 fst;
            public UInt32 lst;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class aw_can_std_msg
        {
            public aw_can_msg can_msg;
            //public aw_can_msg can_msg 
            //{
            //    get { return _can_msg; }
            //    set { _can_msg = value; }

            //}//属性
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] MasgBuff;
            //public byte[] MasgBuff
            //{
            //    get { return _masgbuff; }
            //    set { _masgbuff = value; }

            //}//属性
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class aw_can_msg
        {
            public UInt32 _timestamp;
            //public byte timestamp { get; set; }//时间戳
            public UInt32 _id;
            //public byte id { get; set; }//
            public UInt16 _flags;
            //public byte flags { get; set; }//
            public byte _channel;
            //public byte channal { get; set; }
            public byte _length;
            //public byte length { get; set; }//
        }
        //Version3
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class _file_head
        {
            public UInt16 type;       // 数据类型
            public UInt16 ver;        // 版本
            public Byte mode;       // 记录模式
            public Byte flag;       // 记录模式标志
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public Byte[] res;     // 保留
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public Byte[] sn;     // 设备序列号
            public UInt32 tm_first;   // 文件第一帧数据时间UTC
            public UInt32 tm_end;     // 文件最后一帧时间UTC, s
            public UInt32 tm_start;   // 设备启动时间, s
            public UInt32 tm_offs;    // 时间偏移, s
            public UInt32 items;      // 文件内部块数目
            public UInt32 len;        // 数据域长度
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Frame
        {
        
            public UInt64 Timestamp;     // us

            public UInt32 Id;


            private UInt16 Flags;//Flags用于存放下列值，并用C#属性来实现位域的功能。
            
            //属性用于访问位域
            public UInt16 SendType  // 仅发送有效, 接收为0, 0:正常发送, 1:单次发送 2:自发自收
            {
                get { return (ushort)(Flags & (0x0003)); }
                set { Flags = (ushort)((Flags & ~0x0003) | (value & 0x0003)); }
            }
            public UInt16 Tx           // 1:tx 2:rx
            {
                get { return (ushort)((Flags & (0x0004)) >> 2); }
                set { Flags = (ushort)((Flags & ~0x0004) | ((value << 2) & 0x0004)); }
            }
            public UInt16 Echo
            {
                get { return (ushort)((Flags & (0x0008)) >> 3); }
                set { Flags = (ushort)((Flags & ~0x0008) | ((value << 3) & 0x0008)); }
            }
            public UInt16 Fd           // 1:canfd 2:can
            {
                get { return (ushort)((Flags & (0x0010)) >> 4); }
                set { Flags = (ushort)((Flags & ~0x0010) | ((value << 4) & 0x0010)); }
            }
            public UInt16 Rtr          // 1:remote 0:data frame
            {
                get { return (ushort)((Flags & (0x0020)) >> 5); }
                set { Flags = (ushort)((Flags & ~0x0020) | ((value << 5) & 0x0020)); }
            }
            public UInt16 Ext         // 1:extend 0:standard
            {
                get { return (ushort)((Flags & (0x0040)) >> 6); }
                set { Flags = (ushort)((Flags & ~0x0040) | ((value << 6) & 0x0064)); }
            }
            public UInt16 Err          // 1:error frame 0:normal frame;
            {
                get { return (ushort)((Flags & (0x080)) >> 7); }
                set { Flags = (ushort)((Flags & ~0x0009) | ((value << 7) & 0x0009)); }
            }
            public UInt16 Brs          // 1:canfd加速 0:不加速
            {
                get { return (ushort)((Flags & (0x0100)) >> 8); }
                set { Flags = (ushort)((Flags & ~0x000A) | ((value << 8) & 0x000A)); }
            }
            public UInt16 Esi          // 1:被动错误 0:主动错误
            {
                get { return (ushort)((Flags & (0x0200)) >> 9); }
                set { Flags = (ushort)((Flags & ~0x000B) | ((value << 9) & 0x000B)); }
            }
            public UInt16 Reserved     // 保留
            {
                get { return (ushort)((Flags & (0x7C00)) >> 10); }
                set { Flags = (ushort)((Flags & ~0x7C00) | ((value << 10) & 0x7C00)); }
            }
            public UInt16 Trigger      // 触发位
            {
                get { return (ushort)((Flags & (0x8000)) >> 15); }
                set { Flags = (ushort)((Flags & ~0x8000) | ((value << 15) & 0x8000)); }
            }
            //public UInt16 SendType;    // 仅发送有效, 接收为0, 0:正常发送, 1:单次发送 2:自发自收
            //public ushort Tx;           // 1:tx 2:rx
            //public ushort Echo;
            //public ushort Fd;           // 1:canfd 2:can
            //public ushort Rtr;          // 1:remote 0:data frame
            //public ushort Ext;          // 1:extend 0:standard
            //public ushort Err;          // 1:error frame 0:normal frame;
            //public ushort Brs;          // 1:canfd加速 0:不加速
            //public ushort Esi;          // 1:被动错误 0:主动错误
            //public ushort Reserved;     // 保留
            //public ushort Trigger;      // 触发位

            public byte Channel;        // 通道号, 若为-1, 代表该报文是发送给所有的通道



            public byte Len;

 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] Data;

          


        }
    }
}
    


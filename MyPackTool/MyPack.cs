/* 打包文件的实现：
 * 
 * 1.文件包格式：文件头+文件数据+文件头+文件数据.....
 * 2.头文件以结构体转byte[]形式存入文件包，固定分配10kb空间
 * 3.提供两个外部方：1.添加文件。2.输出文件包
 * 
 * 优化：
 * 1.取消文件头固定空间。在文件头之前使用特定字符串如“10]”，保存文件头的长度
 * 2.结构体中的文件名单独提取出来转化byte[]
 * 
 */
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MyPack
{
    public class MyPackTool:IDisposable 
    {
        private int positionCount;      //用于计算当前写入文件包的位置

        //文件头保存：1.文件名。2.文件数据起始位置。3.文件数据长度。
        //当前保存的变量主要为了实现需求：输入文件名获取文件数据。
        [Serializable]
        public  struct HeadInfo
        {
            public int start;
            public int length;
            public HeadInfo( int s, int l) {  start = s; length = l; }
        }


        //外部方法：添加一个文件到包
        public void AddFile(string name,byte[] file,string path)
        {

            //记录头文件信息并转为byte[]
            HeadInfo currentInfo = new HeadInfo(positionCount,file.Length);
            byte[] structByte = StructToBytes(currentInfo);
            //将文件信息和文件数据写入包中
            CopyToPackByte(name,structByte, file,path);
        }

        //将文件头和文件数据写入PackByte
        private void CopyToPackByte(string name,byte[] head, byte[]file,string path)
        {
            //文件名字长度
            byte[] nameByte = Encoding.Default.GetBytes(name);
            byte[] nameLength = BitConverter.GetBytes(nameByte.Length);
            int headLength = head.Length;
            byte[] headLengthByte = BitConverter.GetBytes(head.Length);

            //文件名长度+文件名+文件头长度
            byte[] hhByte = new byte[nameByte.Length + 8];
            Array.Copy(nameLength, 0, hhByte, 0, nameLength.Length);
            Array.Copy(nameByte, 0, hhByte, nameLength.Length, nameByte.Length);
            Array.Copy(headLengthByte, 0, hhByte, nameLength.Length + nameByte.Length, headLengthByte.Length);

            using(FileStream fs = new FileStream(path,FileMode.Append))
            {
                fs.Write(hhByte, 0, hhByte.Length);
                fs.Write(head, 0, head.Length);
                fs.Write(file, 0, file.Length);
            }
            positionCount += (headLength + file.Length + hhByte.Length);
        }

        //结构体转化为数组
        public static Byte[] StructToBytes(Object structure)
        {

            Int32 size = Marshal.SizeOf(structure);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, buffer, false);
                Byte[] bytes = new Byte[size];
                Marshal.Copy(buffer, bytes, 0, size);
                return bytes;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }


        public void Dispose()
        {
            //packByte = null;
        }
    }

}

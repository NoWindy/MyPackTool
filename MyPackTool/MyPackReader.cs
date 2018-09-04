/* 解包流程：
 * 1.找“]”字符，获取文件头长度
 * 2.读取文件头，将信息存入字典
 * 3.空间位置移动到下一个文件的开头，重复第一步
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PackReader
{
    public class MyPackReader : IDisposable
    {
        //要解析的文件包路径
        private string packPath;


        //文件头结构体
        [Serializable]
        public struct HeadInfo
        {
            //public string name;
            public int start;
            public int length;
            public HeadInfo(int s, int l) { start = s; length = l; }
        }


        //文件名和头文件信息绑定
        private Dictionary<string, HeadInfo> filesDic = new Dictionary<string, HeadInfo>();

        //构造函数：资源包解析。初始化主要目的是把资源包中所有文件头信息保存到字典中。
        public MyPackReader(string packPath)
        {
            this.packPath = packPath;

            using (FileStream fs = new FileStream(packPath,FileMode.Open))
            {
                //逐个文件读取，获取所有文件信息并保存到字典当中。
                while (fs.Position<fs.Length)
                {
                    //获取文件名字长度
                    byte[] hnByte = new byte[4];
                    fs.Read(hnByte, 0, 4);
                    int nameLength = hnByte[0] + hnByte[1] * 256 + hnByte[2] * 256 * 256 + hnByte[3] * 256 * 256 * 256;

                    //根据长度读取文件名
                    byte[] nameByte = new byte[nameLength];
                    fs.Read(nameByte, 0, nameLength);
                    string name = Encoding.Default.GetString(nameByte);

                    //读取文件头长度
                    byte[] hhByte = new byte[4];
                    fs.Read(hhByte, 0, 4);
                    int headLength = hhByte[0] + hhByte[1] * 256 + hhByte[2] * 256 * 256 + hhByte[3] * 256 * 256 * 256;

                    //根据长度读取文件头并转为结构体
                    byte[] headByte = new byte[headLength];
                    fs.Read(headByte, 0, headLength);
                    HeadInfo currentHead = (HeadInfo)BytesToStruct(headByte, typeof(HeadInfo));

                    //文件起始位置把上面的字节长度都加上
                    currentHead.start += 8 + headLength + nameLength;

                    //加入字典
                    filesDic.Add(name, currentHead);
                    //文件读取位置跳过当前文件数据空间，移动到下一文件起始
                    fs.Seek(currentHead.length,SeekOrigin.Current);
                }
            }
        }

        //外部方法：根据文件名获取文件数据
        //[Benchmark]
        public Byte[] GetFile(string name)
        {
            //先从缓存区查找文件
            if (MyCache.GetInstance().GetCacheFile(name)!=null)
                return MyCache.instance.GetCacheFile(name);

            //根据文件头信息读出文件
            if (!filesDic.ContainsKey(name)) throw new Exception("文件"+name+"不存在！");
            HeadInfo targetHead = filesDic[name];
            byte[] result = new byte[targetHead.length];
            using (FileStream fs = new FileStream(packPath, FileMode.Open))
            {
                fs.Seek(targetHead.start, SeekOrigin.Begin);
                fs.Read(result, 0, targetHead.length);
            }

            //当前文件进行一次规律判断
            MyCache.GetInstance().RuleDetection(name, result);

            return result;
        }


        //将byte[]转化为struct
        private static Object BytesToStruct(Byte[] bytes, Type strcutType)
        {
            Int32 size = Marshal.SizeOf(strcutType);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, buffer, size);
                return Marshal.PtrToStructure(buffer, strcutType);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }


        //清理缓存
        public void Dispose()
        {
            //packByte = null;
        }
    }
}

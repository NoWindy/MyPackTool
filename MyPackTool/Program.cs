using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using MyPack;
using PackReader;
using System.Text;

namespace PackTool
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            /*
            Console.Write("Path: ");
            string path = Console.ReadLine();
            Console.Write("PackName: ");
            string packName = Console.ReadLine();
            Console.Write("PackPath: ");
            string packPath = Console.ReadLine();

            string targetFilePath = "PackFolder/"+packName + ".pck";
            */

            /*
            byte[] b = Encoding.Default.GetBytes("aabb!~.adhi12- ()j ");
            int i = BitConverter.ToInt32(b,0);
            string s = Encoding.Default.GetString(b);
            */

            /*
            string path = "1";
            string targetFilePath = "PackFolder/MyPack.pck";

            //打包测试
            ResPacker.PackAllFile(path, targetFilePath);
            */


            //解包测试
            using( MyPackReader reader = new MyPackReader("PackFolder/MyPack.pck"))
            {
                File.WriteAllBytes("b.jpg", reader.GetFile("a.jpg"));
                //File.WriteAllBytes("b.jpg", reader.GetFile("a.jpg"));
                //File.WriteAllBytes("b.jpg", reader.GetFile("a.jpg"));
                //File.WriteAllBytes("a.mp3", reader.GetFile("Amaranth.mp3"));
                File.WriteAllBytes("a.mp3", reader.GetFile("Amaranth.mp3"));
                //File.WriteAllBytes("a.mp3", reader.GetFile("Amaranth.mp3"));
                //File.WriteAllBytes("a.mp3", reader.GetFile("Amaranth.mp3"));
            }
        }
    }

    
    public static class ResPacker
    {
        //[Benchmark]
        //打包文件夹内的所有文件
        public static void PackAllFile(string path,string targetFilePath)
        {
            if (File.Exists(targetFilePath))
                File.Delete(targetFilePath);

            //搜索出所有子文件并把路径保存到列表List中。
            List<string> filesList = new List<string>();
            GetFile(path, filesList);

            //将文件名+文件数据写入资源包中
            using (MyPackTool myPack = new MyPackTool())
            {
                foreach (string filePath in filesList)
                {
                    myPack.AddFile(Path.GetFileName(filePath), File.ReadAllBytes(filePath),targetFilePath);
                }
            }
        }

        //根据路径获取所有子文件
        public static List<string> GetFile(string path, List<string> FileList)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] fil = dir.GetFiles();
            DirectoryInfo[] dii = dir.GetDirectories();
            //获取子文件
            foreach (FileInfo f in fil)
            {
                long size = f.Length;
                FileList.Add(f.FullName);
            }
            //获取子文件夹内的文件列表，递归遍历
            foreach (DirectoryInfo d in dii)
            {
                GetFile(d.FullName, FileList);
            }
            return FileList;
        }

    }
}

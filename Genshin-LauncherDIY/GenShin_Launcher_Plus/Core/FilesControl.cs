﻿using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace GenShin_Launcher_Plus.Core
{
    public class FilesControl
    {
        public void StreamToFile(Stream stream, string fileName)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            FileStream fs = new(fileName, FileMode.Create);
            BinaryWriter bw = new(fs);
            bw.Write(bytes);
            bw.Close();
            fs.Close();
        }
        public static string ReadHTML(string url, string encoding)
        {
            string strHTML = "";
            try
            {
                System.Net.WebClient myWebClient = new();
                myWebClient.Proxy = null;
                Stream myStream = myWebClient.OpenRead(url);
                StreamReader sr = new StreamReader(myStream, Encoding.GetEncoding(encoding));
                strHTML = sr.ReadToEnd();
                myStream.Close();
            }
            catch
            {
                return "";
            }
            return strHTML;
        }
        public static string MiddleText(string Str, string preStr, string nextStr)
        {
            try
            {
                string tempStr = Str.Substring(Str.IndexOf(preStr) + preStr.Length);
                tempStr = tempStr.Substring(0, tempStr.IndexOf(nextStr));
                return tempStr;
            }
            catch
            {
                return "读取错误，请检查网络后再试！";
            }
        }
        public static bool UnZip(string zipFile, string directory)
        {
            try
            {
                ZipInputStream f = new(File.OpenRead(zipFile));
            A: ZipEntry zp = f.GetNextEntry();
                while (zp != null)
                {
                    string un_tmp2;
                    if (zp.Name.EndsWith("/"))
                    {
                        int tmp1 = zp.Name.LastIndexOf("/");
                        un_tmp2 = zp.Name.Substring(0, tmp1);
                        Directory.CreateDirectory(directory + un_tmp2);
                    }
                    if (!zp.IsDirectory && zp.Crc != 00000000L)
                    {
                        int i = 2048;
                        byte[] b = new byte[i];
                        FileStream s = File.Create(directory + zp.Name);
                        while (true)
                        {
                            i = f.Read(b, 0, b.Length);
                            if (i > 0)
                                s.Write(b, 0, i);
                            else
                                break;
                        }
                        s.Close();
                    }
                    goto A;
                }
                f.Close();
                return true;
            }
            catch { return false; }
        }
        public void FileWriter(string resName, string fileName)
        {
            var resUri = $"pack://application:,,,/{resName}";
            var uri = new Uri(resUri, UriKind.RelativeOrAbsolute);
            var stream = Application.GetResourceStream(uri).Stream;
            StreamToFile(stream, fileName);
        }
    }
}
﻿using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Net;
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

        public bool DownloadFile(string url, string toDirectory, string fileName, int timeout = 3000)
        {
            if (!Directory.Exists(toDirectory))
            {
                Directory.CreateDirectory(toDirectory);
            }

            FileStream file = File.OpenWrite(fileName);
            WebRequest request = WebRequest.Create(url);
            request.Timeout = timeout;

            try
            {
                request.GetResponse().GetResponseStream().CopyTo(file);
            }
            catch
            {
                return false;
            }

            file.Close();
            return true;
        }
    }
}
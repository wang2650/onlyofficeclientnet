﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
namespace OnlyOfficeDocumentClientNetCore.Common
{

    public static class StreamExtension
    {
        #region ToFile(将流写入指定文件路径)

        /// <summary>
        /// 将流写入指定文件路径
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static bool ToFile(this Stream stream, string path)
        {
            if (stream == null)
            {
                return false;
            }

            const int bufferSize = 32768;
            bool result = true;
            Stream fileStream = null;
            byte[] buffer = new byte[bufferSize];
            try
            {
                using (fileStream = File.OpenWrite(path))
                {
                    int len;
                    while ((len = stream.Read(buffer, 0, bufferSize)) > 0)
                    {
                        fileStream.Write(buffer, 0, len);
                    }
                }
            }
            catch
            {
                result = false;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }

            return (result && File.Exists(path));
        }

        #endregion

        #region ContentsEqual(比较流内容是否相等)

        /// <summary>
        /// 比较流内容是否相等
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="other">待比较的流</param>
        /// <returns></returns>
        public static bool ContentsEqual(this Stream stream, Stream other)
        {
            if (stream==null)
            {
                return false;
            }
            if (other == null)
            {
                return false;
            }
            if (stream.Length != other.Length)
            {
                return false;
            }

            const int bufferSize = 2048;
            byte[] streamBuffer = new byte[bufferSize];
            byte[] otherBuffer = new byte[bufferSize];

            while (true)
            {
                int streamLen = stream.Read(streamBuffer, 0, bufferSize);
                int otherLen = other.Read(otherBuffer, 0, bufferSize);

                if (streamLen != otherLen)
                {
                    return false;
                }

                if (streamLen == 0)
                {
                    return true;
                }

                int iterations = (int)Math.Ceiling((double)streamLen / sizeof(Int64));
                for (int i = 0; i < iterations; i++)
                {
                    if (BitConverter.ToInt64(streamBuffer, i * sizeof(Int64)) !=
                        BitConverter.ToInt64(otherBuffer, i * sizeof(Int64)))
                    {
                        return false;
                    }
                }
            }
        }

        #endregion

        #region GetReader(获取流读取器)

        /// <summary>
        /// 获取流读取器，默认编码：UTF-8
        /// </summary>
        /// <param name="stream">流</param>
        /// <returns></returns>
        public static StreamReader GetReader(this Stream stream)
        {
            return GetReader(stream, null);
        }

        /// <summary>
        /// 获取流读取器，使用指定编码
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码，默认：UTF-8</param>
        /// <returns></returns>
        public static StreamReader GetReader(this Stream stream, Encoding encoding)
        {
            if (stream.CanRead == false)
            {
                throw new InvalidOperationException("Stream 不支持读取操作");
            }
            encoding = encoding ?? Encoding.UTF8;
            return new StreamReader(stream, encoding);
        }

        #endregion

        #region GetWriter(获取流写入器)

        /// <summary>
        /// 获取流写入器，默认编码：UTF-8
        /// </summary>
        /// <param name="stream">流</param>
        /// <returns></returns>
        public static StreamWriter GetWriter(this Stream stream)
        {
            return GetWriter(stream, null);
        }

        /// <summary>
        /// 获取流写入器，使用指定编码
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码，默认编码：UTF-8</param>
        /// <returns></returns>
        public static StreamWriter GetWriter(this Stream stream, Encoding encoding)
        {
            if (stream.CanWrite == false)
            {
                throw new InvalidOperationException("Stream 不支持写入操作");
            }

            encoding = encoding ?? Encoding.UTF8;
            return new StreamWriter(stream, encoding);
        }

        #endregion

        #region ReadToEnd(读取所有文本)

        /// <summary>
        /// 从流中读取所有文本，默认编码：UTF-8
        /// </summary>
        /// <param name="stream">流</param>
        /// <returns></returns>
        public static string ReadToEnd(this Stream stream)
        {
            return ReadToEnd(stream, null);
        }

        /// <summary>
        /// 从流中读取所有文本，使用指定编码
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码，默认编码：UTF-8</param>
        /// <returns></returns>
        public static string ReadToEnd(this Stream stream, Encoding encoding)
        {
            using (var reader = stream.GetReader(encoding))
            {
                return reader.ReadToEnd();
            }
        }

        #endregion

        #region SeekToBegin(设置流指针指向流的开始位置)

        /// <summary>
        /// 设置流指针指向流的开始位置
        /// </summary>
        /// <param name="stream">流</param>
        /// <returns></returns>
        public static Stream SeekToBegin(this Stream stream)
        {
            if (stream.CanSeek == false)
            {
                throw new InvalidOperationException("Stream 不支持寻址操作");
            }

            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        #endregion

        #region SeekToEnd(设置流指针指向流的结束位置)

        /// <summary>
        /// 设置流指针指向流的结束位置
        /// </summary>
        /// <param name="stream">流</param>
        /// <returns></returns>
        public static Stream SeekToEnd(this Stream stream)
        {
            if (stream.CanSeek == false)
            {
                throw new InvalidOperationException("Stream 不支持寻址操作");
            }

            stream.Seek(0, SeekOrigin.End);
            return stream;
        }

        #endregion

        #region CopyToMemory(复制流到内存流)

        /// <summary>
        /// 将流复制到内存流中
        /// </summary>
        /// <param name="stream">流</param>
        /// <returns></returns>
        public static MemoryStream CopyToMemory(this Stream stream)
        {
            var memoryStream = new MemoryStream((int)stream.Length);
            stream.CopyTo(memoryStream);
            return memoryStream;
        }

        #endregion

        #region ReadAllBytes(将流写入字节数组)

        /// <summary>
        /// 将流写入字节数组
        /// </summary>
        /// <param name="stream">流</param>
        /// <returns></returns>
        public static byte[] ReadAllBytes(this Stream stream)
        {
            using (var memoryStream = stream.CopyToMemory())
            {
                return memoryStream.ToArray();
            }
        }

        #endregion

        #region Write(将字节数组写入流)

        /// <summary>
        /// 将字节数组写入流
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="bytes">字节数组</param>
        public static void Write(this Stream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        #endregion

        public static string ToMD5Hash(this Stream @this)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(@this);
                var sb = new StringBuilder();
                foreach (byte bytes in hashBytes)
                {
                    sb.Append(bytes.ToString("X2"));
                }

                return sb.ToString();
            }
        }
        public static void SkipLines(this StreamReader @this, int skipCount)
        {
            for (var i = 0; i < skipCount && !@this.EndOfStream; i++)
            {
                @this.ReadLine();
            }
        }
    }
}

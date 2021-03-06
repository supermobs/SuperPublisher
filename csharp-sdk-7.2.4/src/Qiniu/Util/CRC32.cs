﻿using System;
using System.IO;

namespace Qiniu.Util
{
    /// <summary>
    /// CRC32计算器
    /// </summary>
    public class CRC32
    {
        /// <summary>
        /// magic
        /// </summary>
        public const UInt32 IEEE = 0xedb88320;
        private UInt32[] Table;
        private UInt32 Value;

        /// <summary>
        /// 初始化
        /// </summary>
        public CRC32()
        {
            Value = 0;
            Table = makeTable(IEEE);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="p">字节数据</param>
        /// <param name="offset">偏移位置</param>
        /// <param name="count">字节数</param>
        public void write(byte[] p, int offset, int count)
        {
            this.Value = update(this.Value, this.Table, p, offset, count);
        }

        /// <summary>
        /// 校验和
        /// </summary>
        /// <returns>校验和</returns>
        public uint sum()
        {
            return this.Value;
        }

        private static uint[] makeTable(UInt32 poly)
        {
            UInt32[] table = new UInt32[256];
            for (int i = 0; i < 256; i++)
            {
                UInt32 crc = (UInt32)i;
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) == 1)
                        crc = (crc >> 1) ^ poly;
                    else
                        crc >>= 1;
                }
                table[i] = crc;
            }
            return table;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="crc">crc32</param>
        /// <param name="table">表</param>
        /// <param name="p">字节数据</param>
        /// <param name="offset">偏移位置</param>
        /// <param name="count">字节数</param>
        /// <returns></returns>
        public static uint update(UInt32 crc, UInt32[] table, byte[] p, int offset, int count)
        {
            crc = ~crc;
            for (int i = 0; i < count; i++)
            {
                crc = table[((byte)crc) ^ p[offset + i]] ^ (crc >> 8);
            }
            return ~crc;
        }

        /// <summary>
        /// 计算字节数据的crc32值
        /// </summary>
        /// <param name="data">二进制数据</param>
        /// <returns>crc32值</returns>
        public static uint checkSumBytes(byte[] data)
        {
            CRC32 crc = new CRC32();
            crc.write(data, 0, data.Length);
            return crc.sum();
        }

        /// <summary>
        /// 检验
        /// </summary>
        /// <param name="data">字节数据</param>
        /// <param name="offset">偏移位置</param>
        /// <param name="count">字节数</param>
        /// <returns></returns>
        public static uint checkSumSlice(byte[] data, int offset, int count)
        {
            CRC32 crc = new CRC32();
            crc.write(data, offset, count);
            return crc.sum();
        }

        /// <summary>
        /// 计算沙盒文件的crc32值
        /// </summary>
        /// <param name="filePath">沙盒文件全路径</param>
        /// <returns>crc32值</returns>
        public static uint checkSumFile(string filePath)
        {
            CRC32 crc = new CRC32();
            int bufferLen = 32 * 1024;
			using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[bufferLen];
                while (true)
                {
                    int n = fs.Read(buffer, 0, bufferLen);
                    if (n == 0)
                        break;
                    crc.write(buffer, 0, n);
                }
            }
            return crc.sum();
        }
    }
}
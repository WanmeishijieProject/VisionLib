using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Package
{
    public class CheckSet
    {
        public static byte CheckSum(byte[] buf, int offset, int count)
        {
            if (buf.Length < count)
                return 0;
            byte checksum = 0;
            for (int i = offset; i < offset + count; i++)
            {
                checksum += buf[i];
            }
            return (byte)(checksum & 0xFF);
        }
       
    }
    public class CRC32
    {
        const UInt32 INIT_VAL = 0xFFFFFFFF;
        const UInt32 POLY = 0x4C11DB7;
        UInt32 _dr = INIT_VAL;

        /// <summary>
        /// Get the 32-bit CRC value.
        /// </summary>
        public UInt32 CRC
        {
            get
            {
                return _dr;
            }
            private set
            {
                _dr = value;
            }
        }

        /// <summary>
        /// Reset the data register of the CRC
        /// </summary>
        private void Reset()
        {
            this.CRC = INIT_VAL;
        }

        /// <summary>
        /// Computes the 32-bit CRC of 32-bit data buffer independently of the previous CRC value.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        public UInt32 Calculate(UInt32[] Buffer, int Length)
        {
            Reset();
            return Accumulate(Buffer, Length);
        }

        public UInt32 Calculate(byte[] Buffer,int Offset, int Length)
        {
            UInt32[] data = Byte2Uint32(Buffer, Offset, Length);
            return Calculate(data, data.Length);
        }
     
        /// <summary>
        /// Computes the 32-bit CRC of 32-bit data buffer using combination of the previous CRC value and the new one.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        private UInt32 Accumulate(UInt32[] Buffer, int Length)
        {
            for (int i = 0; i < Length; i++)
            {
                calculate(Buffer[i]);
            }

            return this.CRC;
        }

        /// <summary>
        /// Computs the 32-bit CRC using the preivous CRC value and the new value.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private UInt32 calculate(UInt32 data)
        {
            int bindex = 0;
            UInt32 crc = data ^ this.CRC;

            while (bindex < 32)
            {
                if ((crc & 0x80000000) > 0)
                {
                    crc = (crc << 1) ^ POLY;
                }
                else
                {
                    crc <<= 1;
                }

                bindex++;
            }

            this.CRC = crc;

            return crc;
        }
        private uint[] Byte2Uint32(byte[] data, int offset, int length)
        {
            List<UInt32> result = new List<uint>();
            int len = (length - offset) / 4;

            if ((length - offset) % 4 != 0)
            {
                len = len + 1;
            }
            for (int i = offset; i < len; i++)
            {
                UInt32 ret = 0;
                for (int j = 0; j < 4; j++)
                {
                    int index = 4 * i + j + offset;
                    if (index < length+offset)
                        ret += (UInt32)(data[index] << (j * 8));
                }
                result.Add(ret);
            }
            return result.ToArray();
        }

    }
}

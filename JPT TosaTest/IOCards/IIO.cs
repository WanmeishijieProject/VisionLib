using JPT_TosaTest.Config.HardwareManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.IOCards
{
    public enum EnumIOType
    {
        INPUT,
        OUTPUT,
    }
    public delegate void IOStateChange(IIO sender, EnumIOType IOType, UInt16 OldValue, UInt16 NewValue);
    public interface IIO
    {
        event IOStateChange OnIOStateChanged;

        IOCardCfg ioCfg { get; set; }

        /// <summary>
        /// 初始化IO卡
        /// </summary>
        /// <returns></returns>
        bool Init(IOCardCfg ioCfg, ICommunicationPortCfg communicationPortCfg);

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <returns></returns>
        bool Deinit();

        /// <summary>
        /// 读取Index位的状态
        /// </summary>
        /// <param name="Index">
        /// 从0开始
        /// </param>
        /// <returns></returns>
        bool ReadIoInBit(int Index, out bool value);

        /// <summary>
        /// 读取输出位的状态
        /// </summary>
        /// <param name="Index">从0开始</param>
        /// <returns></returns>
        bool ReadIoOutBit(int Index, out bool value);

        /// <summary>
        /// 写输出位的状态值
        /// </summary>
        /// <param name="Index">从零开始0-15</param>
        /// <param name="value">写入的状态</param>
        /// <returns></returns>
        bool WriteIoOutBit(int Index,bool value);

        /// <summary>
        /// 读取输入字的值
        /// </summary>
        /// <param name="StartIndex">起始位</param>
        /// <returns></returns>
        bool ReadIoInWord(int StartIndex, out int value);

        /// <summary>
        /// 读取输出字的值
        /// </summary>
        /// <param name="StartIndex">其实位的，默认从0开始</param>
        /// <returns></returns>
        bool ReadIoOutWord(int StartIndex, out int value);

        /// <summary>
        /// 一次写16位的状态值
        /// </summary>
        /// <param name="StartIndex">起始位</param>
        /// <param name="value">写入的状态字的值</param>
        /// <returns></returns>
        bool WriteIoOutWord(int StartIndex, UInt16 value);
    }
}

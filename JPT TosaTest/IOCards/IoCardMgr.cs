using System;
using System.Collections.Generic;

namespace JPT_TosaTest.IOCards
{
    public class IOCardMgr
    {
        private IOCardMgr()
        {

        }
        private static readonly Lazy<IOCardMgr> _instance = new Lazy<IOCardMgr>(() => new IOCardMgr());
        public static IOCardMgr Instance
        {
            get { return _instance.Value; }
        }
        public Dictionary<string, IOBase> IOCardDic = new Dictionary<string, IOBase>();

        public void AddIOCard(string CardName, IOBase IOCard)
        {
            bool bFind = false;
            foreach (var it in IOCardDic)
            {
                if (it.Key == CardName)
                {
                    bFind = true;
                    return;
                }
            }
            if (!bFind)
                IOCardDic.Add(CardName, IOCard);
        }
        public void RemoveIOCard(string CardName)
        {
            foreach (var it in IOCardDic)
            {
                if (it.Key == CardName)
                    IOCardDic.Remove(CardName);
            }
        }
        public IOBase FindIOCardByCardName(string CardName)
        {
            foreach (var it in IOCardDic)
            {
                if (it.Key.Equals(CardName))
                    return it.Value;
            }
            return null;
        }
        public IOBase FindIOCardByCardNo(int CardIndex)
        {
            int i = 0;
            foreach (var it in IOCardDic)
            {
                if (i++ == CardIndex)
                    return it.Value;
            }
            return null;
        }

        public bool Deinit(string CardName)
        {
            var card = FindIOCardByCardName(CardName);
            if (card != null)
                return card.Deinit();
            return false;  
        }
     
        public bool ReadIoInBit(string CardName, int Index, out bool value)
        {
            value = false;
            var card = FindIOCardByCardName(CardName);
            if (card != null)
                return card.ReadIoInBit(Index, out bool retValue);
            return false;
        }

        public bool ReadIoOutBit(string CardName, int Index, out bool value)
        {
            value = false;
            var card = FindIOCardByCardName(CardName);
            if (card != null)
                return card.ReadIoOutBit(Index, out value);
            return false;
        }
  
        public bool WriteIoOutBit(string CardName, int Index, bool value)
        {
            var card = FindIOCardByCardName(CardName);
            if (card != null)
                return card.WriteIoOutBit(Index,value);
            return false;
        }

        public bool ReadIoInWord(string CardName, int StartIndex, out int value)
        {
            value = 0;
            var card = FindIOCardByCardName(CardName);
            if (card != null)
                return card.ReadIoInWord(StartIndex, out value);
            return false;
        }

        public bool ReadIoOutWord(string CardName, int StartIndex, out int value)
        {
            value = 0;
            var card = FindIOCardByCardName(CardName);
            if (card != null)
                return card.ReadIoOutWord(StartIndex, out value);
            return false;
        }

        public bool WriteIoOutWord(string CardName, int StartIndex, UInt16 value)
        {
            var card = FindIOCardByCardName(CardName);
            if (card != null)
                return card.WriteIoOutWord(StartIndex,value);
            return false;
        }



    }
}

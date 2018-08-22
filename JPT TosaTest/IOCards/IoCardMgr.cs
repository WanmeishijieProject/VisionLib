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
     
        public bool ReadIoInBit(string CardName, int Index)
        {
            var card = FindIOCardByCardName(CardName);
            if (card != null)
                return card.ReadIoInBit(Index);
            return false;
        }

        public bool ReadIoOutBit(string CardName, int Index)
        {
            var card = FindIOCardByCardName(CardName);
            if (card != null)
                return card.ReadIoOutBit(Index);
            return false;
        }
  
        public bool WriteIoOutBit(string CardName, int Index, bool value)
        {
            var card = FindIOCardByCardName(CardName);
            if (card != null)
                return card.WriteIoOutBit(Index,value);
            return false;
        }

        public int ReadIoInWord(string CardName, int StartIndex = 0)
        {
            var card = FindIOCardByCardName(CardName);
            if (card != null)
                return card.ReadIoInWord(StartIndex);
            return -1;
        }

        public int ReadIoOutWord(string CardName, int StartIndex = 0)
        {
            var card = FindIOCardByCardName(CardName);
            if (card != null)
                return card.ReadIoOutWord(StartIndex);
            return -1;
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

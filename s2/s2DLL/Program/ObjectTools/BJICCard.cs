using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Com.Aote.ObjectTools;
using System.Runtime.InteropServices.Automation;
using System.Windows.Browser;
using System.ComponentModel;
using Com.Aote.Logs;

namespace Com.Aote.ObjectTools
{
    //宝鸡ic卡对象
    public class BJICCard : CustomTypeHelper, IAsyncObject
    {
        private static Log Log = Log.GetInstance("Com.Aote.ObjectTools.BJICCard");

        //读卡控件
        dynamic obj;// = AutomationFactory.CreateObject("ICCARD.ICCardCtrl.1");

        private bool init = false;
        public bool Init
        {
            get { return init; }
            set
            {
                this.init = value;
                if (init)
                {
                    obj = AutomationFactory.CreateObject("YLIC.YLICCtrl.1");
                   
                }
            }
        }

        #region CanInitCard 是否可以发初始化卡
        //是否可发初始化卡
        private bool canInitCard = false;
        public bool CanInitCard
        {
            get { return canInitCard; }
            set
            {
                this.canInitCard = value;
                if (canInitCard)
                {
                    this.ReInitCard();
                }
            }
        }
        #endregion

        #region CanRenewCard 是否可以补卡
        //是否可发初始化卡
        private bool canRenewCard = false;
        public bool CanRenewCard
        {
            get { return canRenewCard; }
            set
            {
                this.canRenewCard = value;
                if (canRenewCard)
                {
                    this.MakeUp();
                    this.canRenewCard = false;
                }
            }
        }
        #endregion

        public BJICCard()
        {
        }

        public BJICCard(bool isInit)
        {
            if (isInit)
            {
                obj = AutomationFactory.CreateObject("YLIC.YLICCtrl.1");
            }
            
        }

        #region CanSellGas 是否可以售气
        public static readonly DependencyProperty CanSellGasProperty =
            DependencyProperty.Register("CanSellGas", typeof(bool), typeof(BJICCard),
            new PropertyMetadata(new PropertyChangedCallback(OnCanSellGasChanged)));
        public bool CanSellGas
        {
            get { return (bool)GetValue(CanSellGasProperty); }
            set { SetValue(CanSellGasProperty, value); }
        }

        //如果可以售气，调用售气过程
        private static void OnCanSellGasChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            BJICCard card = (BJICCard)dp;
            if (card.CanSellGas)
            {
                card.SellGas();
                card.CanSellGas = false;
            }
        }
        #endregion

        #region 读卡
        /// <summary>
        /// 读卡
        /// </summary>
        public void ReadCard()
        {
            IsBusy = true;
            Error = "";
            CardId = "";
            Factory = "";
            Gas = 0;
            BuyTimes = 0;
            RenewTimes = 0;
            State = State.StartLoad;
            //读卡
            int openRen = GetEvent("ReadCard").ReadCard();
            if (openRen == 0)
            {
                State = State.Loaded;
                Log.Debug("read card loaded");
                CardId = GetEvent("GetCardId").GetCardId();
                Factory = GetEvent("GetFactory").GetFactory();
                Gas = GetEvent("GetGas").GetGas();
                BuyTimes = GetEvent("GetTimes").GetTimes();
                RenewTimes = GetEvent("GetRenewTimes").GetRenewTimes();
                MeterID = GetEvent("GetMeterID").GetMeterID();
            }
            else
            {
                State = State.LoadError;
                Error = "读卡失败,错误代码:" + openRen;
            }
            IsBusy = false;
            OnReadCompleted(null);
        }
        #endregion

        #region 读卡海力采集卡
        /// <summary>
        /// 
        /// 读卡海力采集卡
        /// </summary>
        public void HLReadCard()
        {
            IsBusy = true;
            Error = "";
            GetCardId = "";
            GetHLDataGas = 0;
            GetLmugGas = 0;
            State = State.StartLoad;
            //读卡
            int openRen = GetEvent("ReadCard").ReadCard();
            if (openRen == 0)
            {
                State = State.Loaded;
                CardId = GetEvent("CardId").GetCardId();
                GetHLDataGas = GetEvent("GetHLDataGas").GetHLDataGas();
                GetLmugGas = GetEvent("GetLmugGas").GetLmugGas();
            }
            else
            {
                State = State.LoadError;
                Error = "读卡失败,错误代码:" + openRen;
            }
            IsBusy = false;
            OnReadCompleted(null);
        }
        #endregion

        #region 售气
        public void SellGas()
        {
            IsBusy = true;
            Error = "";
            State = State.Start;
            //设置 脉冲系数， 表型编码
            GetEvent("SerIcardCont").SerIcardCont(IcardCont);
            GetEvent("SetTableCont").SetTableCont(TableCont);
            //设置补卡次数
            GetEvent("SetRenewTimes").SetRenewTimes(RenewTimes);
            int sellRen = GetEvent("SellGas").SellGas(Factory, CardId, Gas, BuyTimes, Price, ScBuyGas, SscBuyGas);
            //售气成功返回true,失败false;
            if (sellRen==0)
            {
                State = State.End;
            }
            else
            {
                State = State.Error;
                Error = "售气失败,错误代码:" + sellRen;
            }
            IsBusy = false;
            OnCompleted(new AsyncCompletedEventArgs(null, true, null));
        }
        #endregion

        //发初始化卡
        public void ReInitCard()
        {
            this.State = State.Start;
            IsBusy = true;
            Error = "";
            
            //设置 脉冲系数， 表型编码
            GetEvent("SerIcardCont").SerIcardCont(IcardCont);
            GetEvent("SetTableCont").SetTableCont(TableCont);
            //发卡
            int initRen = GetEvent("Init").Init(Factory, CardId, Gas, BuyTimes, Price);
            if (initRen == 0)
            {
                State = State.End;
            }
            else
            {
                State = State.Error;
                Error = "发卡不成功,错误代码:" + initRen;
            }
            IsBusy = false;
            OnCompleted(null);
        }

        //制作工程卡包括天庆一体、天庆工业、开元新表、海力民用、海力工业
        public void MakeTestCard()
        {
            this.State = State.Start;
            IsBusy = true;
            Error = "";

            //制作工程卡
            int initRen = GetEvent("MakeTestCard").MakeTestCard(Factory);
            if (initRen == 0)
            {
                State = State.End;
            }
            else
            {
                State = State.Error;
                Error = "制工程卡不成功,错误代码" + initRen;
            }
            IsBusy = false;
            OnCompleted(null);
        }

        /// 撤销:cardId, cardgas, cardmoney,selldate
        public void ReturnGas()
        {
            State = State.Start;
            IsBusy = true;
            Error = "";
            //设置 脉冲系数， 表型编码
            GetEvent("SerIcardCont").SerIcardCont(IcardCont);
            GetEvent("SetTableCont").SetTableCont(TableCont);

            int sellRen = GetEvent("RollBackGas").RollBackGas(Factory);
            //售气成功返回true,失败false;
            if (sellRen==0)
            {
                State = State.End;
            }
            else
            {
                State = State.Error;
                Error = "冲正不成功,错误代码:" + sellRen;
            }
            IsBusy = false;
            OnCompleted(null);
        }

        //补卡  
        public void MakeUp()
        {
            this.State = State.Start;
            IsBusy = true;
            Error = "";

            //设置 脉冲系数， 表型编码
            GetEvent("SerIcardCont").SerIcardCont(IcardCont);
            GetEvent("SetTableCont").SetTableCont(TableCont);
            //设置补卡次数
            GetEvent("SetRenewTimes").SetRenewTimes(RenewTimes);

            int initRen = GetEvent("ReNewCard").ReNewCard(Factory, CardId, Gas, BuyTimes, Price, MeterID, ScBuyGas, SscBuyGas);
            if (initRen==0)
            {
                State = State.End;
            }
            else
            {
                State = State.Error;
                Error = "补卡不成功,错误代码:" + initRen;
            }
            IsBusy = false;
            OnCompleted(null);
        }

        public void MakeNewCard()
        {
            IsBusy = true;
            Error = "";
            State = State.Start;

            //设置 脉冲系数， 表型编码
            GetEvent("SerIcardCont").SerIcardCont(IcardCont);
            GetEvent("SetTableCont").SetTableCont(TableCont);

            int initRen = GetEvent("MakeNewCard").MakeNewCard(Factory);
            if (initRen == 0)
            {
                State = State.End;
            }
            else
            {
                State = State.Error;
                Error = "卡檫除不成功,错误代码:" + initRen;
            }
            IsBusy = false;
            OnCompleted(null);
        }

        public void MakeClearCard()
        {
            IsBusy = true;
            Error = "";
            State = State.Start;

            //设置 脉冲系数， 表型编码
            GetEvent("SerIcardCont").SerIcardCont(IcardCont);
            GetEvent("SetTableCont").SetTableCont(TableCont);

            int initRen = GetEvent("MakeClearCard").MakeClearCard(Factory ,Gas  );
            if (initRen == 0)
            {
                State = State.End;
            }
            else
            {
                State = State.Error;
                Error = "清零卡制作不成功,错误代码:" + initRen;
            }
            IsBusy = false;
            OnCompleted(null);
        }

        //航天卡开锁
        public void HTOpen()
        {
            IsBusy = true;
            Error = "";
            State = State.Start;
            //发卡
            int initRen = GetEvent("HTOpen").HTOpen();
            if (initRen == 0)
            {
                State = State.End;
            }
            else
            {
                State = State.Error;
                Error = "此卡无需开锁,错误代码:" + initRen;
            }
            IsBusy = false;
            OnCompleted(null);
        }

        //开元新表用户卡初始化
        public void KYInit()
        {
            IsBusy = true;
            Error = "";
            State = State.Start;
            //用户卡初始化
            int initRen = GetEvent("KYInit").KYInit();
            if (initRen == 0)
            {
                State = State.End;
            }
            else
            {
                State = State.Error;
                Error = "初始化不成功,错误代码：" + initRen;
            }
            IsBusy = false;
            OnCompleted(null);
        }

        /// <summary>
        /// 获得控件方法对象
        /// </summary>
        /// <param name="EventName">方法名</param>
        /// <returns></returns>
        public dynamic GetEvent(string EventName)
        {
            AutomationFactory.GetEvent(obj, EventName);
            return obj;
        }

        /// <summary>
        /// 卡号自动补零
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public string AddZero(string Source, int len)
        {
            var strTemp = "";
            Source = Source + "";
            for (var i = 1; i <= len - Source.Length; i++)
            {
                strTemp += "0";
            }
            return strTemp + Source;
        }
        
        public string Name { get; set; }

        #region Error 卡错误
        public string error = "";
        public string Error
        {
            get { return error; }
            set
            {
                error = value;
                OnPropertyChanged("Error");
            }
        }
        #endregion

        #region 属性
        //脉冲系数， 表型编码  开元和海力使用
        public long IcardCont { get; set; }
        public long TableCont { get; set; }

        //
        public string factory;
        public string Factory
        {
            get { return factory; }
            set
            {
                this.factory = value;
                OnPropertyChanged("Factory");
            }
        }
        public string CardType { get; set; }
        public string Price { get; set; }
        //卡内气量
        private double gas;
        public double Gas
        {
            get { return gas; }
            set
            {
                this.gas = value;
                OnPropertyChanged("Gas");
            }
        }
        //表ID
        private long meterID;
        public long MeterID 
        {
            get { return meterID; }
            set
            {
                this.meterID = value;
                OnPropertyChanged("MeterID");
            }
        }
        //上次购气量
        private double scbuygas;
        public double ScBuyGas 
        {
            get { return scbuygas; }
            set 
            {
                this.scbuygas = value;
                OnPropertyChanged("ScBuyGas");
            }
        }
        //上上次购气量
        private double sscbuygas;
        public double SscBuyGas
        {
            get { return sscbuygas; }
            set
            {
                this.sscbuygas = value;
                OnPropertyChanged("SscBuyGas");
            }
        }
        //购气次数
        private int buytimes;
        public int BuyTimes
        {
            get { return buytimes; }
            set
            {
                this.buytimes = value;
                OnPropertyChanged("BuyTimes");
            }
        }

        //补卡次数
        private decimal renewtimes;
        public decimal RenewTimes
        {
            get { return renewtimes; }
            set
            {
                this.renewtimes = value;
                OnPropertyChanged("RenewTimes");
            }
        }

        //海力采集卡气量
        private int gethldatagas;
        public int GetHLDataGas
        {
            get { return gethldatagas; }
            set
            {
                this.gethldatagas = value;
                OnPropertyChanged("GetHLDataGas");
            }
        }

        //卡号
        private string cardid;
        public string CardId
        {
            get { return cardid; }
            set
            {
                this.cardid = value;
                OnPropertyChanged("CardId");
            }
        }

        public double Money { get; set; }
        public string BuyDate { get; set; }
        public int FactoryId { get; set; }

        public string MetreCon { get; set; }
        public int FirstAlarmV { get; set; }
        public int SecondAlamV { get; set; }
        public int NewRemnant { get; set; }
        public string InitTime { get; set; }
        public int Xs { get; set; }
        public int UserLevel { get; set; }
        public int UserType { get; set; }
        #endregion

        #region State 卡状态
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(State), typeof(BJICCard), null);

        public State State
        {
            get { return (State)GetValue(StateProperty); }
            set
            {
                SetValue(StateProperty, value);
            }
        }
        #endregion

        public event System.ComponentModel.AsyncCompletedEventHandler ReadCompleted;

        public void OnReadCompleted(System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (ReadCompleted != null)
            {
                ReadCompleted(this, e);
            }
        }

        public event System.ComponentModel.AsyncCompletedEventHandler Completed;

        public void OnCompleted(System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (Completed != null)
            {
                Completed(this, e);
            }
        }

        public bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        public int GetLmugGas { get; set; }

        public dynamic GetCardId { get; set; }
    }
}

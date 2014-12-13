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

namespace Com.Aote.ObjectTools
{
    //长安ic卡对象
    public class ICCardObj : CustomTypeHelper, IAsyncObject
    {
        //读卡控件
        dynamic obj;// = AutomationFactory.CreateObject("ICCARD.ICCardCtrl.1");
        //cpu卡控件
        dynamic cpuobj;// = AutomationFactory.CreateObject("CPUCARD.CPUCardCtrl.1");
        //售气控件
        dynamic sellobj;// = AutomationFactory.CreateObject("ICCARDNEW.ICCardNewCtrl.1");


        private bool init = false;
        public bool Init 
        {
            get { return init; }
            set
            {
                this.init = value;
                if (init)
                {
                    obj = AutomationFactory.CreateObject("ICCARD.ICCardCtrl.1");
                    sellobj = AutomationFactory.CreateObject("ICCARDNEW.ICCardNewCtrl.1");
                }
            }
        }

        public ICCardObj()
        {
            //读卡
            //ReadCard();


            //售气
            //Gas = 20;
            //BuyTimes = 2;
            //SellDate = "2011-11-11";
            //FactoryId = 14;
            //PriceNow = 1.98;
            //PriceNew = 1.98;
            //PriceDate = "2011-11-11";
            //Money = 100;
            //BuyTimes = 5;
            //SellGas();

            //发初始化卡
            //CardId = "0023333333";
            //Alarm = 10;
            //UserClass = "民用";
            //MeterType = 4;
            //Amount = 20;
            //ReInitCard();


            //补卡
            //CardId = "0021234567";
            //MeterType = 4;
            //MakeUp();

            //撤销
            //CardId = "0021111111";
            //Gas = 10;
            //ReturnGas();
        }

        #region 读卡
        /// <summary>
        /// 读卡
        /// </summary>
        public void ReadCard()
        {
            IsBusy = true;
            Error = "";
            //先读普通卡
            int openRen = GetEvent("Open").Open(0);
            //alert(openRen);
            if (openRen == 0)
            {
                ReadNormalCard();
                GetEvent("Close").Close();
            }
            else if (openRen == 1)
            {
                GetEvent("Close").Close();
                Error = "卡没有插入!";
                MessageBox.Show(Error);
                return;
            }
            else if (openRen == 2)
            {
                GetEvent("Close").Close();
                Error = "硬件连接错误：线没连接或读卡器有问题或计算机串口坏!";
                MessageBox.Show(Error);
                return;
            }
            else if (openRen == 3)
            {
                GetEvent("Close").Close();
                Error = "密码错误!";
                MessageBox.Show(Error);
                return;
            }
            //是cpu卡，关闭本控件串口,cpu卡串口打开关闭在readcpu内做
            else if (openRen == 4)
            {
                ReadCpu();
                GetEvent("Close").Close();
            }
            else
            {
                GetEvent("Close").Close();
            }
            IsBusy = false;
            OnReadCompleted(null);
        }

        /// <summary>
        /// 读cpu卡
        /// </summary>
        private void ReadCpu()
        {
            CardId = "";
            Factory = "";
            CardType = "";
            var cardTypeId = 6;
            CardType = CardTypeConver(cardTypeId);
            // alert(cardType);
            Gas = 0;
            BuyTimes = 0;
            Money= 0;
            BuyDate = "";
            FactoryId = 0;
            FirstAlarmV = 0;
            SecondAlamV = 0;
            NewRemnant = 0;
            InitTime = "";
            Xs = 0;
            UserLevel = 0;
            var openRen = GetCpuEvent("OpenPort").OpenPort(0);
            if (!openRen)
            {
                GetCpuEvent("ClosePort").ClosePort();
                Error = "硬件错误";
            }
            var cardAffRen = GetCpuEvent("CardAffim").CardAffim();
            if (!cardAffRen)
            {
                GetCpuEvent("ClosePort").ClosePort();
                Error = "认证错误";
            }
            var readRen = GetCpuEvent("ReadCard").ReadCard();
            if (!readRen)
            {
                GetCpuEvent("ClosePort").ClosePort();
                Error = "读卡失败!";
            }
            var ef3Read = GetCpuEvent("ReadEf3").ReadEf3();
            if (!ef3Read)
            {
                GetCpuEvent("ClosePort").ClosePort();
                Error = "读文件错误!";
            }
            
            //卡号
            CardId = cpuobj.Cardnumber;

            CardId = AddZero(CardId, 10);
            //第一次告警气量
            FirstAlarmV = cpuobj.FirstAlarmV;
            //第二次告警气量
            SecondAlamV = cpuobj.SecondAlamV;
            //新上线告警气量
            NewRemnant = cpuobj.NewRemnant;
            //购气次数
            BuyTimes = cpuobj.BuyGasTimes;
            //购气时间
            BuyDate = cpuobj.LastBuyGasTime;
            if (BuyDate != null && BuyDate.Length > 10)
            {
                BuyDate = BuyDate.Substring(0, 10);
            }
            // alert(buyDate);
            //发卡时间
            InitTime = cpuobj.InitiTime;
            //alert(initTime);
            //优惠系数
            Xs = cpuobj.xs;
            Xs = Xs / 100;
            //剩余气量
            Gas = cpuobj.NowRemantVO;
            //用户级别
            UserLevel = cpuobj.consumerlevel;
            //用户类型 ???和普通卡不一样
            UserType = cpuobj.consumertypes;
            //cardType = userType;
            GetCpuEvent("ClosePort").ClosePort();
            Factory = "秦港工业";
            FactoryId = 9;
        }


        /// <summary>
        /// 读一般卡 ，非cpu卡
        /// </summary>
        private void ReadNormalCard()
        {
            FactoryId = GetEvent("GetFactory").GetFactory();

            //factoryid==14,工业无线
            if (FactoryId == 14 || FactoryId == 16)
            {

                ReadWX();
            }
            else
            {
                ReadCommon();
            }
        }

        /// <summary>
        /// 读取无线表
        /// </summary>
        private void ReadWX()
        {
            CardId = "";
            Factory = "";
            CardType = "";
            Gas = 0;
            BuyTimes = 0;
            Money = 0;
            BuyDate = "";
            FactoryId = GetEvent("GetFactory").GetFactory();
            // alert("factoryId:"+factoryId);
            Factory = FactoryTypeConver(FactoryId);
            var cardTypeId = GetEvent("GetCardUsetype").GetCardUsetype();
            // alert(cardTypeId);
            //if(cardTypeId != 0 && cardTypeId !=6 && cardTypeId != 3)
            //{
            //   return "{isValid:false,tishi:'不是购气卡,不能购气!'}";
            //}
            CardType = CardTypeConver(cardTypeId);
            var cardnum = GetEvent("GetCardNum").GetCardNum();
            CardId = AddZero(cardnum + "", 8);
            var preCard = GetEvent("preCard").preCard();
            preCard = AddZero(preCard+"", 2);
            CardId = "" + preCard + CardId;
            // alert(cardNum);
            Money = GetEvent("GetGasNumber").GetGasNumber() / 100;
            //错误卡，气量为0
            if (!GetEvent("CheckCard").CheckCard())
            {
                Gas = 0;
            }
            BuyTimes = GetEvent("GetBuyGasTimes").GetBuyGasTimes();
            var buydate = GetEvent("GetDate").GetDate();
            BuyDate = buydate + "";
        }

        /// <summary>
        /// 读一般卡
        /// </summary>
        public void ReadCommon()
        {
            CardId = "";
            Factory = "";
            CardType = "";
            Gas = 0;
            BuyTimes = 0;
            Money = 0;
            BuyDate = "";
            FactoryId = GetEvent("GetFactory").GetFactory();
            //alert("表厂id:" + factoryId);
            Factory = FactoryTypeConver(FactoryId);
            //如果表长id为0，为华捷，为了取限购气量，转为1
            if (FactoryId == 0)
            {
                FactoryId = 1;
            }
            var cardTypeId = GetEvent("GetCardUseType").GetCardUseType();
            // if (cardTypeId != 0 && cardTypeId != 6 && cardTypeId != 3) {
            //      return "{isValid:false,tishi:'不是购气卡,不能购气!'}";
            // }
            CardType = CardTypeConver(cardTypeId);
            var cardnum = GetEvent("GetCardNum").GetCardNum();
            var cardId = cardnum + "";
            //alert("卡号"+cardNum);
            CardId = AddZero(cardId, 10);
            //如果是无线
            if (FactoryId == 13 || FactoryId == 14 || FactoryId == 15 || FactoryId == 16)
            {

                Money = GetEvent("GetGasNumber").GetGasNumber() / 100;
            }
            else
            {
                Gas = GetEvent("GetGasNumber").GetGasNumber();
            }

            //如果秦港的初始化卡，并且有气量，不能购气
            // if (factoryId == 3 && gas > 0 && cardTypeId == 1) {
            //    return "{isValid:false,tishi:'不是购气卡,不能购气!'}";
            //}
            //错误卡，气量为0
            if (!GetEvent("CheckCard").CheckCard())
            {
                Gas = 0;
            }
            BuyTimes = GetEvent("GetBuyGasTimes").GetBuyGasTimes();
            var buydate = GetEvent("GetDate").GetDate();
            BuyDate = buydate + "";
        }

        #endregion

        #region 售气

        //当前单价
        public double PriceNow { get; set; }
        //是否更改单价，只用于无线表 ，默认0
        private int princemodify = 0;
        public int PriceModify 
        {
            get { return princemodify; }
            set { princemodify = value; }
        }
        /// <summary>
        /// 新单价
        /// </summary>
        public double PriceNew { get; set; }
        /// <summary>
        /// 单价生效日期
        /// </summary>
        public string PriceDate { get; set; }
        /// <summary>
        /// 售气时间
        /// </summary>
        public string SellDate { get; set; }
        //统一购气函数   watchcode, money, pricenow, pricemodify, pricenew, pricedate, gas, buytimes,selldate
        public void SellGas() {
            //alert("watchcode:" + watchcode + ",money:" + money + ",pricenow:" + pricenow + ",pricemodify:" + pricemodify + ",pricenew:" + pricenew + ",pricedate:" + pricedate + ",gas" + gas
           //+ ",buytimes:" + buytimes);
            //9 工业cpu
            if (FactoryId == 9)
            {
                NewRemnant = 1000;
                FirstAlarmV = 1000;
                SecondAlamV = 2000;
                CpuSellGas();
            }
            else
            {
                CSellGas();
            }
        }

        //cpu卡售气
        public void CpuSellGas() {
            SellDate = SellDate.Replace("-", "");
            SellDate = SellDate.Replace(":", "");
            SellDate = SellDate.Replace(" ", "");
            var openRen = GetCpuEvent("OpenPort").OpenPort(0);
            if (!openRen) {
                GetEvent("Close").Close();
                Error = "打开串口失败!";
            }
            var cardAffRen = GetCpuEvent("CardAffim").CardAffim();
            if (!cardAffRen) {
                GetCpuEvent("ClosePort").ClosePort();
                Error = "认证错误!";
            }
            cpuobj.NewRemnant = NewRemnant;
            cpuobj.FirstAlarmV = FirstAlarmV;
            cpuobj.SecondAlamV = SecondAlamV;
            cpuobj.SellGastime = SellDate;
            var alarmRen = GetCpuEvent("SetAlarmVol").SetAlarmVol();
            //设置告警气量错误
            if (!alarmRen) {
                GetCpuEvent("ClosePort").ClosePort();
                Error = "设置告警气量错误!";
            }
            var sellRen = GetCpuEvent("SellGass").SellGass(Gas);
            GetCpuEvent("ClosePort").ClosePort();
            if (sellRen) {
                State = State.End;
            }
            else {
                State = State.Error;
                Error = "售气不成功!";
            }
            
        }

        //购气 购气用cardnew控件
        public void CSellGas() {
            if (PriceDate != null)
            {
                PriceDate = PriceDate.Substring(0, 10);
                PriceDate = PriceDate.Replace("-", "");
            }
            if (SellDate != null)
            {
                SellDate = SellDate.Substring(0, 10);
                SellDate = SellDate.Replace("-", "");
            }
            //购气次数加1
            BuyTimes++;
            var openRen = GetSellEvent("Open").Open(0);
            if (openRen != 0) {
                GetSellEvent("Close").Close();
                Error = "打开串口失败!";
                MessageBox.Show(Error);
                return;
            }

            var pricenow = PriceNow * 100;
            var pricenew = PriceNew * 100;
            var money = Money * 100;
            var pricemodify = 1;
            //如果是无线表，需要先设置数据
            if (FactoryId == 13 || FactoryId == 14 || FactoryId == 15 || FactoryId == 16)
            {
                var r = GetSellEvent("SaveWxPara").SaveWxPara(money, pricenow, pricemodify, pricenew, PriceDate);
            }
            //售气成功返回true,失败false;
            var sellRen = GetSellEvent("SellGas").SellGas(Gas, BuyTimes, SellDate);
            GetSellEvent("Close").Close();
            if (sellRen) {
                State = State.End;
                OnCompleted(null);
            }
            else {
                State = State.Error;
                Error = "售气不成功!";
                MessageBox.Show(Error);
            }
        }
        #endregion

         //发初始化卡，大厅补卡使用
        public string UserClass{get;set;}
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
        public int Amount{get;set;}
        public int Alarm{get;set;}
        public int Constant{get;set;}
        public void ReInitCard() {
            UserType = 0;
            SecondAlamV = 2000;
            NewRemnant=1000;
            int consumerlevel = 11;
            Xs = 1;
           // alert("userClass:" + userClass + ",cardid:" + cardid + ",alarm:" + alarm + ",constant:" + constant + ",amount:" + amount + ",metertype:" + metertype + ",usertype:" + usertype + ",money" + money
           // + ",pricenow:" + pricenow + ",pricemodify:" + pricemodify + ",pricenew:" + pricenew + ",pricedate:" + pricedate);
         
            if (UserClass == "民用") {
                MYInitCard(CardId, Alarm, Constant, Amount, FactoryId, UserType, Money, PriceNow, PriceModify, PriceNew, PriceDate, SellDate);
            }
            else {
                GYInitCard(CardId, Alarm, Constant, Amount, FactoryId, UserType, Money, PriceNow, PriceModify, PriceNew, PriceDate, SecondAlamV, NewRemnant, consumerlevel, Xs, SellDate);
            }
        }

        //发卡
        /* 民用发卡
        cardid:卡号
        alarm:告警气量
        constant:表常数
        amount:气量
        metertype:表型
        user_type:用户类型
        money:金额
        pricenow:当前单价
        */
        public bool MYInitCard(string cardid,int alarm,int constant,int amount,
            int metertype,int user_type,double money,double pricenow,int pricemodify,
            double pricenew, string pricedate, string selldate)
        {
                //日期转换格式:yyyymmdd
                if (PriceDate != null)
                {
                    PriceDate = PriceDate.Substring(0, 10);
                    PriceDate = PriceDate.Replace("-", "");
                }
                if (SellDate != null)
                {
                    SellDate = SellDate.Substring(0, 10);
                    SellDate = SellDate.Replace("-", "");
                }
                //打开串口，检查卡
                var openRen = GetEvent("Open").Open(0);
                if (openRen != 0) {
                    GetEvent("Close").Close();
                    OpenErrorTip(openRen);
                }
                //发卡，如果是无线表，需要先设置参数
                if (metertype == 13 || metertype == 14 || metertype == 15 || metertype == 16)
                {
                    GetEvent("SaveWxPara").SaveWxPara(money, pricenow, pricemodify, pricenew, pricedate);
                }
                //发卡
                var initRen = GetEvent("Init").Init(cardid, alarm, metertype, constant, amount, user_type);
                GetEvent("Close").Close();
                if (initRen) {
                    State = State.End;
                    return true;
                }
                else {
                    State = State.Error;
                    Error = "发卡不成功!'";
                    return false;
                }
        }

        /*工业发初始化
        subunit_id：单位id
        metertype:表型 ，整型
        price: 单价 整型
        */
        public void GYInitCard(string cardid,int alarm,int constant,int amount,int metertype,int usertype,double money,
            double pricenow,int pricemodify,double pricenew,string pricedate,int secondalarm,int newremnant,int consumerlevel,int xs,string selldate) {
            //工业无线表
                if (metertype == 14 || metertype == 16)
                {
                //日期转换格式:yyyymmdd
                    pricedate = pricedate.Substring(0, 10);
                    pricedate = pricedate.Replace("-", "");
                //截取卡号，后8为当做实际卡号
                    var cardlasteight = cardid.Substring(2, 10);
                    var cardfirsttwo = Int32.Parse(cardid.Substring(0, 2));
                    pricenow = pricenow * 100;
                    money = money * 100;
                //调用民用发卡,告警气量0，用户类型0，修改单价标记 1
                var wxRen = MYInitCard(cardlasteight, 0, cardfirsttwo, amount, metertype, 0, money, pricenow, 1, pricenow, pricedate, selldate);
                if (wxRen) {
                    State = State.End;
                }
                else {
                    State = State.Error;
                    Error = "发卡不成功!";
                }
            }
            //cpu卡
            else {
                CpuInit(cardid, alarm, secondalarm, newremnant, consumerlevel, usertype, pricedate, xs, amount, selldate);
            }
        }

        //cpu卡发初始化卡
        public void CpuInit(string cardid,int firstalarm,int secondalarm,int newremnant,int consumerlevel,int usertype,string pricedate,int xs,int gas,string selldate) {
            //日期转换格式:yyyymmdd
            selldate = selldate.Substring(0, 10);
            selldate = selldate.Replace("-", "");
            selldate = selldate.Replace("-", "");
            selldate = selldate.Replace(" ", "");
         
            //alert(selldate);
            //打开串口
            var openRen = GetCpuEvent("OpenPort").OpenPort(0);
            if (!openRen) {
                GetCpuEvent("ClosePort").ClosePort();
                Error = "打开串口错误!";
            }
            //卡认证
            var cardaffim = GetCpuEvent("CardAffim").CardAffim();
            if (!cardaffim) {
                GetCpuEvent("ClosePort").ClosePort();
                Error = "卡认证失败!";
            }
            //清 EF3文件
            GetCpuEvent("ClearEf3").ClearEf3();
            //卡号
            cpuobj.Cardnumber = cardid;
            //第一次告警气量
            cpuobj.FirstAlarmV = firstalarm;
            //第二次告警气量
            cpuobj.SecondAlamV = secondalarm;
            //新上限剩余气量
            cpuobj.NewRemnant = newremnant;
            //用户级别
            cpuobj.consumerlevel = consumerlevel;
            //用户类别
            cpuobj.consumertypes =usertype;
            //初始化时间
            cpuobj.InitiTime = selldate;
            //优惠系数
            cpuobj.xs =xs;
            //初始化写卡
            var initRen = GetCpuEvent("InitiaCard").InitiaCard();
            if (!initRen) {
                GetCpuEvent("ClosePort").ClosePort();
                Error="初始化卡失败!";
            }
            var alaramRen = GetCpuEvent("SetAlarmVol").SetAlarmVol();
            if (!alaramRen) {
                GetCpuEvent("ClosePort").ClosePort();
                Error="初始化报警气量失败!";
            }
            //售气
            cpuobj.SellGastime = selldate;
            gas  =gas/xs;
            var sellRen = GetCpuEvent("SellGass").SellGass(gas);   //初始购气
            if(!sellRen)
            {
                GetCpuEvent("ClosePort").ClosePort();
                Error="初始购气不成功!";
            }
            GetCpuEvent("ClosePort").ClosePort();
        }

        /// <summary>
        /// 撤销:cardId, cardgas, cardmoney,selldate
        /// </summary>
        /// <param name="?"></param>
        /// <param name="?"></param>
        /// <param name="?"></param>
        /// <param name="?"></param>
        public void ReturnGas() {
            ICCardObj ic = new ICCardObj();
            ic.ReadCard();
            if (CardId != ic.CardId || Gas != ic.Gas || Money != ic.Money) {
                GetSellEvent("Close").Close();
                Error = "读卡信息与撤销卡信息不匹配!";
                MessageBox.Show(Error);
                return;
            }
            var openRen = GetEvent("Open").Open(0);
            if (openRen == 0) {
                GetEvent("Close").Close();
                GetSellEvent("Open").Open(0);
                if (GetSellEvent("DelGas").DelGas())
                {
                    GetSellEvent("Close").Close();
                    return;
                }
                else {
                    GetSellEvent("Close").Close();
                    Error = "撤销不成功!";
                    MessageBox.Show(Error);
                    return;
                }
            }
            else if (openRen == 1) {
                GetEvent("Close").Close();
                Error = "卡没有插入!";
                MessageBox.Show(Error);
                return;
            }
            else if (openRen == 2) {
                GetEvent("Close").Close();
                Error = "硬件连接错误：线没连接或读卡器有问题或计算机串口坏!";
                MessageBox.Show(Error);
                return;
            }
            else if (openRen == 3) {
                GetEvent("Close").Close();
                Error = "密码错误!";
                MessageBox.Show(Error);
                return;
            }
            else if (openRen == 4)
            {
                GetEvent("Close").Close();
                // CpuDelGas();
            }
            else
            {
                Error = "撤销不成功!";
                MessageBox.Show(Error);
                return;
            }
            State = State.End;
        }

        //补卡  card_id, meter_type, times, metre_con, warn_gas, pricenow, pricemodify, pricenew, pricedate, selldate
        public void MakeUp() {
            //如果是化解，不允许补购气卡
            if (FactoryId == 1 || FactoryId == 2 || FactoryId == 11 || FactoryId == 12)
            {
                Error = "华捷不允许补购气卡!";
                MessageBox.Show(Error);
                return;
            }

            var cpuselldate = SellDate;
            var cpupricedate = PriceDate;
            if (PriceDate != null)
            {
                PriceDate = PriceDate.Substring(0, 10);
                PriceDate = PriceDate.Replace("-", "");
            }
            if (SellDate != null)
            {
                SellDate = SellDate.Substring(0, 10);
                SellDate = SellDate.Replace("-", "");
            }

          //  alert("card_id:" + card_id + ",meter_type:" + meter_type + ",times:" + times +",metre_con:" + metre_con + ",warn_gas:" + warn_gas + ",pricenow" + pricenow
          //  + ",pricemodify:" + pricemodify + ",pricenew:" + pricenew + ",pricedate:" + pricedate);
            
            var gas = 0;
            var cus_type = 0;
            var money = 0;
            var openRen = GetEvent("Open").Open(0);
            if (openRen == 0) {
                GetEvent("Close").Close();
                GetSellEvent("Open").Open(0);
                bool cardren;
                if (FactoryId == 13 || FactoryId == 14 || FactoryId == 15 || FactoryId == 16)
                {
                    GetSellEvent("SaveWxPara").SaveWxPara(money, PriceNow, PriceModify, PriceNew, PriceDate);
                }
                if (FactoryId == 14 || FactoryId == 16)
                {
                    var firstCard = CardId.Substring(0, 2);
                    CardId = CardId.Substring(2, 10);
                    cardren = GetSellEvent("SaveOldCard").SaveOldCard(CardId, FactoryId, gas, BuyTimes, cus_type, firstCard, Alarm, firstCard, 0, 0);
                }
                else {
                    cardren = GetSellEvent("SaveOldCard").SaveOldCard(CardId, FactoryId, gas, BuyTimes, cus_type, MetreCon, Alarm, MetreCon, 0, 0);
                }
                GetSellEvent("Close").Close();
                if (cardren) {
                    State = State.End;
                    OnCompleted(null);
                }
                else {
                    State = State.Error;
                    Error = "补卡不成功!'";
                    MessageBox.Show(Error);
                }
                
            }
            else if (openRen == 1) {
                GetEvent("Close").Close();
                Error= "卡没有插入!";
                MessageBox.Show(Error);
               
            }
            else if (openRen == 2) {
                GetEvent("Close").Close();
                Error = "硬件连接错误：线没连接或读卡器有问题或计算机串口坏!";
                MessageBox.Show(Error);
            }
            else if (openRen == 3) {
                GetEvent("Close").Close();
                Error = "密码错误!";
                MessageBox.Show(Error);
            }
            //补cpu卡
            else if (openRen == 4) {
                var NewRemnant = 1000;
                var FirstAlarmV = 1000;
                var SecondAlamV = 2000;
                var xs = 1;
                var consumerlevel = 11;
                GetEvent("Close").Close();
                //MakeUpCpu(card_id, FirstAlarmV, SecondAlamV, NewRemnant, consumerlevel, cus_type, pricedate, xs, cpupricedate, times, money, cpuselldate);
            }
        }


        //打开串口错误提示
        private void OpenErrorTip(int val) {
            if (val == 1) {
                Error = "卡没有插入!'";
             }
            else if (val == 2) {
                Error = "硬件错误!'";
            }
            else if (val == 3) {
                Error = "密码错误!'";
            }
            else if (val == 4) {
                Error = "卡已经损坏!'";
            }
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
        /// 获得cpu控件方法对象
        /// </summary>
        /// <param name="EventName">方法名</param>
        /// <returns></returns>
        public dynamic GetCpuEvent(string EventName)
        {
            AutomationFactory.GetEvent(cpuobj, EventName);
            return obj;
        }
        /// <summary>
        /// 获得售气控件方法对象
        /// </summary>
        /// <param name="EventName"></param>
        /// <returns></returns>
        public dynamic GetSellEvent(string EventName)
        {
            AutomationFactory.GetEvent(sellobj, EventName);
            return obj;
        }


        /// <summary>
        /// 卡号自动补零
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public string AddZero(string Source,int len)
        {
            var strTemp = "";
            Source = Source + "";
            for (var i = 1; i <= len - Source.Length; i++)
            {
                strTemp += "0";
            }
           return strTemp+Source; 
        }
         /// <summary>
        /// 卡类型转换
         /// </summary>
         /// <param name="cardtype"></param>
         /// <returns></returns>
        public string CardTypeConver(int cardtype)
        {
           if(cardtype==0)
           {
             return "用户卡";
           }
           else if(cardtype == 1)
           {
             return "初始化卡";
           }
           else if(cardtype == 2)
           {
             return "清零卡";
           }
           else if(cardtype == 3)
           {
             return "数据采集卡";
           }
           else if(cardtype == 4)
           {
             return "数据恢复卡";
           }
           else if(cardtype == 5)
           {
             return "数据检测卡";
           }
           else if(cardtype==6)
           {
             return "购气卡";
           }
           else if(cardtype==7)
           {
             return "超级卡";
           }
           else if(cardtype==8)
           {
             return "阀门测试卡";
           }
           else if(cardtype==9)
           {
             return "过流设置卡";
           }
           else if(cardtype == 10)
           {
             return "卡号设置卡";
           }
           else if(cardtype == 11)
           {
            return "表号设置卡";
           }
           else if(cardtype == 13)
           {
             return "新卡";
           }
           else{
             return "未知卡";
           }
             
        }
        /// <summary>
        /// 表类型转换
        /// </summary>
        /// <param name="factoryid"></param>
        /// <returns></returns>
        private string FactoryTypeConver(long factoryid)
        {
            if (factoryid == 0 || factoryid == 1 || factoryid == 2) {
                //再判断是否是化解加密
                AutomationFactory.GetEvent(obj, "GetCardType");
                var huajie = obj.GetCardType();
                if (huajie == 4) {
                    return "华捷普通";
                }
                return "华捷加密";
            }
            else if (factoryid == 3) {
                return "秦港";
            }
            else if (factoryid == 4) {
                return "秦川";
            }
            else if (factoryid == 5) {
                return "天庆";
            }
            else if (factoryid == 6) {
                return "致力老表";
            }
            else if (factoryid == 7) {
                return "致力新表";
            }
            else if (factoryid == 8) {
                return "赛福";
            }
            else if (factoryid == 13) {
                return "天然气无线";
            }
            else if (factoryid == 14) {
                return "工业无线";
            }
            else if (factoryid == 15) {
                return "秦港民用无线";
            }
            else if (factoryid == 16) {
                return "秦刚工业无线";
            }
            else if (factoryid == -1) {
                return "新卡";
            }
            else {
                return "未知卡";
            }
        }

        public string Name { get; set; }
        public string Error { get; set; }
        #region 属性

        public string Factory { get; set; }
        public string CardType { get; set; }
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
        public int BuyTimes { get; set; }
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


        public State State
        {
            get;
            set;
        }

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

        public bool IsBusy
        {
            get;
            set;
        }
    }
}

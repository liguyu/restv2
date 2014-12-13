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
using System.Runtime.InteropServices.Automation;
using System.ComponentModel;
using System.Threading;

namespace Com.Aote.ObjectTools
{
    //通用写卡对象
    public class NewGeneralICCard : CustomTypeHelper, IAsyncObject
    {
        #region CardId 卡号
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
        #endregion

        #region Gas 卡内气量
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
        #endregion

        #region BuyTimes 购气次数
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
        #endregion

        #region Factory 表厂
        //表厂
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
        #endregion

        #region Klx 卡类型
        //卡类型
        public int klx;
        public int Klx
        {
            get { return klx; }
            set
            {
                this.klx = value;
                OnPropertyChanged("Klx");
            }
        }
        #endregion

        #region Kzt 卡状态
        //卡状态
        public int kzt;
        public int Kzt
        {
            get { return kzt; }
            set
            {
                this.kzt = value;
                OnPropertyChanged("Kzt");
            }
        }
        #endregion

        #region Dqdm 地区代码
        //地区代码
        public string dqdm;
        public string Dqdm
        {
            get { return dqdm; }
            set
            {
                this.dqdm = value;
                OnPropertyChanged("Dqdm");
            }
        }
        #endregion

        #region Yhh 用户号
        //用户号
        public string yhh;
        public string Yhh
        {
            get { return yhh; }
            set
            {
                this.yhh = value;
                OnPropertyChanged("Yhh");
            }
        }
        #endregion

        #region Tm 表条码
        //表条码
        public string tm;
        public string Tm
        {
            get { return tm; }
            set
            {
                this.tm = value;
                OnPropertyChanged("Tm");
            }
        }
        #endregion

        #region Ljgql 累计购气量
        //累计购气量
        public double ljgql;
        public double Ljgql
        {
            get { return ljgql; }
            set
            {
                this.ljgql = value;
                OnPropertyChanged("Ljgql");
            }
        }
        #endregion

        #region Bkcs 补卡次数
        //补卡次数
        public int bkcs;
        public int Bkcs
        {
            get { return bkcs; }
            set
            {
                this.bkcs = value;
                OnPropertyChanged("Bkcs");
            }
        }
        #endregion

        #region Ljyql 累计用气量
        //累计用气量
        public double ljyql;
        public double Ljyql
        {
            get { return ljyql; }
            set
            {
                this.ljyql = value;
                OnPropertyChanged("Ljyql");
            }
        }
        #endregion

        #region Syql 剩余气量
        //剩余气量
        public double syql;
        public double Syql
        {
            get { return syql; }
            set
            {
                this.syql = value;
                OnPropertyChanged("Syql");
            }
        }
        #endregion

        #region Bjql 报警气量
        //报警气量
        public int bjql;
        public int Bjql
        {
            get { return bjql; }
            set
            {
                this.bjql = value;
                OnPropertyChanged("Bjql");
            }
        }
        #endregion

        #region Czsx 充值上限
        //充值上限
        public int czsx;
        public int Czsx
        {
            get { return czsx; }
            set
            {
                this.czsx = value;
                OnPropertyChanged("Czsx");
            }
        }
        #endregion

        #region Tzed 透支额度
        //透支额度
        public int tzed;
        public int Tzed
        {
            get { return tzed; }
            set
            {
                this.tzed = value;
                OnPropertyChanged("Tzed");
            }
        }
        #endregion

        #region Kmm 卡密码
        //卡密码
        public string kmm;
        public string Kmm
        {
            get { return kmm; }
            set
            {
                this.kmm = value;
                OnPropertyChanged("Kmm");
            }
        }


        #endregion

        #region Sqrq 售气日期
        //售气日期 
        public string sqrq;
        public string Sqrq
        {
            get { return sqrq; }
            set
            {
                this.sqrq = value;
                OnPropertyChanged("Sqrq");
            }
        }
        #endregion

        #region OldPrice 现在单价
        //现在单价
        public double oldprice;
        public double OldPrice
        {
            get { return oldprice; }
            set
            {
                this.oldprice = value;
                OnPropertyChanged("OldPrice");
            }
        }
        #endregion

        #region NewPrice 新单价
        //新单价
        public double newprice;
        public double NewPrice
        {
            get { return newprice; }
            set
            {
                this.newprice = value;
                OnPropertyChanged("NewPrice");
            }
        }
        #endregion

        #region Sxrq 单价生效日期
        //单价生效日期 
        public string sxrq;
        public string Sxrq
        {
            get { return sxrq; }
            set
            {
                this.sxrq = value;
                OnPropertyChanged("Sxrq");
            }
        }
        #endregion

        #region Sxbj 单价生效标记
        //单价生效标记 
        public string sxbj;
        public string Sxbj
        {
            get { return sxbj; }
            set
            {
                this.sxbj = value;
                OnPropertyChanged("Sxbj");
            }
        }
        #endregion

        //读卡控件
        dynamic obj;
        private bool init = false;
        public bool Init
        {
            get { return init; }
            set
            {
                this.init = value;
                if (init)
                {
                    this.InitCardObj();

                }
            }
        }

        private void InitCardObj()
        {
            if (obj == null)
            {
                obj = AutomationFactory.CreateObject("HJIC.HJICCtrl.1");
            }
        }

        #region CanInitCard 是否可发初始化卡
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

        #region CanSellGas 是否可以售气
        public static readonly DependencyProperty CanSellGasProperty =
            DependencyProperty.Register("CanSellGas", typeof(bool), typeof(NewGeneralICCard),
            new PropertyMetadata(new PropertyChangedCallback(OnCanSellGasChanged)));
        public bool CanSellGas
        {
            get { return (bool)GetValue(CanSellGasProperty); }
            set { SetValue(CanSellGasProperty, value); }
        }

        //如果可以售气，调用售气过程
        private static void OnCanSellGasChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            NewGeneralICCard card = (NewGeneralICCard)dp;
            if (card.CanSellGas)
            {
                card.SellGas();
            }
        }
        #endregion

        #region CanRewriteCard 是否可以擦卡后写初始化卡
        public static readonly DependencyProperty CanRewriteCardProperty =
            DependencyProperty.Register("CanRewriteCard", typeof(bool), typeof(NewGeneralICCard),
            new PropertyMetadata(new PropertyChangedCallback(OnCanRewriteCardChanged)));
        public bool CanRewriteCard
        {
            get { return (bool)GetValue(CanRewriteCardProperty); }
            set { SetValue(CanRewriteCardProperty, value); }
        }

        private static void OnCanRewriteCardChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            NewGeneralICCard card = (NewGeneralICCard)dp;
            if (card.CanRewriteCard)
            {
                card.ReWriteCard();
            }
        }
        #endregion

        public NewGeneralICCard()
        {
        }

        #region ReadCard 读卡
        /// <summary>
        /// 读卡
        /// </summary>
        public void ReadCard()
        {
            IsBusy = true;
            Error = "";
            State = State.StartLoad;
            //卡上信息初始化
            CardId = "";
            Factory = "";
            Gas = 0;
            BuyTimes = 0;
            Klx = -1;
            Kzt = -1;
            Dqdm = "";
            Yhh = "";
            Tm = "";
            Ljgql = 0;
            Bkcs = 0;
            Ljyql = 0;
            Syql = 0;
            Bjql = 0;
            Czsx = 0;
            Tzed = 0;
            Sqrq = "";
            OldPrice = 0;
            NewPrice = 0;
           // Sxrq = "";
            Sxbj = "";
            //读卡
            Thread thread = new Thread(() =>
            {
                dynamic ocx = AutomationFactory.CreateObject("HJIC.HJICCtrl.1");
                AutomationFactory.GetEvent(ocx, "ReadGasCard");
                int openRen = ocx.ReadGasCard();
                //IsBusy=false;
                if (this.Dispatcher.CheckAccess())
                {
                    IsBusy = false;
                }
                else
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        IsBusy = false;
                    });
                }
                if (openRen == 0)
                {
                    //获取卡上内容
                    AutomationFactory.GetEvent(ocx, "GetCardId");
                    string cardId = ocx.GetCardId();
                    AutomationFactory.GetEvent(ocx, "GetFactory");
                    string factory = ocx.GetFactory();
                    AutomationFactory.GetEvent(ocx, "GetGas");
                    double gas = ocx.GetGas();
                    AutomationFactory.GetEvent(ocx, "GetTimes");
                    int buyTimes = ocx.GetTimes();
                    AutomationFactory.GetEvent(ocx, "GetKlx");
                    int klx = ocx.GetKlx();
                    AutomationFactory.GetEvent(ocx, "GetKzt");
                    int kzt = ocx.GetKzt();
                    AutomationFactory.GetEvent(ocx, "GetDqdm");
                    string dqdm = ocx.GetDqdm();
                    AutomationFactory.GetEvent(ocx, "GetYhh");
                    string yhh = ocx.GetYhh();
                    AutomationFactory.GetEvent(ocx, "GetTm");
                    string tm = ocx.GetTm();
                    AutomationFactory.GetEvent(ocx, "GetLjgql");
                    double ljgql = ocx.GetLjgql();
                    AutomationFactory.GetEvent(ocx, "GetBkcs");
                    int bkcs = ocx.GetBkcs();
                    AutomationFactory.GetEvent(ocx, "GetLjyql");
                    double ljyql = ocx.GetLjyql();
                    AutomationFactory.GetEvent(ocx, "GetSyql");
                    double syql = ocx.GetSyql();
                    AutomationFactory.GetEvent(ocx, "GetBjql");
                    int bjql = ocx.GetBjql();
                    AutomationFactory.GetEvent(ocx, "GetCzsx");
                    int czsx = ocx.GetCzsx();
                    AutomationFactory.GetEvent(ocx, "GetTzed");
                    int tzed = ocx.GetTzed();
                    AutomationFactory.GetEvent(ocx, "GetSqrq");
                    string sqrq = ocx.GetSqrq();
                    AutomationFactory.GetEvent(ocx, "GetOldPrice");
                    int oldprice = ocx.GetOldPrice();
                    AutomationFactory.GetEvent(ocx, "GetNewPrice");
                    int newprice = ocx.GetNewPrice();
                    AutomationFactory.GetEvent(ocx, "GetSxrq");
                    string sxrq = ocx.GetSxrq();
                    AutomationFactory.GetEvent(ocx, "GetSxbj");
                    string sxbj = ocx.GetSxbj();
                    //直接赋值
                    if (this.Dispatcher.CheckAccess())
                    {
                        CardId = cardId;
                        Factory = factory;
                        Gas = gas;
                        BuyTimes = buyTimes;
                        Klx = klx;
                        Kzt = kzt;
                        Dqdm = dqdm;
                        Yhh = yhh;
                        Tm = tm;
                        Ljgql = ljgql;
                        Bkcs = bkcs;
                        Ljyql = ljyql;
                        Syql = syql;
                        Bjql = bjql;
                        Czsx = czsx;
                        Tzed = tzed;
                        Sqrq = sqrq;
                        OldPrice = oldprice;
                        NewPrice = newprice;
                        //Sxrq = sxrq;
                        Sxbj = sxbj;
                        State = State.Loaded;
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            CardId = cardId;
                            Factory = factory;
                            Gas = gas;
                            BuyTimes = buyTimes;
                            Klx = klx;
                            Kzt = kzt;
                            Dqdm = dqdm;
                            Yhh = yhh;
                            Tm = tm;
                            Ljgql = ljgql;
                            Bkcs = bkcs;
                            Ljyql = ljyql;
                            Syql = syql;
                            Bjql = bjql;
                            Czsx = czsx;
                            Tzed = tzed;
                            Sqrq = sqrq;
                            OldPrice = oldprice;
                            NewPrice = newprice;
                           // Sxrq = sxrq;
                            Sxbj = sxbj;
                            State = State.Loaded;
                        });
                    }
                }
                else
                {
                    if (this.Dispatcher.CheckAccess())
                    {
                        Error = "读卡错误!错误代码" + openRen;
                        State = State.LoadError;
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            Error = "读卡错误!错误代码" + openRen;
                            State = State.LoadError;
                        });
                    }
                }
                if (this.Dispatcher.CheckAccess())
                {
                    OnReadCompleted(null);
                }
                else
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        OnReadCompleted(null);
                    });
                }
            });
            thread.Start();
        }
        #endregion

        #region SellGas 售气 ,退气
        //气量大于0代表购气，气量等于0代表退气
        public void SellGas()
        {
            IsBusy = true;
            Error = "";
            State = State.Start;
            //执行写卡线程
            Thread thread = new Thread(() =>
            {
                dynamic ocx = AutomationFactory.CreateObject("HJIC.HJICCtrl.1");
                AutomationFactory.GetEvent(ocx, "WriteGasCard");
                int sellRen = ocx.WriteGasCard(Factory, Kmm, Klx, CardId, Dqdm, Gas, BuyTimes, Ljgql, Bjql, Czsx, Tzed, Sqrq, OldPrice, NewPrice, Sxrq, Sxbj);
                //IsBusy=false;
                AutomationFactory.GetEvent(ocx, "GetKmm");
                kmm = ocx.GetKmm();
                if (this.Dispatcher.CheckAccess())
                {
                    IsBusy = false;
                }
                else
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        IsBusy = false;
                    });
                }
                //售气成功返回true,失败false;
                if (sellRen == 0)
                {
                    if (this.Dispatcher.CheckAccess())
                    {
                        State = State.End;
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            State = State.End;
                        });
                    }
                }
                else
                {
                    if (this.Dispatcher.CheckAccess())
                    {
                        Error = "售气不成功!错误代码" + sellRen;
                        State = State.Error;
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            Error = "售气不成功!错误代码" + sellRen;
                            State = State.Error;
                        });
                    }
                }
                if (this.Dispatcher.CheckAccess())
                {
                    OnCompleted(null);
                }
                else
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        OnCompleted(null);
                    });
                }
            });
            thread.Start();
        }
        #endregion

        #region ReInitCard 发初始化卡，或补卡
        //发初始化卡,或补卡 0：开户卡状态，1：用户卡状态。 卡状态
        public void ReInitCard()
        {
            IsBusy = true;
            Error = "";
            State = State.Start;
            State = State.End;
            return;
            //执行写卡线程
            Thread thread = new Thread(() =>
            {
                dynamic ocx = AutomationFactory.CreateObject("HJIC.HJICCtrl.1");
                AutomationFactory.GetEvent(ocx, "WriteGasCard");
              //  int sellRen = ocx.WriteNewCard(Factory, Kmm, Klx, Kzt, CardId, Dqdm, Yhh, Tm, Gas, BuyTimes, Ljgql, Bkcs, Bjql, Czsx, Tzed, Sqrq, OldPrice, NewPrice, Sxrq, Sxbj);
                int sellRen = ocx.WriteNewCard(Factory, Kmm, Klx, Kzt, CardId, Dqdm, Yhh, Tm, Gas, BuyTimes, Ljgql, Bkcs, Bjql, Czsx, Tzed, Sqrq, OldPrice, NewPrice, Sxrq, Sxbj);
                //IsBusy=false;
                AutomationFactory.GetEvent(ocx, "GetKmm");
                kmm = ocx.GetKmm();
                if (this.Dispatcher.CheckAccess())
                {
                    IsBusy = false;
                }
                else
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        IsBusy = false;
                    });
                }
                //售气成功返回true,失败false;
                if (sellRen == 0)
                {
                    if (this.Dispatcher.CheckAccess())
                    {

                        State = State.End;
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            State = State.End;
                        });
                    }
                }
                else
                {
                    if (this.Dispatcher.CheckAccess())
                    {
                        Error = "发卡不成功!错误代码" + sellRen;
                        State = State.Error;
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            Error = "发卡不成功!错误代码" + sellRen;
                            State = State.Error;
                        });
                    }
                }
                if (this.Dispatcher.CheckAccess())
                {
                    OnCompleted(null);
                }
                else
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        OnCompleted(null);
                    });
                }
            });
            thread.Start();
        }
        #endregion






        #region ReWriteCard 重新写卡，先清卡，然后再初始化卡
        //格式化卡
        public void ReWriteCard()
        {
            IsBusy = true;
            Error = "";
            State = State.Start;
            //执行写卡线程
            Thread thread = new Thread(() =>
            {
                dynamic ocx = AutomationFactory.CreateObject("HJIC.HJICCtrl.1");
                AutomationFactory.GetEvent(ocx, "ReadGasCard");
                int openRen = ocx.ReadGasCard();
                //读卡失败
                if (openRen != 0)
                {
                    if (this.Dispatcher.CheckAccess())
                    {
                        Error = "擦卡不成功!错误代码" + openRen;
                        State = State.Error;
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            Error = "擦卡不成功!错误代码" + openRen;
                            State = State.Error;
                        });
                    }
                }
                else
                {
                    //获取擦卡所需数据
                    AutomationFactory.GetEvent(ocx, "GetCardId");
                    string _cardId = ocx.GetCardId();
                    AutomationFactory.GetEvent(ocx, "GetFactory");
                    string _factory = ocx.GetFactory();
                    AutomationFactory.GetEvent(ocx, "GetKlx");
                    int _klx = ocx.GetKlx();
                    AutomationFactory.GetEvent(ocx, "GetDqdm");
                    string _dqdm = ocx.GetDqdm();
                    //擦卡
                    AutomationFactory.GetEvent(ocx, "FormatGasCard");
                    int sellRen = ocx.FormatGasCard(_factory, Kmm, _klx, _cardId, _dqdm);
                    //擦卡后，写初始化卡
                    if (sellRen == 0)
                    {
                        //写初始化卡
                        AutomationFactory.GetEvent(ocx, "WriteGasCard");
                        sellRen = ocx.WriteNewCard(Factory, Kmm, Klx, Kzt, CardId, Dqdm, Yhh, Tm, Gas, BuyTimes, Ljgql, Bkcs, Bjql, Czsx, Tzed, Sqrq, OldPrice, NewPrice, Sxrq, Sxbj);
                        //售气成功返回true,失败false;
                        AutomationFactory.GetEvent(ocx, "GetKmm");
                        kmm = ocx.GetKmm();
                        if (sellRen == 0)
                        {
                            if (this.Dispatcher.CheckAccess())
                            {
                                State = State.End;
                            }
                            else
                            {
                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    State = State.End;
                                });
                            }
                        }
                        else
                        {
                            if (this.Dispatcher.CheckAccess())
                            {
                                Error = "发卡不成功!错误代码" + sellRen;
                                State = State.Error;
                            }
                            else
                            {
                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    Error = "发卡不成功!错误代码" + sellRen;
                                    State = State.Error;
                                });
                            }
                        }
                    }
                    else
                    {
                        if (this.Dispatcher.CheckAccess())
                        {
                            Error = "擦卡不成功!错误代码" + sellRen;
                            State = State.Error;
                        }
                        else
                        {
                            this.Dispatcher.BeginInvoke(() =>
                            {
                                Error = "擦卡不成功!错误代码" + sellRen;
                                State = State.Error;
                            });
                        }
                    }
                }
                //IsBusy=false;
                if (this.Dispatcher.CheckAccess())
                {
                    IsBusy = false;
                }
                else
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        IsBusy = false;
                    });
                }
                if (this.Dispatcher.CheckAccess())
                {
                    OnCompleted(null);
                }
                else
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        OnCompleted(null);
                    });
                }
            });
            thread.Start();
        }
        #endregion

        #region MakeNewCard 格式化卡
        //格式化卡
        public void MakeNewCard()
        {
            IsBusy = true;
            Error = "";
            State = State.Start;
            //执行写卡线程
            Thread thread = new Thread(() =>
            {
                dynamic ocx = AutomationFactory.CreateObject("HJIC.HJICCtrl.1");
                AutomationFactory.GetEvent(ocx, "ReadGasCard");
                int openRen = ocx.ReadGasCard();
                //读卡失败
                if (openRen != 0)
                {
                    if (this.Dispatcher.CheckAccess())
                    {
                        Error = "擦卡不成功!错误代码" + openRen;
                        State = State.Error;
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            Error = "擦卡不成功!错误代码" + openRen;
                            State = State.Error;
                        });
                    }
                }
                else
                {
                    //获取擦卡所需数据
                    AutomationFactory.GetEvent(ocx, "GetCardId");
                    string _cardId = ocx.GetCardId();
                    AutomationFactory.GetEvent(ocx, "GetFactory");
                    string _factory = ocx.GetFactory();
                    AutomationFactory.GetEvent(ocx, "GetKlx");
                    int _klx = ocx.GetKlx();
                    AutomationFactory.GetEvent(ocx, "GetDqdm");
                    string _dqdm = ocx.GetDqdm();
                    //擦卡
                    AutomationFactory.GetEvent(ocx, "FormatGasCard");
                    int sellRen = ocx.FormatGasCard(_factory, Kmm, _klx, _cardId, _dqdm);
                    //售气成功返回true,失败false;
                    if (sellRen == 0)
                    {
                        if (this.Dispatcher.CheckAccess())
                        {
                            State = State.End;
                        }
                        else
                        {
                            this.Dispatcher.BeginInvoke(() =>
                            {
                                State = State.End;
                            });
                        }
                    }
                    else
                    {
                        if (this.Dispatcher.CheckAccess())
                        {
                            Error = "擦卡不成功!错误代码" + sellRen;
                            State = State.Error;
                        }
                        else
                        {
                            this.Dispatcher.BeginInvoke(() =>
                            {
                                Error = "擦卡不成功!错误代码" + sellRen;
                                State = State.Error;
                            });
                        }
                    }
                }
                //IsBusy=false;
                if (this.Dispatcher.CheckAccess())
                {
                    IsBusy = false;
                }
                else
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        IsBusy = false;
                    });
                }
                if (this.Dispatcher.CheckAccess())
                {
                    OnCompleted(null);
                }
                else
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        OnCompleted(null);
                    });
                }
            });
            thread.Start();
        }
        #endregion

        #region HTOpen 航天卡开锁
        //航天卡开锁
        public void HTOpen()
        {
            IsBusy = true;
            Error = "";
            State = State.Start;
            //发卡
            int initRen = GetEvent("HTOpen").HTOpen(Factory);
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
        #endregion

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

        #region State 卡状态
        public State state = State.Free;
        public State State
        {
            get { return state; }
            set
            {
                state = value;
                OnPropertyChanged("State");
            }
        }

        //public static readonly DependencyProperty StateProperty =
        //    DependencyProperty.Register("State", typeof(State), typeof(NewGeneralICCard), null);

        //public State State
        //{
        //    get { return (State)GetValue(StateProperty); }
        //    set
        //    {
        //        SetValue(StateProperty, value);
        //    }
        //}
        #endregion

        #region ReadCompleted 读卡完成事件
        public event System.ComponentModel.AsyncCompletedEventHandler ReadCompleted;
        public void OnReadCompleted(System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (ReadCompleted != null)
            {
                ReadCompleted(this, e);
            }
        }
        #endregion

        #region Completed 写卡完成事件
        public event System.ComponentModel.AsyncCompletedEventHandler Completed;
        public void OnCompleted(System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (Completed != null)
            {
                Completed(this, e);
            }
        }
        #endregion

        #region Writing 写卡前事件，在写卡之前触发
        public event System.ComponentModel.AsyncCompletedEventHandler Writing;
        public void OnWriting(System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (Writing != null)
            {
                Writing(this, e);
            }
        }
        #endregion


        #region IsBusy 是否忙
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
        #endregion
    }
}

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

namespace Com.Aote.Behaviors
{

    //批量转换列表动作
    public class PageAction : BaseAsyncAction
    {
        //转换源
        private PagedObjectList sourceObject;
        public PagedObjectList SourceObject 
        {
            get { return sourceObject; }
            set
            {
                sourceObject = value;
            }
        }

        //动作
        private BatchExcuteAction action;
        public BatchExcuteAction Action 
        {
            get { return action; }
            set
            {
                action = value;
            }
        }
        //转换对象
        //public ObjectList TargetObject { get; set; }

        public override void Invoke()
        {
            State = State.Start;
            IsBusy = true;
            
            //源对象加载完成后
            sourceObject.DataLoaded += sourceObject_DataLoaded;
            Action.Completed += Action_Completed;

            Action.Invoke();
            //加载总数
            //SourceObject.Load();
        }

        void sourceObject_DataLoaded(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (sourceObject.Count != 0)
            {
                //执行动作
                Action.Invoke();
            }
            else
            {
                sourceObject.DataLoaded -= sourceObject_DataLoaded;
                action.Completed -= Action_Completed;
                State = State.End;
                IsBusy = false;
            }
        }

        void Action_Completed(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            SourceObject.Load();
        }


        //转换列表
/*        public void TransSave()
        {
            TargetObject.Clear();
            foreach (GeneralObject item in (IList)SourceObject)
            {
                //模板对象
                if (TargetObject.templetObject == null)
                {
                    throw new Exception("模板对象不能为空!");
                }
                if (TargetObject.TempObj == null)
                {
                    throw new Exception("临时对象不能为空!");
                }
                //将临时对象值赋值为要转换对象
                TargetObject.TempObj.CopyFrom(item);
                //产生新对象
                GeneralObject go = new GeneralObject();
                go.WebClientInfo = TargetObject.templetObject.WebClientInfo;
                go.CopyDataFrom(TargetObject.templetObject);
                //回写档案余额
                huixie(go);
                TargetObject.Add(go);
            }
            //保存
            TargetObject.Save();
        }

        private void huixie(GeneralObject go)
        {
            object o = go.GetPropertyValue("mark");
            if (o != null)
            {
                GeneralObject user = (GeneralObject)go.GetPropertyValue("users");
                user.Name = "user";
                user.WebClientInfo = go.WebClientInfo;
                //Double f_zhye = Double.Parse(user.GetPropertyValue("f_zhye")+"");
               // Double oughtfee = Double.Parse(go.GetPropertyValue("oughtfee") + "");
                object obj = go.GetPropertyValue("f_bczhye");
                //回写新余额
                user.SetPropertyValue("f_zhye", obj, true);
                //当前表累计气量
                Double oughtamount = Double.Parse(go.GetPropertyValue("oughtamount")+"");
                string meter = user.GetPropertyValue("f_metergasnums") + "";
                Double f_metergasnums = 0;
                if (meter != "")
                {
                    f_metergasnums = Double.Parse(meter);
                }
                user.SetPropertyValue("f_metergasnums", oughtamount + f_metergasnums, true);
                //总累计购气量
                string cumul = user.GetPropertyValue("f_cumulativepurchase") + "";
                Double f_cumulativepurchase = 0;
                if (cumul != "")
                {
                    f_cumulativepurchase = Double.Parse(cumul);
                }
                user.SetPropertyValue("f_cumulativepurchase", oughtamount + f_cumulativepurchase, true);
                //最后购气量
                user.SetPropertyValue("f_finallybought", oughtamount, true);
                //最后购气日期
                user.SetPropertyValue("f_finabuygasdate", go.GetPropertyValue("f_deliverydate"), true);
                //上次抄表底数
                user.SetPropertyValue("lastinputgasnum", go.GetPropertyValue("lastrecord"), true);
                //本次抄表底数
                user.SetPropertyValue("lastrecord", go.GetPropertyValue("lastrecord"), true);
                //抄表日期
                user.SetPropertyValue("lastinputdate", go.GetPropertyValue("lastinputdate"), true);
                user.Save();
            }
        }

        //监听
        public void Listen()
        {
            //列表保存完成后
            TargetObject.Completed += (o, e) =>
            {
                //翻页
                int page = SourceObject.Count / SourceObject.PageSize;
                if (SourceObject.Count % SourceObject.PageSize > 0)
                {
                    page++;
                }
                if (page-1 > SourceObject.PageIndex)
                {
                    SourceObject.PageIndex++;
                }
                else
                {
                    IsBusy = false;
                    State = State.End;
                }
            };
        }
*/

        private bool canInvoke;
        public bool CanInvoke
        {
            get { return canInvoke; }
            set
            {
                if (canInvoke != value)
                {
                    canInvoke = value;
                    if (canInvoke)
                    {
                        Invoke();
                    }
                }
            }
        }
    }
}

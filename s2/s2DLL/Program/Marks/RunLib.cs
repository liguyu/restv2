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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Com.Aote.ObjectTools;
using System.Text.RegularExpressions;
using System.Json;
using Com.Aote.Attachs;

namespace Com.Aote.Utils
{
    /// <summary>
    /// Exp标记系统运行时所需库函数，比如Sum，Compare等。
    /// </summary>
    public static class RunLib
    {
        #region 数字类型全部转换成decimal，包括object，以及decimal本身，主要用于加、减、乘、除等数学运算
        public static decimal Convert(int a)
        {
            decimal result = new decimal(a);
            return result;
        }
        public static decimal Convert(int? a)
        {
            decimal result = (a == null ? 0 : new decimal((int)a));
            return result;
        }
        public static decimal Convert(long a)
        {
            decimal result = new decimal(a);
            return result;
        }
        public static decimal Convert(long? a)
        {
            decimal result = (a == null ? 0 : new decimal((long)a));
            return result;
        }
        public static decimal Convert(double a)
        {
            decimal result = new decimal(a);
            return result;
        }
        public static decimal Convert(double? a)
        {
            decimal result = (a == null ? 0 : new decimal((double)a));
            return result;
        }
        public static decimal Convert(decimal a)
        {
            return a;
        }
        public static decimal Convert(decimal? a)
        {
            decimal result = (a == null ? 0 : (decimal)a);
            return result;
        }
        public static decimal Convert(DateTime a)
        {
            return a.Ticks;
        }
        public static decimal Convert(object a)
        {
            decimal result = 0;
            if (a == null) 
                result = 0;
            else if (a is int || a is int?) 
                result = new decimal((int)a);
            else if (a is long || a is long?)
                result = new decimal((long)a);
            else if (a is double || a is double?) 
                result = new decimal((double)a);
            else if (a is decimal || a is decimal?) 
                result = (decimal)a;
            else if (a is JsonPrimitive)
            {
                result = Decimal.Parse(a.ToString());
            }
            else if (a is string)
            {
                result = Decimal.Parse((string)a);
            }
            else

                throw new Exception("类型" + a.GetType() + "不能转换成数字");
            return result;
        }
        #endregion

        #region 三目运算符中的类型，全部转换成object
        public static object ToObject(int a)
        {
            return a;
        }
        public static object ToObject(int? a)
        {
            return a;
        }
        public static object ToObject(double a)
        {
            return a;
        }
        public static object ToObject(double? a)
        {
            return a;
        }
        public static object ToObject(decimal a)
        {
            return a;
        }
        public static object ToObject(decimal? a)
        {
            return a;
        }
        public static object ToObject(bool a)
        {
            return a;
        }
        public static object ToObject(bool? a)
        {
            return a;
        }
        public static object ToObject(string a)
        {
            return a;
        }
        public static object ToObject(object a)
        {
            return a;
        }

        public static object ToObject(DateTime a)
        {
            return a;
        }
        #endregion

        #region 转换成不可空bool值，用于逻辑表达式处理
        public static bool ToBool(bool a)
        {
            return a;
        }
        public static bool ToBool(bool? a)
        {
            return a == null ? false : (bool)a;
        }

        public static bool ToBool(object a)
        {
            return a == null ? false : (bool)a;
        }
        #endregion

        //获取对象属性值，用于编译期无法确定对象属性的情况
        public static object GetPropertyValue(object o, string name)
        {
            object result = null;
            if (o == null) return null;
            //先按一般对象获取属性
            PropertyInfo pi = o.GetType().GetProperty(name);
            if (pi != null)
            {
                result = pi.GetValue(o, null);
                return result;
            }
            //没有获取到，按自定义属性获取方式获取属性
            pi = o.NewGetType().GetProperty(name);
            if(pi == null)
                throw new Exception("对象" + o + "没有属性" + name);
            result = pi.GetValue(o, null);
            return result;
        }

        public static object GetPropertyValue(object o, PropertyInfo pi)
        {
            object result = pi.GetValue(o, null);
            return result;
        }

        #region SetPropertyValue 设置属性值
        public static void SetPropertyValue(object o, PropertyInfo pi, object value)
        {
            pi.SetValue(o, value, null);
        }

        public static void SetPropertyValue(object o, PropertyInfo pi, bool value)
        {
            pi.SetValue(o, value, null);
        }

        public static void SetPropertyValue(object o, PropertyInfo pi, int value)
        {
            pi.SetValue(o, value, null);
        }

        public static void SetPropertyValue(object o, PropertyInfo pi, double value)
        {
            pi.SetValue(o, value, null);
        }

        public static void SetPropertyValue(object o, PropertyInfo pi, decimal value)
        {
            pi.SetValue(o, value, null);
        }
        #endregion

        //获取对象属性，用于编译期无法确定对象属性的情况
        public static PropertyInfo GetProperty(object o, string name)
        {
            if (o == null) return null;
            PropertyInfo pi = o.NewGetType().GetProperty(name);
            if (pi == null)
                throw new Exception("对象" + o + "没有属性" + name);
            return pi;
        }

        //从总金额中减，直到不够减为止，获得最后一个够减的对象
        public static GeneralObject EndItem(IEnumerable<object> list, Delegate d, decimal money)
        {
            decimal m = decimal.Parse(money.ToString());
            object end = null;
            foreach (object obj in list)
            {
                object r = d.DynamicInvoke(new object[] { obj });
                decimal v = decimal.Parse(r.ToString());
                m -= v;
                if (m < 0)
                    break;
                //end为最后一个够减的对象
                end = obj;
            }
            return end as GeneralObject;
        }



        //设置选中，目前用于机表交费一户多表，列表中含有多个用户的多条未交费记录，按f_userid,抄表日期排序,
        //从第一个用户的一条记录开始过滤，金额购，处理，不够，跳过该用户所有记录
        public static IEnumerable<object> SetSelected(IEnumerable<object> list, Delegate d, decimal money)
        {
            decimal m = decimal.Parse(money.ToString());
            object end = null;
            //上一个用户id
            string preuid= "";
            foreach (object obj in list)
            {
                GeneralObject go = (GeneralObject)obj;
                string uid = (string)go.GetPropertyValue("f_userid");
                object r = d.DynamicInvoke(new object[] { obj });
                decimal v = decimal.Parse(r.ToString());
                //preuid不为空说明该用户记录跳过
                if (preuid.Equals(uid))
                {
                    go.SetValue("IsChecked", false);
                    continue;
                }
                if (m >= v)
                {
                    m -= v;
                    preuid = "";
                    go.SetValue("IsChecked", true);
                }
                else
                {
                    preuid = uid;
                    go.SetValue("IsChecked", false);
                }
            }
            return list;
        }

        //Sum求和，对列表中的数据求和, d是编译后的参数表达式语句
        public static object Sum(IEnumerable<object> list, Delegate d)
        {
            if (list == null) return null;

            object s = list.Sum(f =>
            {
                object r = d.DynamicInvoke(new object[] { f });
                if (r is int || r is double)
                    return decimal.Parse(r.ToString());
                return (decimal?)r;
            });
            return s;
        }

        //Max，求最大值
        public static object Max(IEnumerable<object> list, Delegate d)
        {
            object s = list.Max(f =>
            {
                object r = d.DynamicInvoke(new object[] { f });
                return r;
            });
            return s;
        }

        //对列表中的数据对象挨个调用处理过程
        public static void Each(IEnumerable<object> list, Delegate d)
        {
            foreach (object o in list)
            {
                d.DynamicInvoke(new object[] { o });
            }
        }
        public static void Each(IEnumerable list, Delegate d)
        {
            List<object> l = new List<object>();
            foreach (object o in list)
            {
                l.Add(o);
            }
            foreach (object o in l)
            {
                d.DynamicInvoke(new object[] { o });
            }
        }

        //对列表中的对象按条件进行过滤，条件是编译好的程序
        public static IEnumerable<object> Where(object obj, Delegate where)
        {
            if (obj == null)
            {
                return null;
            }
            if (!(obj is IEnumerable<object>))
                throw new Exception("只能对列表调用Where函数");
            IEnumerable<object> list = (IEnumerable<object>)obj;
            IEnumerable<object> s = list.Where(f => 
            {
                if (where.Method.GetParameters().Length == 2)
                {
                    bool r = false;
                    try
                    {
                        r = (bool)where.DynamicInvoke(new object[] { f });
                    }
                    //忽略空指针异常
                    catch (TargetInvocationException e)
                    {
                        if (!(e.InnerException != null && e.InnerException is NullReferenceException))
                        {
                            throw e;
                        }
                    }
                    return r;
                }
                else
                {
                    bool r = (bool)where.DynamicInvoke();
                    return r;
                }
            });
            return s;
        }


        //对列表中的对象按条件进行过滤，条件是编译好的程序
        public static string ToWhere(object list)
        {
            IEnumerable<object> enumer = (IEnumerable<object>)list;
            StringBuilder sb = new StringBuilder();
            foreach (var obj in enumer)
            {
                CustomTypeHelper cth = (CustomTypeHelper)obj;
                string field =  cth.GetPropertyValue("code").ToString();
                string fieldtype =(string) cth.GetPropertyValue("fieldtype");
                PropertyInfo pi = cth.GetProperty("f_changehou");
                object val = cth.GetPropertyValue("f_changehou");
                if (val == null)
                    continue;
                    //如果有自定义类型
                else if (fieldtype != null && fieldtype.Equals("oracledate"))
                {
                  sb.Append(",").Append(field).Append(" = ").Append("to_date('"+val.ToString()+"','yyyy-mm-dd')");
                }

                else if (pi.PropertyType == typeof(int) || pi.PropertyType == typeof(long) || pi.PropertyType == typeof(double)
                    || pi.PropertyType == typeof(decimal))
                    sb.Append(",").Append(field).Append(" = ").Append(val);

                else if (pi.PropertyType == typeof(string))
                {
                    string v = "'" + val.ToString() + "'";
                    sb.Append(",").Append(field).Append(" = ").Append(v);
                }
                else
                    throw new Exception("类型" + val.GetType() + "未知");
            }
            if (sb.Length == 0) return "";
            return sb.Remove(0, 1).ToString();
        }

      


        //ToString，把列表中的对象转换成“，”分隔的字符串
        public static string ToString(IEnumerable<object> list, Delegate d)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var obj in list)
            {
                object r = d.DynamicInvoke(new object[] { obj });
                sb.Append(",").Append(r.ToString());
            }
            if (sb.Length == 0) return "";
            return sb.Remove(0, 1).ToString();
        }


        //ToString，把列表中的对象转换成“，”分隔的字符串,单引号包含
        public static string ToStringWithSQ(IEnumerable<object> list, Delegate d)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var obj in list)
            {
                object r = d.DynamicInvoke(new object[] { obj });
                sb.Append(",").Append("'").Append(r.ToString()).Append("'");
            }
            if (sb.Length == 0) return "";
            return sb.Remove(0, 1).ToString();
        }

        //非泛型 ToStringWithSQ 把列表中的对象转换成“，”分隔的字符串,单引号包含
        public static string ToStringWithSQ(IEnumerable list, Delegate d)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var obj in list)
            {
                object r = d.DynamicInvoke(new object[] { obj });
                sb.Append(",").Append("'").Append(r.ToString()).Append("'");
            }
            if (sb.Length == 0) return "";
            return sb.Remove(0, 1).ToString();
        }
      
       
        //为非泛型准备的ToString函数
        public static string ToString(IEnumerable list, Delegate d)
        {
            //把非泛型里的数据全部转换到泛型中，然后调用泛型的方法
            List<object> objs = new List<object>();
            foreach(object obj in list)
            {
                objs.Add(obj);
            }
            return ToString(objs, d);
        }

        //把日期转换成固定格式的字符串
        public static string ToString(object obj, string format)
        {
            if (obj is DateTime)
               return ((DateTime)obj).ToString(format);
            
            
            
            
            
            
            throw new Exception("带格式的字符串转换函数不认识数据类型: " + obj.GetType());
        }

        //把日期转换成固定格式的字符串
        public static string ReplaceStr(object obj, string format)
        {
            string text = (string)obj;
            if (text == null)
            {
                return text;
            }
            string[] strs = format.Split('|');
            return text.Replace(strs[0], strs[1]);
        }

        //获取某月总天数
        public static int DayOfMonth(DateTime date, int year, int month)
        {
           return DateTime.DaysInMonth(year, month);
        }

        //把double转换成固定格式的字符串
        public static string ToString(object obj)
        {
            if (obj is long)
                return obj.ToString();
            throw new Exception("带格式的字符串转换函数不认识数据类型: " + obj.GetType());
        }

         //把日期转换成固定格式的字符串
        public static string Format(int obj, string format)
        {
            format = "{" + format + "}";
            format = format.Replace('|', ':');
            return String.Format(format, obj);
            throw new Exception("无法进行格式化处理: " + obj.GetType());
        }

        //把对象转换成日期
        public static DateTime ToDate(object obj)
        {
            if (obj == null || obj.Equals(""))
            {
                throw new NullReferenceException();
            }
            if (obj is string)
            {
                string str = (string)obj;
                DateTime dt;
                //转换成功，直接返回，否则，按原来的系统截取字符串
                if (DateTime.TryParse(str, out dt))
                {
                    return dt;
                }
                str = str.Substring(4, 2) + "/" + str.Substring(6, 2) + "/" + str.Substring(0, 4) + " 00:00:00";
                dt = DateTime.Parse(str);
                return dt;
            }
            else if (obj is DateTime)
            {
                 return (DateTime)obj;
            }
            else if (obj is decimal)
            {
                DateTime from1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
                TimeSpan ts = new TimeSpan(DateTime.Now.ToUniversalTime().Ticks - from1970.Ticks);
                long _timeDiff = long.Parse(obj.ToString()) - (long)ts.TotalMilliseconds;
                return DateTime.Now.AddMilliseconds(_timeDiff);
            }
            throw new Exception("只有字符串或者日期型可以强转成日期");
        }

                //把对象转换成整形数
        public static int ToInt(decimal obj)
        {
            return (int)obj; 
        }
 
        //把对象转换成整形数
        public static int ToInt(object obj)
        {
            if (obj == null)
            {
                throw new NullReferenceException();
            }
            
            if (obj is string)
            {
                string str = (string)obj;
                int dt = int.Parse(str);
                return dt;


            }
            else if (obj is int)
            {
                return (int)obj;
            }
            else if (obj is decimal)
            {
                decimal d = (decimal)obj; 
                return (int)d;
            }
            throw new Exception("只有字符串或者整形可以强转成整形");
                 }



  
        public static long ToLong(object obj)
        {
            if (obj == null)
            {
                throw new NullReferenceException();
            }
            if (obj is string)
            {
                string str = (string)obj;
                long dt = long.Parse(str);
                return dt;
            }

            throw new Exception("只有字符串或者整形可以强转成整形");
        }

        public static double ToDouble(object obj)
        {
            if (obj == null)
            {
                throw new NullReferenceException();
            }
            if (obj is string)
            {
                string str = (string)obj;
                double dt = double.Parse(str);
                return dt;
            }
            if (obj is decimal || obj is double)
            {
                return double.Parse(obj.ToString());
            }
            throw new Exception("只有字符串或者整形可以强转成Double型");
        }


       

        //把列表转换成通用对象列表
        public static BaseObjectList ToObjectList(object list)
        {
            if (list == null)
            {
                throw new NullReferenceException();
            }
            //只有枚举对象才可以转换成通用对象
            if (!(list is IEnumerable))
            {
                throw new Exception("非枚举对象无法转换成对象列表");
            }
            BaseObjectList result = new ObjectList();
            foreach (GeneralObject go in (IEnumerable)list)
            {
                result.Add(go);
            }
            return result;
        }


        //通过树状结构子属性名把列表转换成通用对象列表
        public static BaseObjectList ToList(object list, string childPropertyName)
        {
            //只有枚举对象才可以转换成通用对象
            if (!(list is IEnumerable))
            {
                throw new Exception("非枚举对象无法转换成对象列表：");
            }
            BaseObjectList result = new ObjectList();
            foreach (GeneralObject go in (IEnumerable)list)
            {
                BaseObjectList bol = (BaseObjectList)go.GetPropertyValue(childPropertyName);
                if (bol != null)
                {
                    foreach (GeneralObject cgo in bol)
                    {
                        result.Add(cgo);
                    }
                }
            }
            return result;
        }

        //返回list第一项数据
        public static object First(object list)
        {
            //只有枚举对象才可以转换成通用对象
            if (!(list is IEnumerable))
            {
                throw new Exception("非枚举对象无法转换成对象列表：");
            }
            IEnumerator temp = ((IEnumerable)list).GetEnumerator();
            bool hasNext =  temp.MoveNext();
            object ret = hasNext ? temp.Current : null;
            return ret;
        }

        //对象比较运算
        public static bool Compare(object obj1, object obj2)
        {
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null && obj2 != null) return false;
            if (obj1 != null && obj2 == null) return false;
            object s1 = (obj1.GetType().IsEnum || obj1.GetType().IsPrimitive || obj1 is decimal) ? obj1.ToString() : obj1;
            object s2 = (obj2.GetType().IsEnum || obj2.GetType().IsPrimitive || obj2 is decimal) ? obj2.ToString() : obj2;
            return s1.Equals(s2);
        }

        //根据对象和方法名调用
        public static object NewMethodCall(object obj,string methodName)
        {
           MethodInfo mi =  obj.GetType().GetMethod(methodName, new Type[] { });
           return mi.Invoke(obj, new object[] { });
        }

        //根据对象、方法名及参数调用
        public static object NewMethodCall(object obj, string methodName, object[] param)
        {
            //运行期调用方法时，只能找到参数类型完全一致的方法
            Type[] types = new Type[param.Length];
            for (int i = 0; i < param.Length; i++)
            {
                types[i] = param[i].GetType();
            }
            MethodInfo mi = obj.GetType().GetMethod(methodName, types);
            return mi.Invoke(obj, param);
        }

        //字符串连接
        public static string Concat(object one, object two)
        {
            if (one == null || two == null)
            {
                //throw new NullReferenceException();
                return null;
            }
            return string.Concat(one, two);
        }


        //数字四舍五入
        public static double MathRound(decimal value, int decimals)
        {
            double d = Double.Parse(value.ToString());
            return Math.Round(d, decimals);
        }
        //数字取整
        public static double MathRoundO(decimal value, int decimals)
        {
            double d = Double.Parse(value.ToString());
            return Math.Floor(d);
        }

        public static double MathRound(object value, int decimals)
        {
            double d = Double.Parse(value.ToString());
            return Math.Round(d, decimals);
        }


        //字符串IndexOf
        public static int IndexOf(object one, object two)
        {
            if (one == null || two == null)
            {
                return -1;
            }
            return one.ToString().IndexOf(two.ToString());
        }

        //获取对象在某个列表中的位置
        public static int Pos(object go, object list)
        {
            if (go == null)
                throw new NullReferenceException();
            if (!(go is GeneralObject && list is BaseObjectList))
            {
                throw new Exception("Pos函数只能用于获取GeneralObject在BasicObjectList中的位置");
            }
            GeneralObject ngo = (GeneralObject)go;
            BaseObjectList nlist = (BaseObjectList)list;
            int ret = nlist.IndexOf(go);
            return ret;
        }

        //金额转大写
        public static string ToChinaMoney(object money)
        {
            if (money == null ||Double.Parse(money.ToString())==0)
            {
                return "零元整";
            }
            // 定义转换后的大写字符和单位
            string upper = "零壹贰叁肆伍陆柒捌玖";
            string bit = "分角整元拾佰仟万拾佰仟亿拾佰仟";
            // 定义一个临时变量
            string temp = "";
            // 转换后返回的大写金额
            string upperMoney = "";
            // 金额的整数位长度
            int length = 0;
            // 去掉空格
            temp = money.ToString().Trim();
            // 如果得到的小写金额没有字符"."
            if (temp.IndexOf(".") == -1)
            {
                length = temp.Length;
            }
            // 否则如果有，整数位的长度为
            else
            {
                length = temp.IndexOf(".");
            }
            // 位置变量
            int position = 0;
            // 数字对应的大写汉字
            String upperNum = "";
            // 数字对应的单位
            String unit = "";
            // 对于数字金额来说
            for (int i = 0; i < temp.Length; i++)
            {
                if (i > length + 2)
                {
                    break;
                }
                if (i == length)
                {
                    continue;
                }
                // 找出所得的数字对应的中文
                char[] c = temp.ToCharArray();
                position = Int32.Parse(c[i] + "");
                upperNum = upper.Substring(position, 1);
                // 根据位数算得他的单位
                position = length - i + 2;
                unit = bit.Substring(position, 1);
                // 得到的大写金额为
                upperMoney = upperMoney + upperNum + unit;
            }
            // 如果upperMoney中含有“零仟、零佰、零拾、零元、零角”的，都以零代替
            if (upperMoney.IndexOf("零拾") != -1 || upperMoney.IndexOf("零佰") != -1
                    || upperMoney.IndexOf("零仟") != -1
                    || upperMoney.IndexOf("零元") != -1
                    || upperMoney.IndexOf("零角") != -1)
            {
                upperMoney = upperMoney.Replace("零拾", "零");
                upperMoney = upperMoney.Replace("零佰", "零");
                upperMoney = upperMoney.Replace("零仟", "零");
            }
            // 如果upperMoney中有相连的两个“零”，或三个、四个，都换为一个
            if (upperMoney.IndexOf("零零") != -1 || upperMoney.IndexOf("零零零") != -1
                    || upperMoney.IndexOf("零零零零") != -1)
            {
                upperMoney = upperMoney.Replace("零零零零", "零");
                upperMoney = upperMoney.Replace("零零零", "零");
                upperMoney = upperMoney.Replace("零零", "零");
            }
            // 如果有零亿、零万或者零元,以亿、万或元代替
            if (upperMoney.IndexOf("零亿") != -1 || upperMoney.IndexOf("零万") != -1
                    || upperMoney.IndexOf("零元") != -1)
            {
                upperMoney = upperMoney.Replace("零亿", "亿");
                upperMoney = upperMoney.Replace("零万", "万");
                upperMoney = upperMoney.Replace("零元", "元");
            }
            // 如果“万”前面是“亿”的话，把万去掉
            if (upperMoney.LastIndexOf("亿万") != -1)
            {
                upperMoney = upperMoney.Replace("亿万", "亿");
            }
            // 如果小写金额最后两位为零，补整
            if (upperMoney.LastIndexOf("零角零分") != -1)
            {
                upperMoney = upperMoney.Replace("零角零分", "整");
            }
            // 否则，如果有“零角”，以零代替
            else if (upperMoney.LastIndexOf("零角") != -1)
            {
                upperMoney = upperMoney.Replace("零角", "零");
            }
            // 如果有“零分”，以零代替
            if (upperMoney.LastIndexOf("零分") != -1)
            {
                upperMoney = upperMoney.Replace("零分", "");
            }
            // 如果整数位长度等于小写金额的位数，给大写金额加整
            if ((length == temp.Length) || (length == temp.Length - 1))
            {
                upperMoney = upperMoney + "整";
            }
            // 如果最后一位为角，加整
            if (upperMoney.EndsWith("角"))
            {
                upperMoney = upperMoney + "整";
            }
            // 如果小数位后只有一位数字，且为零，去零加整
          //  if (upperMoney.EndsWith("零"))
           // {
           //     upperMoney = upperMoney.Substring(0, upperMoney.Length - 1) + "整";
          //  }
            return upperMoney;
        }

        #region BringToUp 把一个界面元素放大显示到某容器中
        public static void BringToUp(FrameworkElement ui, FrameworkElement parent)
        {
            //获得需要平移的坐标值
            MatrixTransform pt = (MatrixTransform)ui.TransformToVisual(parent);
            CompositeTransform trans = new CompositeTransform();
            trans.TranslateX = -pt.Matrix.OffsetX;
            trans.TranslateY = -pt.Matrix.OffsetY;
            //计算放大比例
            //trans.ScaleX = parent.ActualWidth / ui.ActualWidth;
            //trans.ScaleY = parent.ActualHeight / ui.ActualHeight;
            ui.RenderTransform = trans;
            //放置到最上面
            //Canvas.SetZIndex(ui, 100);
        }
        #endregion

        #region BringToDown 把放大显示的界面还原回去
        public static void BringToDown(FrameworkElement ui)
        {
            ui.RenderTransform = null;
            Canvas.SetZIndex(ui, 0);
        }
        #endregion

        #region RemoveTop 移走Frame中最顶层的子
        public static void RemoveTop(Panel ui)
        {
            ControlAttach.RemoveTop(ui);
        }
        #endregion

        #region GetTop 获得Frame最顶层子的名称
        public static string GetTop(Panel ui)
        {
            string name = ControlAttach.GetTop(ui);
            return name;
        }
        #endregion

        #region SetSource 设置容器控件的Source附加属性
        public static void SetSource(FrameworkElement c, string name)
        {
            ControlAttach.SetSource(c, name);
        }
        #endregion

        #region 设置Grid某列宽度
        public static void ColumnWidth(Grid grid, int col, string width)
        {
            if (width == "*")
            {
                grid.ColumnDefinitions[col].Width = new GridLength(0, GridUnitType.Auto);
            }
            else if (width.EndsWith("*"))
            {
                int size = int.Parse(width.Substring(0, width.Length - 1));
                grid.ColumnDefinitions[col].Width = new GridLength(size, GridUnitType.Star);
            }
            else
            {
                int size = int.Parse(width);
                grid.ColumnDefinitions[col].Width = new GridLength(size, GridUnitType.Pixel);
            }
        }
        #endregion

        #region ToVisibility 字符转可见性
        public static Visibility ToVisibility(object o)
        {
            string str = o.ToString();
            if (str == "Visible")
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }
        #endregion

        #region 把空值转换成空串，Text框用
        public static object NullToEmpty(object o)
        {
            if (o == null)
            {
                return "";
            }
            return o;
        }
        #endregion

        #region ToUri 把字符串转换成Uri
        public static Uri ToUri(string str)
        {
            return new Uri(str, UriKind.Relative);
        }
        #endregion

        #region Add_ 往ObjectList里添加一项
        public static void Add_(object list, object go)
        {
            if (!(list is BaseObjectList) || !(go is GeneralObject))
                throw new Exception("只能把GeneralObject放进BasicObjectList中");
            BaseObjectList l = list as BaseObjectList;
            GeneralObject o = go as GeneralObject;
            l.Add(o);
        }
        #endregion

        #region GetMinNum 获取一户多表的最小指数
        //Sum求和，对列表中的数据求和, d是编译后的参数表达式语句
        public static double GetMinNum(IEnumerable<object> list)
        {
            Dictionary<string, double> mins = new Dictionary<string, double>();

            foreach (GeneralObject go in list)
            {
                string f_userid = go.GetPropertyValue("f_userid") as string;
                double lastinputgasnum = double.Parse(go.GetPropertyValue("lastinputgasnum").ToString());
                //如果编号不存在，把值放入当前编号
                if (!mins.Keys.Contains(f_userid))
                {
                    mins[f_userid] = lastinputgasnum;
                }
                //否则，取出编号，当前小于存放的，把当前放进去
                else
                {
                    double old = mins[f_userid];
                    if (lastinputgasnum < old)
                    {
                        mins[f_userid] = lastinputgasnum;
                    }
                }
            }

            //对所有表的最小指数求和
            double result = mins.Values.Sum();
            return result;
        }
        #endregion

        #region GetMaxNum 获取一户多表的最大指数
        //Sum求和，对列表中的数据求和, d是编译后的参数表达式语句
        public static double GetMaxNum(IEnumerable<object> list)
        {
            Dictionary<string, double> mins = new Dictionary<string, double>();

            foreach (GeneralObject go in list)
            {
                string f_userid = go.GetPropertyValue("f_userid") as string;
                double lastrecord = double.Parse(go.GetPropertyValue("lastrecord").ToString());
                //如果编号不存在，把值放入当前编号
                if (!mins.Keys.Contains(f_userid))
                {
                    mins[f_userid] = lastrecord;
                }
                //否则，取出编号，当前小于存放的，把当前放进去
                else
                {
                    double old = mins[f_userid];
                    if (lastrecord > old)
                    {
                        mins[f_userid] = lastrecord;
                    }
                }
            }

            //对所有表的最小指数求和
            double result = mins.Values.Sum();
            return result;
        }
        #endregion

    }
}

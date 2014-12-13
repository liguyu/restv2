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
using System.Xaml;
using System.Windows.Markup;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using Com.Aote.Utils;
using System.Linq.Expressions;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;
using System.Collections;
using Com.Aote.ObjectTools;
using Com.Aote.Logs;

namespace Com.Aote.Marks
{
    //有返回值，没有参数的代理
    public delegate object Value();

    /// <summary>
    /// 表达式标记扩展，即可用于属性值，也可用于事件处理
    /// </summary>
    public class Exp : FrameworkElement, IMarkupExtension<object>
    {
        private static Log Log = Log.GetInstance("Com.Aote.Marks.Exp");

        //要执行的表达式
        public string Str { get; set; }

        //目标对象，标记所加入到的目标对象
        public object _targetObject;

        //目标对象属性，标记加入的目标对象属性
        private PropertyInfo _targetProperty;

        //所加入为依赖属性，依赖属性所调用的设置值的方法
        private MethodInfo _targetMethod;

        //所加入的事件处理
        private EventInfo _targetEvent;

        //编译好的结果，只有集合中对象的属性变化需要
        private Delegate Result;

        //语法树，便于跟踪
        private System.Linq.Expressions.Expression Expression;

        //所有建立好的绑定放在这里，以免丢失
        private static List<BindingSlave> _bindings = new List<BindingSlave>();

        //所有已经编译的事件处理结果存放在这里，key为事件信息，value为编译结果
        private static Dictionary<object, Delegate> _progs = new Dictionary<object, Delegate>();

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            var target =
                (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            _targetObject = target.TargetObject;
            //目标对象是PropertySetter，将Exp给PropertySetter，以便列表中重用
            if (_targetObject is PropertySetter)
            {
                PropertySetter ps = (PropertySetter)_targetObject;
                ps.Exps.Add(this);
            }
            object obj = target.TargetProperty;
            //是静态Get方法，依赖属性取值
            if (obj is MethodInfo && (obj as MethodInfo).IsStatic && (obj as MethodInfo).Name.StartsWith("Get"))
            {
                MethodInfo mi = (MethodInfo)obj;
                //如果方法返回值是代理，当做事件处理
                if (mi.ReturnType == typeof(Delegate))
                {
                    return new Action(EventHandle);
                }
                else
                {
                    //对象的Set属性方法，在赋值时调用
                    _targetMethod = mi.ReflectedType.GetMethod("Set" + mi.Name.Substring(3));
                    //界面加载完成后，再进行表达式处理
                    handler = new RoutedEventHandler(OnLoaded);
                    _targetObject.GetType().GetEvent("Loaded").AddEventHandler(_targetObject, handler);
                    //返回原来的值
                    object o = mi.Invoke(null, new object[] { _targetObject });
                    return o;
                }
            }
            else if (obj is PropertyInfo)
            {
                var pi = (obj as PropertyInfo);
                _targetProperty = obj as PropertyInfo;
                //界面加载完成后，再进行表达式处理
                handler = new RoutedEventHandler(OnLoaded);
                _targetObject.GetType().GetEvent("Loaded").AddEventHandler(_targetObject, handler);
                //返回原来的值
                return _targetProperty.GetValue(_targetObject, null);
            }
            else if (obj is EventInfo)
            {
                _targetEvent = (EventInfo)obj;
                //是事件，调用事件处理过程
                if (_targetEvent.EventHandlerType == typeof(SelectionChangedEventHandler))
                {
                    return new SelectionChangedEventHandler((o, e) => { EventHandle(); });
                }
                else if (_targetEvent.EventHandlerType == typeof(RoutedEventHandler))
                {
                    return new RoutedEventHandler((o, e) => { EventHandle(); });
                }
                else if (_targetEvent.EventHandlerType == typeof(AsyncCompletedEventHandler))
                {
                    return new AsyncCompletedEventHandler((o, e) => { EventHandle(); });
                }
                else if (_targetEvent.EventHandlerType == typeof(MouseButtonEventHandler))
                {
                    return new MouseButtonEventHandler((o, e) => { EventHandle(); });
                }
                else if (_targetEvent.EventHandlerType == typeof(EventHandler))
                {
                    return new EventHandler((o, e) => { EventHandle(); });
                }
                else if (_targetEvent.EventHandlerType == typeof(EventHandler<EventArgs>))
                {
                    return new EventHandler<EventArgs>((o, e) => { EventHandle(); });
                }
                else if (_targetEvent.EventHandlerType == typeof(PopulatingEventHandler))
                {
                    return new PopulatingEventHandler((o, e) => { EventHandle(); });
                }
                else if (_targetEvent.EventHandlerType == typeof(RoutedPropertyChangedEventHandler<Boolean>))
                {
                    return new RoutedPropertyChangedEventHandler<Boolean>((o, e) => { EventHandle(); });
                }
                else
                {
                    throw new Exception("不支持这种事件类型：" + _targetEvent.EventHandlerType);
                }
            }
            else
            {
                return null;
                //throw new Exception("表达式不能赋值给事件");
            }
        }

        private RoutedEventHandler handler;
        public void OnLoaded(object o, RoutedEventArgs e)
        {
            //避免重复工作
            _targetObject.GetType().GetEvent("Loaded").RemoveEventHandler(_targetObject, handler);
            handler = null;
            //对表达式进行解析，在绑定发生变化时，调用表达式执行
            //可以对一组表达式工作，表达式之间用'|or|'分割
            string[] strs = Str.Split(new String[] { "|or|" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string str in strs)
            {
                Program prog = new Program(str, _targetObject, true);
                Delegate result = prog.Parse(prog.Exp);
                Result = result;
                Expression = prog.exp;
                //如果目标是一个代理，直接把编译结果给目标
                if (_targetProperty != null && _targetProperty.PropertyType == typeof(Delegate))
                {
                    _targetProperty.SetValue(_targetObject, result, null);
                    return;
                }
                //如果有事件，则只是在事件发生时才执行表达式。第一次默认及属性变化都不执行。
                if (prog.Events.Count == 0)
                {
                    //先执行一次默认赋值过程，然后当属性值改变时，再调用每个赋值过程
                    Assign(result);
                    foreach (BindingSlave bs in prog.Bindings)
                    {
                        //把BS保存起来，以免丢失
                        bs.Result = result;
                        _bindings.Add(bs);
                        bs.PropertyChanged += (o1, e1) =>
                        {
                            if (e1.PropertyName == "Value")
                            {
                                BindingSlave _bs = (BindingSlave)o1;
                                Assign(bs.Result);
                                //如果是集合本身变了，要监听集合内容的变化过程
                                if (_bs.Value is INotifyCollectionChanged)
                                {
                                    MonityCollection(_bs);
                                }
                            }
                        };
                        //如果绑定到了集合，要监听集合变化过程
                        if (bs.Value is INotifyCollectionChanged)
                        {
                            MonityCollection(bs);
                        }
                    }
                }
                //执行事件触发时的表达式计算过程
                foreach (ObjectEvent ei in prog.Events)
                {
                    if (ei.Event.EventHandlerType == typeof(SelectionChangedEventHandler))
                    {
                        ei.Event.AddEventHandler(ei.Object, new SelectionChangedEventHandler((o1, e1) => { Assign(result); }));
                    }
                    else if (ei.Event.EventHandlerType == typeof(RoutedEventHandler))
                    {
                        ei.Event.AddEventHandler(ei.Object, new RoutedEventHandler((o1, e1) => { Assign(result); }));
                    }
                    else if (ei.Event.EventHandlerType == typeof(AsyncCompletedEventHandler))
                    {
                        ei.Event.AddEventHandler(ei.Object, new AsyncCompletedEventHandler((o1, e1) => { Assign(result); }));
                    }
                    else if (ei.Event.EventHandlerType == typeof(MouseButtonEventHandler))
                    {
                        ei.Event.AddEventHandler(ei.Object, new MouseButtonEventHandler((o1, e1) => { Assign(result); }));
                    }
                    else if (ei.Event.EventHandlerType == typeof(RoutedPropertyChangedEventHandler<object>))
                    {
                        ei.Event.AddEventHandler(ei.Object, new RoutedPropertyChangedEventHandler<object>((o1, e1) => { Assign(result); }));
                    }
                    else if (ei.Event.EventHandlerType == typeof(PopulatingEventHandler))
                    {
                        ei.Event.AddEventHandler(ei.Object, new PopulatingEventHandler((o1, e1) => { Assign(result); }));
                    }
                    else if (ei.Event.EventHandlerType == typeof(RoutedPropertyChangedEventHandler<Boolean>))
                    {
                        ei.Event.AddEventHandler(ei.Object, new RoutedPropertyChangedEventHandler<Boolean>((o1, e1) => { Assign(result); }));
                    }
                    else if (ei.Event.EventHandlerType == typeof(NotifyCollectionChangedEventHandler))
                    {
                        ei.Event.AddEventHandler(ei.Object, new NotifyCollectionChangedEventHandler((o1, e1) => { Assign(result); }));
                    }
                    else if (ei.Event.EventHandlerType == typeof(EventHandler))
                    {
                        ei.Event.AddEventHandler(ei.Object, new EventHandler((ol, el) => { Assign(result); }));
                    }
                    else
                    {
                        throw new Exception("不支持这种事件类型：" + ei.Event.EventHandlerType);
                    }
                }
            }
        }

        //监听集合本身的变化过程
        private void MonityCollection(BindingSlave bs)
        {
            //开始要监听集合中对象的属性变化过程
            foreach (object obj in (IEnumerable)bs.Value)
            {
                if (obj is GeneralObject)
                    (obj as GeneralObject).DynamicPropertyChanged += PropertyChangedHandler;
            }
            //监听集合本身的变化过程
            (bs.Value as INotifyCollectionChanged).CollectionChanged += (o1, e1) =>
            {
                Assign(bs.Result);
                //reset时，监听集合自身所有对象变化
                if (e1.Action == NotifyCollectionChangedAction.Reset)
                {
                    foreach (object obj in (IEnumerable)o1)
                    {
                        if (obj is GeneralObject)
                            (obj as GeneralObject).DynamicPropertyChanged += PropertyChangedHandler;
                    }
                }
                //监听集合中新增对象的属性变化
                if (e1.NewItems != null)
                {
                    foreach (object obj in e1.NewItems)
                    {
                        if (obj is GeneralObject)
                            (obj as GeneralObject).DynamicPropertyChanged += PropertyChangedHandler;
                    }
                }
                //删除集合中移除对象的属性变化
                if (e1.OldItems != null)
                {
                    foreach (object obj in e1.OldItems)
                    {
                        if (obj is GeneralObject)
                            (obj as GeneralObject).DynamicPropertyChanged -= PropertyChangedHandler;
                    }
                }
            };
        }

        //集合中对象属性发生变化时的监听过程
        private void PropertyChangedHandler(object o, PropertyChangedEventArgs e)
        {
            Assign(Result);
        }

        //赋值操作，要调用新的赋值方法，以处理字符串到其他数据类型的转换
        private void Assign(Delegate result)
        {
            try
            {
                object value = null;
                //如果目标对象是组件，把当时的数据上下文当做参数传递进去
                if (_targetObject is FrameworkElement && result.Method.GetParameters().Length == 2)
                {
                    value = result.DynamicInvoke(new object[] { (_targetObject as FrameworkElement).DataContext });
                }
                else if (_targetObject is PropertySetter && result.Method.GetParameters().Length == 2)
                {
                    value = result.DynamicInvoke(new object[] { (_targetObject as PropertySetter).Object });
                }
                else
                {
                    value = result.DynamicInvoke();
                }
                //不是依赖属性，直接赋值
                if (_targetMethod == null)
                {
                    /*
                    string name = _targetProperty.Name;
                    if (_targetObject is PropertySetter)
                    {
                        PropertySetter ps = (PropertySetter)_targetObject;
                        name = ps.PropertyName;
                    }
                    Log.Debug("assign property=" + name + ", value=" + (value == null ? "null" : value.ToString()) + ", exp=" + Str);
                    */
                    _targetProperty.NewSetValue(_targetObject, value);
                }
                else
                {
                    _targetMethod.Invoke(null, new object[] { _targetObject, value });
                }
            }
            catch (TargetInvocationException ex)
            {
                //忽略空指针异常，这时可能因为绑定没有值，没法计算表达式值
                if (!(ex.InnerException is NullReferenceException))
                {
                    ex.Data["str"] = Str;
                    ex.Data["targetProperty"] = _targetProperty.Name;
                    ex.Data["targetObject"] = _targetObject.ToString();
                    throw ex;
                }
            
            }
        }

        /// <summary>
        /// 事件产生时的处理动作，调用表达式执行
        /// </summary>
        private void EventHandle()
        {
            //在缓存中取编译结果
            if (!_progs.ContainsKey(this))
            {
                Program exp = new Program(Str, _targetObject, false);
                _progs[this] = exp.Parse(exp.Prog);
            }
            Delegate result = _progs[this];
            //如果目标对象是组件，把当时的数据上下文当做参数传递进去
            if (_targetObject is FrameworkElement && result.Method.GetParameters().Length == 2)
            {
                result.DynamicInvoke(new object[] { (_targetObject as FrameworkElement).DataContext });
            }
            else
            {
                result.DynamicInvoke();
            }
        }

        /// <summary>
        /// 用于属性是一个处理过程的情况
        /// </summary>
        /// <returns></returns>
        private object ValueHandle()
        {
            //在缓存中取编译结果
            if (!_progs.ContainsKey(this))
            {
                Program exp = new Program(Str, _targetObject, false);
                _progs[this] = exp.Parse(exp.Exp);
                Expression = exp.exp;
            }
            object result = _progs[this].DynamicInvoke();
            return result;
        }

        #region Clone 复制
        public Exp Clone()
        {
            Exp exp = new Exp();
            exp._targetProperty = this._targetProperty;
            exp.Str = this.Str;
            return exp;
        }
        #endregion
    }
}

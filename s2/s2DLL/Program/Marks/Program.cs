using System;
using System.Net;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq.Expressions;
using Com.Aote.Utils;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Data;
using System.Linq;
using Com.Aote.ObjectTools;
using System.Collections;

namespace Com.Aote.Marks
{
    /// <summary>
    /// 执行一段程序，例如函数调用，赋值等。也可以用于给属性赋值
    /// </summary>
    public class Program
    {
        //源程序
        public string Source { get; set; }

        //配置表达式的对象
        public object UI { get; set; }

        //是否处理绑定，事件处理不处理绑定
        private bool IsBinding { get; set; }

        //Token堆栈，用于回退
        private List<Token> _tokens = new List<Token>();

        //当前获取到的字符位置
        private int pos;

        //当前是否在字符串处理环节
        private bool inString;

        //字符串处理环节堆栈，@进入字符串处理环节，“{}”中间部分脱离字符串处理环节
        private List<bool> inStrings = new List<bool>();

        //集合对象参数表达式，在进行集合对象的参数表达式解析时，名称为参数的属性名
        private ParameterExpression param;

        #region outParams 外部传入的参数列表，用于表达式本身含外部参数的情况
        private List<ParameterExpression> outParams = new List<ParameterExpression>();
        private ParameterExpression GetOutParam(string name)
        {
            //参数名固定为valuei，i为第几项参数，value0简写为value
            int i = 0;
            if (name == "value")
            {
                i = 0;
            }
            else
            {
                i = int.Parse(name.Substring(5));
            }
            //按照i指定的书面，新增参数
            int size = i - outParams.Count;
            for (int j = 0; j <= size; j++)
            {
                outParams.Add(Expression.Parameter(typeof(object)));
            }
            return outParams[i];
        }
        #endregion

        //绑定辅助对象列表，表达式解析完成后，外面的对象可以调用绑定辅助对象，监听其值变化，调用表达式赋值过程
        public List<BindingSlave> Bindings = new List<BindingSlave>();

        //事件列表，有时表达式的计算由特殊事件触发，而不是绑定，这里登记所有触发表达式计算的事件
        public List<ObjectEvent> Events = new List<ObjectEvent>();

        //语法树，便于跟踪
        public Expression exp;

        public Program(string prog, object ui, bool isBinding)
        {
            Source = prog;
            UI = ui;
            IsBinding = isBinding;
        }

        public Delegate Parse(Func<Expression> func)
        {
            //构造赋值表达式
            exp = func();
            LambdaExpression com = null;
            if (outParams.Count == 0)
            {
                com = Expression.Lambda(exp);
            }
            else
            {
                //有外面传入参数，按照外面传入参数方式编译
                com = Expression.Lambda(exp, outParams.ToArray<ParameterExpression>());
            }
            return com.Compile();
        }

        //程序=语句(;语句)
        public Expression Prog()
        {
            List<Expression> stas = new List<Expression>();
            stas.Add(Statement());
            Token t = GetToken();
            while (t.Type != TokenType.End && t.Type == TokenType.Oper && t.Value.Equals(";"))
            {
                stas.Add(Statement());
                t = GetToken();
            }
            _tokens.Insert(0, t);
            return Expression.Block(stas.ToArray());
        }

        //语句=赋值语句 | 函数调用语句，赋值语句=对象.属性 '=' 表达式
        private Expression Statement()
        {
            //获取要进行属性赋值或者函数调用的成员信息
            Token t = GetToken();
            if (t.Type != TokenType.Identy)
            {
                throw new Exception("语法错误，左边必须是对象");
            }
            Expression retExp;
            Expression objExp = ObjItem((string)t.Value, true, out retExp);
            
            //看下面是否赋值过程，如果是，调用赋值过程，否则，继续
            Token n = GetToken();
            if (n.Type == TokenType.Oper && (string)n.Value == "=")
            {
                objExp = Assign(objExp, retExp);
            }
            else
            {
                //如果是属性信息，调用属性的取值方法
                if (objExp.Type == typeof(PropertyInfo))
                {
                    MethodInfo mi = typeof(RunLib).GetMethod("GetPropertyValue", new Type[] { typeof(object), typeof(PropertyInfo) });
                    objExp = Expression.Call(mi, new Expression[] { retExp, objExp });
                }
                _tokens.Add(n);
            }

            return objExp;
        }

        //赋值过程
        private Expression Assign(Expression l, Expression retExp)
        {
            Expression r = Exp();
            //如果l为属性，调用属性的SetValue方法
            if (l.Type == typeof(PropertyInfo))
            {
                MethodInfo mi = typeof(RunLib).GetMethod("SetPropertyValue", new Type[] { typeof(object), typeof(PropertyInfo), r.Type });
                return Expression.Call(mi, new Expression[] { retExp, l, r });
            }
            else
            {
                //强制转换成目标类型
                r = Expression.Convert(r, l.Type);
                return Expression.Assign(l, r);
            }
        }

        //表达式=条件:结果(,条件:结果)(,结果)?|结果
        //条件=单个结果
        public Expression Exp()
        {
            Expression v = Logic();
            //是':'，表示条件，否则直接返回单结果
            Token t = GetToken();
            if (t.Type == TokenType.Oper && (string)t.Value == ":")
            {
                //条件转换
                MethodInfo convert = typeof(RunLib).GetMethod("ToBool", new Type[] { v.Type });
                v = Expression.Call(convert, new Expression[] { v });
                //第一项转换
                Expression result = Logic();
                convert = typeof(RunLib).GetMethod("ToObject", new Type[] { result.Type });
                result = Expression.Call(convert, new Expression[] { result });
                //下一个是","，继续读取下一个条件结果串，由于是右结合，只能采用递归
                t = GetToken();
                if(t.Type == TokenType.Oper && (string)t.Value == ",")
                {
                    //第二项转换
                    Expression sExp = Exp();
                    convert = typeof(RunLib).GetMethod("ToObject", new Type[] { sExp.Type });
                    sExp = Expression.Call(convert, new Expression[] { sExp });
                    //返回
                    return Expression.Condition(v, result, sExp);
                }
                else
                {
                    throw new Exception(GetExceptionMessage("必须有默认值!"));
                }
            }
            else
            {
                _tokens.Add(t);
                return v;
            }
        }

        //单个结果项=逻辑运算 (and|or 逻辑运算)* | !表达式
        private Expression Logic()
        {
            Token t = GetToken();
            if (t.Type == TokenType.Oper && (string)t.Value == "!")
            {
                Expression exp = Logic();
                MethodInfo convert = typeof(RunLib).GetMethod("ToBool", new Type[] { exp.Type });
                exp = Expression.Call(convert, new Expression[] { exp });
                exp = Expression.Not(exp);
                return exp;
            }
            _tokens.Add(t);
            Expression v = Compare();
            t = GetToken();
            while (t.Type == TokenType.Identy && ((string)t.Value == "and" || (string)t.Value == "or"))
            {
                //第一项转换
                MethodInfo convert = typeof(RunLib).GetMethod("ToBool", new Type[] { v.Type });
                v = Expression.Call(convert, new Expression[] { v });
                //第二项转换
                Expression exp = Logic();
                convert = typeof(RunLib).GetMethod("ToBool", new Type[] { exp.Type });
                exp = Expression.Call(convert, new Expression[] { exp });
                //执行
                if (t.Value.Equals("and"))
                {
                    v = Expression.And(v, exp);
                }
                else
                {
                    v = Expression.Or(v, exp);
                }
                t = GetToken();
            }
            _tokens.Add(t);
            return v;
        }

        //逻辑运算=数字表达式 (比较运算符 数字表达式)?
        private Expression Compare()
        {
            Expression left = Math();
            Token t = GetToken();
            if(t.Type == TokenType.Oper && ((string)t.Value == ">" || (string)t.Value == ">=" || 
                (string)t.Value == "^" || (string)t.Value == "^="))
            {
                MethodInfo convert = typeof(RunLib).GetMethod("Convert", new Type[] { left.Type });
                left = Expression.Call(convert, new Expression[] { left });

                Expression rExp = Math();
                convert = typeof(RunLib).GetMethod("Convert", new Type[] { rExp.Type });
                rExp = Expression.Call(convert, new Expression[] { rExp });

                if ((string)t.Value == ">")
                    return Expression.GreaterThan(left, rExp);
                if ((string)t.Value == ">=")
                    return Expression.GreaterThanOrEqual(left, rExp);
                if ((string)t.Value == "^")
                    return Expression.LessThan(left, rExp);
                if ((string)t.Value == "^=")
                    return Expression.LessThanOrEqual(left, rExp);
            }
            else if (t.Type == TokenType.Oper && ((string)t.Value == "==" || (string)t.Value == "!="))
            {
                //对象不能转换成string进行比较
                if (left.Type.IsEnum || left.Type.IsPrimitive || left.Type == typeof(decimal))
                {
                    MethodInfo convert = left.Type.GetMethod("ToString", new Type[]{});
                    left = Expression.Call(left, convert);
                }
                Expression rExp = Math();
                //只有枚举以及原始类型才转换字符串
                if (rExp.Type.IsEnum || rExp.Type.IsPrimitive || rExp.Type == typeof(decimal))
                {
                    MethodInfo convert = rExp.Type.GetMethod("ToString", new Type[] { });
                    rExp = Expression.Call(rExp, convert);
                }
                MethodInfo mi = typeof(RunLib).GetMethod("Compare", new Type[] { typeof(object), typeof(object) });
                //相等比较
                if ((string)t.Value == "==")
                {
                    if (left.Type != typeof(string) || rExp.Type != typeof(string))
                    {
                        return Expression.Call(mi, new Expression[] { left, rExp });
                    }
                    else
                    {
                        return Expression.Equal(left, rExp);
                    }
                }
                if ((string)t.Value == "!=")
                {
                    if(left.Type != typeof(string) || rExp.Type != typeof(string))
                    {
                        return Expression.Not(Expression.Call(mi, new Expression[] {left, rExp}));
                    }
                    else
                    {
                        return Expression.NotEqual(left, rExp);
                    }
                }
            }
            //返回当个表达式结果
            _tokens.Add(t);
            return left;
        }

        //单个结果项=乘除项 (+|-) 乘除项
        private Expression Math()
        {
            Expression v = Mul();
            Token t = GetToken();
            while (t.Type == TokenType.Oper && ((string)t.Value == "+" || (string)t.Value == "-"))
            {
                //转换操作数1为数字
                MethodInfo convert = typeof(RunLib).GetMethod("Convert", new Type[] { v.Type });
                v = Expression.Call(convert, new Expression[] { v });
                //转换操作数2为数字
                Expression r = Mul();
                convert = typeof(RunLib).GetMethod("Convert", new Type[] { r.Type });
                r = Expression.Call(convert, new Expression[] { r });
                //开始运算
                if (t.Value.Equals("+"))
                {
                    v = Expression.Add(v, r);
                }
                else
                {
                    v = Expression.Subtract(v, r);
                }
                t = GetToken();
            }
            _tokens.Add(t);
             return v;
        }

        //乘除项=项 (*|/ 项)
        private Expression Mul()
        {
            Expression v = UnarySub();
            Token t = GetToken();
            while (t.Type == TokenType.Oper && ((string)t.Value == "*" || (string)t.Value == "/" || (string)t.Value == "%"))
            {
                //转换第一个为数字型
                MethodInfo convert = typeof(RunLib).GetMethod("Convert", new Type[] { v.Type });
                v = Expression.Call(convert, new Expression[] { v });
                //转换第二个为数字型
                Expression r = UnarySub();
                convert = typeof(RunLib).GetMethod("Convert", new Type[] { r.Type });
                r = Expression.Call(convert, new Expression[] { r });
                //开始运算
                if (t.Value.Equals("*"))
                {
                    v = Expression.Multiply(v, r);
                }
                else if (t.Value.Equals("/"))
                {
                    v = Expression.Divide(v, r);
                }
                else
                {
                    v = Expression.Modulo(v, r);
                }
                t = GetToken();
            }
            _tokens.Add(t);
            return v;
        }

        //单目运算符
        private Expression UnarySub()
        {
            Token t = GetToken();
            if (t.Type == TokenType.Oper && (string)t.Value == "-")
            {
                Expression r = Item();
                MethodInfo convert = typeof(RunLib).GetMethod("Convert", new Type[] { r.Type });
                r = Expression.Call(convert, new Expression[] { r });
                return Expression.Subtract(Expression.Constant((decimal)0), r);
            }
            _tokens.Add(t);
            return Item();
        }

        //项=对象(.对象路径)*
        private Expression Item()
        {
            Expression retExp = null;
            //获取对象表达式
            object obj;
            Expression objExp = ItemHead(out obj);
            //获取对象路径表达式
            objExp = ObjectPath(false, ref retExp, objExp, obj);
            return objExp;
        }

        //对象=(表达式)|常数|标识符|字符串拼接序列，
        //对象解析过程中，如果是标识符，则要返回找到的对象
        private Expression ItemHead(out object obj)
        {
            obj = null;
            Token t = GetToken();
            if (t.Type == TokenType.Oper && (string)t.Value == "(")
            {
                Expression result = Exp();
                t = GetToken();
                if (t.Type != TokenType.Oper || (string)t.Value != ")")
                {
                    throw new Exception("括号不匹配");
                }
                return result;
            }
            else if (t.Type == TokenType.Oper && (string)t.Value == "$")
            {
                //字符串拼接序列
                Expression strExp = StringUnion();
                return strExp;
            }
            else if (t.Type == TokenType.Int || t.Type == TokenType.Double || t.Type == TokenType.Bool)
            {
                return Expression.Constant(t.Value);
            }
            else if (t.Type == TokenType.Identy)
            {
                return ObjectName((string)t.Value, out obj);
            }
            else if (t.Type == TokenType.Null)
            {
                return Expression.Constant(null);
            }
            throw new Exception(GetExceptionMessage("单词类型错误，" + t));
        }

        //对字符串拼接序列进行解析
        private Expression StringUnion()
        {
            //字符串连接方法
            MethodInfo method = typeof(RunLib).GetMethod("Concat", new Type[]{typeof(object), typeof(object)});
            Expression exp = Expression.Constant("");
            Token t = GetToken();
            //是对象序列
            while ((t.Type == TokenType.Oper && t.Value.Equals("{")) || t.Type == TokenType.String)
            {
                //字符串，返回字符串连接结果
                if (t.Type == TokenType.String)
                {
                    exp = Expression.Call(method, new Expression[]{exp, Expression.Constant(t.Value)});
                }
                else
                {
                    //按表达式调用{}里面的内容
                    Expression objExp = Exp();
                    t = GetToken();
                    if(t.Type != TokenType.Oper || (string)t.Value != "}")
                    {
                        throw new Exception(GetExceptionMessage("缺少'}'"));
                    }
                    //把表达式里的内容转换成字符串
                    MethodInfo convert = objExp.Type.GetMethod("ToString", new Type[] { });
                    objExp = Expression.Call(objExp, convert);
                    exp = Expression.Call(method, new Expression[]{exp, objExp});
                 }
                 t = GetToken();
            }
            _tokens.Add(t);
            return exp;
        }

        //对象路径解析
        //private Expression ObjItem(string objName)
        //{
        //    Expression retExp;
        //    return ObjItem(objName, false, out retExp);
        //}

        //对象路径解析, isLeft 是否赋值语句的左边，赋值语句左边，返回取属性，而不是取属性值
        private Expression ObjItem(string objName, bool isLeft, out Expression retExp)
        {
            retExp = null;
            //根据名称获取对象
            object obj;
            Expression objExp = ObjectName(objName, out obj);
            //执行对象路径
            objExp = ObjectPath(isLeft, ref retExp, objExp, obj);
            return objExp;
        }

        //根据名称获取对象以及对象表达式
        private Expression ObjectName(string objName, out object obj)
        {
            Expression objExp;
            obj = null;

            if (param != null)
            {
                objExp = param;
                //插入.name，以便按属性过程工作
                _tokens.Add(new Token(TokenType.Oper, ".", pos));
                _tokens.Add(new Token(TokenType.Identy, objName, pos));
            }
            //是外面传进来的参数值
            else if (objName.Length >= 5 && objName.Substring(0, 5) == "value")
            {
                objExp = GetOutParam(objName);
            }
            //是数据上下文，数据上下文也当做外面传入的参数
            else if (objName == "data")
            {
                //对象为附加的对象本身
                obj = UI;
                objExp = GetOutParam("value");
            }
            else
            {
                //根据名称获取对象
                obj = UI.FindResource(objName);
                if (obj == null) throw new Exception(GetExceptionMessage("没找到对象 " + objName));
                objExp = Expression.Constant(obj);
            }
            return objExp;
        }

        private Expression ObjectPath(bool isLeft, ref Expression retExp, Expression inExp, object obj)
        {
            Expression propertyExp = null;

            //记录绑定所需要的属性路径的开始位置，prevPos为读最后一个单词之前的位置
            int endPos = -1;
            int startPos = pos;

            //调用条件过滤解析，转换出条件过滤结果
            Expression objExp = ObjPath(inExp);
            Token t = GetToken();
            while (t.Type == TokenType.Oper && (string)t.Value == ".")
            {
                retExp = objExp;
                propertyExp = null;
                //获取对象成员
                Token nameToken = GetToken();
                //继续看是否方法调用，如果是方法调用，执行方法调用过程
                Token n = GetToken();
                if (n.Type == TokenType.Oper && (string)n.Value == "(")
                {
                    //进入方法调用，属性路径结束位置为过滤掉方法调用名称的部分
                    //如果已经过滤掉，就不再重新计算位置
                    if (endPos == -1)
                    {
                        endPos = t.StartPos - 1;
                    }
                    string name = (string)nameToken.Value;
                    objExp = MethodCall(name, objExp, objExp.Type);
                }
                else
                {
                    _tokens.Add(n);
                    //取属性
                    PropertyInfo pi = objExp.Type.NewGetProperty((string)nameToken.Value);
                    //没有找到属性，在编译期无法确定属性，调用运行库的对象取属性方法
                    if (pi == null)
                    {
                        //取属性方法，赋值语句不能取属性值，只能取属性，然后设置值
                        MethodInfo mi = typeof(RunLib).GetMethod("GetProperty", new Type[] { typeof(object), typeof(string) });
                        propertyExp = Expression.Call(mi, new Expression[] { objExp, Expression.Constant((string)nameToken.Value) });
                        mi = typeof(RunLib).GetMethod("GetPropertyValue", new Type[] { typeof(object), typeof(string) });
                        objExp = Expression.Call(mi, new Expression[] { objExp, Expression.Constant((string)nameToken.Value) });
                    }
                    //如果是我们扩充出来的属性，调用对象的GetProperty方法
                    else if (pi is CustomPropertyInfoHelper)
                    {
                        //取属性方法，赋值语句不能取属性值，只能取属性，然后设置值
                        MethodInfo mi = (objExp.Type as CustomType).InnerType.GetMethod("GetProperty", new Type[] { typeof(string) });
                        propertyExp = Expression.Call(objExp, mi, new Expression[] { Expression.Constant((string)nameToken.Value) });
                        mi = (objExp.Type as CustomType).InnerType.GetMethod("GetPropertyValue", new Type[] { typeof(string) });
                        objExp = Expression.Call(objExp, mi, new Expression[] { Expression.Constant((string)nameToken.Value) });
                    }
                    else
                    {
                        objExp = Expression.Property(objExp, pi);
                    }
                    //调用条件过滤解析，产生条件过滤结果
                    objExp = ObjPath(objExp);
                }
                t = GetToken();
            }
            //如果是赋值语句，且有属性表达式，返回属性表达式
            if (isLeft && propertyExp != null)
            {
                objExp = propertyExp;
            }
            //如果后面指定了=>，说明是事件触发，不调用绑定，登记事件，以便解析后，监控事件
            if (t.Type == TokenType.Oper && (string)t.Value == "=>")
            {
                t = GetToken();
                if (t.Type != TokenType.Identy) throw new Exception("=>后面必须跟事件名称");
                EventInfo ei = obj.GetType().GetEvent((string)t.Value);
                if (ei == null)
                {
                    throw new Exception(obj.GetType().ToString() + "没有" + t.Value.ToString() + "事件");
                }
                Events.Add(new ObjectEvent() { Object = obj, Event = ei });
                return objExp;
            }
            //如果没有结束位置，结束位置为最后一个单词之前的位置
            if (endPos == -1)
            {
                endPos = t.StartPos - 1;
            }
            //如果开始位置为"."，读掉之
            if (startPos < Source.Length && Source[startPos] == '.')
            {
                startPos++;
            }
            //只有在属性赋值时，才建立绑定过程，包括依赖属性。集合参数状态下，不能绑定
            if (IsBinding && param == null && endPos > startPos)
            {
                string str = Source.Substring(startPos, endPos - startPos + 1);
                //如果是对象本身，路径添加DataContext
                if (obj == UI && (obj is System.Windows.FrameworkElement || obj is PropertySetter))
                {
                    str = "DataContext." + str;
                }
                SetBinding(obj, str);
            }
            _tokens.Add(t);
            return objExp;
        }

        //对象单个路径=属性名([条件])?
        private Expression ObjPath(Expression objExp)
        {
            Token n = GetToken();
            //是条件判断
            if (n.Type == TokenType.Oper && (string)n.Value == "[")
            {
                //对条件进行编译，编译前保存旧的参数环境
                List<ParameterExpression> oldParams = new List<ParameterExpression>(outParams);
                Expression exp = Exp();
                //读掉']'
                n = GetToken();
                if (n.Type != TokenType.Oper || (string)n.Value != "]")
                {
                    throw new Exception(GetExceptionMessage("缺少']'"));
                }
                Delegate subdel = Expression.Lambda(exp, outParams.ToArray<ParameterExpression>()).Compile();
                //恢复旧参数 outParams.Clear();
                outParams = oldParams;
                //产生条件过滤调用函数
                MethodInfo mi = typeof(RunLib).GetMethod("Where", new Type[] { typeof(IEnumerable<object>), typeof(Delegate) });
                Expression result = Expression.Call(mi, new Expression[] { objExp, Expression.Constant(subdel) });
                return result;
            }
            _tokens.Insert(0, n);
            return objExp;
        }

        #region 建立绑定，把绑定到的对象存起来，在表达式解析完成后，外面再监听这些对象的值变化
        
        //保存已经建立过的绑定信息，已经建立的绑定，不再重复建立
        List<BindInfo> binds = new List<BindInfo>();
        class BindInfo
        {
            public object Object { get; set; }
            public string Path { get; set; }
            public override bool Equals(object obj)
            {
                BindInfo other = (BindInfo)obj;
                return this.Object == other.Object && this.Path == other.Path;
            }
        }

        //建立绑定，把绑定到的对象存起来，在表达式解析完成后，外面再监听这些对象的值变化
        private void SetBinding(object o, string path)
        {
            //条件部分不包括
            if (path != null)
            {
                int index = path.IndexOf("[");
                if (index != -1)
                {
                    path = path.Substring(0, index);
                }
            }
            //如果建立过绑定，不重复建立
            BindInfo info = new BindInfo() {Object = o, Path = path};
            if (!this.binds.Contains(info))
            {
                //把绑定信息存起来，以便检查
                binds.Add(info);
                //建立绑定关系
                Binding b = new Binding();
                b.Source = o;
                b.Path = new System.Windows.PropertyPath(path);
                BindingSlave bs = new BindingSlave();
                BindingOperations.SetBinding(bs, BindingSlave.ValueProperty, b);
                Bindings.Add(bs);
            }
        }
        #endregion

        //函数调用
        private Expression MethodCall(string name, Expression obj, Type inType)
        {
            //我们自己扩充的类型，方法调用不好用，直接用基类的
            if (inType is CustomType)
            {
                inType = (inType as CustomType).InnerType;
            }
            Token t = GetToken();
            //没有参数，调用无参处理过程
            if (t.Type == TokenType.Oper && (string)t.Value == ")")
            {
                //获取方法
                MethodInfo mi = inType.GetMethod(name, new Type[] {});
                if (mi != null)
                {
                    return Expression.Call(obj, mi);
                }
                //从系统库取
                mi = typeof(RunLib).GetMethod(name, new Type[]{inType});
                if (mi != null)
                {
                    return Expression.Call(mi, new Expression[]{obj});
                }
                //都没有，采用反射调用对象方法
                mi = typeof(RunLib).GetMethod("NewMethodCall", new Type[] { typeof(object), typeof(string) });
                return Expression.Call(mi, new Expression[] { obj, Expression.Constant(name) });
            }
            else
            {
                _tokens.Add(t);
                Expression result = null;
                //如果是集合，取集合处理函数
                if ((typeof(IEnumerable).IsAssignableFrom(obj.Type) || typeof(IEnumerable<object>).IsAssignableFrom(obj.Type))
                    && name.Equals("Remove"))
                {
                    Expression[] ps = Params();
                    var types = (from p in ps select p.Type);
                    MethodInfo mi = inType.GetMethod(name, types.ToArray());
                    result = Expression.Call(obj, mi, ps.ToArray());
                }
                //集合函数不以"_"结束，不想当做集合处理的函数，用"_"结束
                else  if ((typeof(IEnumerable).IsAssignableFrom(obj.Type) || typeof(IEnumerable<object>).IsAssignableFrom(obj.Type))
                    && !name.EndsWith("_"))
                {
                    param = Expression.Parameter(typeof(object));
                    //对于Each函数，要调用处理过程
                    Expression exp = null;
                    if (name == "Each")
                    {
                        exp = Prog();
                    }

                    else
                    {
                        exp = Exp();
                    }
                    //清空参数表达式，转入正常的表达式处理过程
                    Delegate subdel = Expression.Lambda(exp, new ParameterExpression[] { param }).Compile();
                    param = null;
                    //类型第一个为列表本身，第二项为每一项表达式，以后各项为集合的操作参数
                    List<Type> types = new List<Type>();
                    if (typeof(IEnumerable<object>).IsAssignableFrom(obj.Type))
                    {
                        types.Add(typeof(IEnumerable<object>));
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(obj.Type))
                    {
                        types.Add(typeof(IEnumerable));
                    }
                    types.Add(typeof(Delegate));
                    //生成表达式列表
                    List<Expression> exps = new List<Expression>();
                    exps.Add(obj);
                    exps.Add(Expression.Constant(subdel));
                    //如果还有参数，调用参数过程，获取其他参数                    
                    t = GetToken();
                    if (t.Type == TokenType.Oper && (string)t.Value == ",")
                    {
                        Expression[] ps = Params();
                        types.AddRange((from p in ps select p.Type));
                        exps.AddRange(ps);
                    }
                    else
                    {
                        _tokens.Add(t);
                    }
                    MethodInfo mi = typeof(RunLib).GetMethod(name, types.ToArray());
                    result = Expression.Call(mi, exps.ToArray());
                }
                else
                {
                    Expression[] ps = Params();
                    var types = (from p in ps select p.Type);
                    MethodInfo mi = inType.GetMethod(name, types.ToArray());
                    //本身不存在这个函数，去系统运行库找
                    if (mi == null)
                    {
                        //系统库中的静态方法，头一个参数为对象本身
                        List<Type> list = types.ToList();
                        list.Insert(0, inType);
                        mi = typeof(RunLib).GetMethod(name, list.ToArray());
                        //系统库也没有，抛出异常
                        if (mi == null)
                        {
                            //都没有，采用反射调用对象方法
                            mi = typeof(RunLib).GetMethod("NewMethodCall", new Type[] { typeof(object), typeof(string), typeof(object[]) });
                            result = Expression.Call(mi, new Expression[] { obj, Expression.Constant(name), Expression.NewArrayInit(typeof(object), ps)});
                        }
                        else
                        {
                            //系统库中调用时，头一个参数为对象本身
                            List<Expression> objs = ps.ToList();
                            objs.Insert(0, obj);
                            result = Expression.Call(mi, objs);
                        }
                    }
                    else
                    {
                        result = Expression.Call(obj, mi, ps.ToArray());
                    }
                }
                t = GetToken();
                if (t.Type != TokenType.Oper || (string)t.Value != ")")
                {
                    throw new Exception(GetExceptionMessage("函数调用括号不匹配"));
                }
                return result;
            }
        }

        //函数参数列表
        private Expression[] Params()
        {
            List<Expression> ps = new List<Expression>();
            Expression exp = Exp();
            ps.Add(exp);
            Token t = GetToken();
            while (t.Type == TokenType.Oper && (string)t.Value == ",")
            {
                exp = Exp();
                ps.Add(exp);
                t = GetToken();
            }
            _tokens.Add(t);
            return ps.ToArray();
        }

        //获取异常信息输出
        private string GetExceptionMessage(string msg)
        {
            //对出错的语句位置进行处理
            string result = Source.Substring(0, pos) + " <- " + Source.Substring(pos, Source.Length - pos);
            return msg + ", " + result;
        }

        //获取单词
        public Token GetToken()
        {
            //如果队列里有，直接取上次保存的
            if (_tokens.Count != 0)
            {
                var result = _tokens[0];
                _tokens.RemoveAt(0);
                return result;
            }
            //记录单词的起始位置，包括空格等内容
            int sPos = pos;
            //如果是字符串处理状态，把除过特殊字符的所有字符全部给字符串
            if (inString)
            {
                int startPos = pos;
                //在特殊情况下，字符串要使用$结束
                while (pos < Source.Length && Source[pos] != '{' && Source[pos] != '$'  && Source[pos] != '}')
                {
                    pos++;
                }
                //脱离字符串操作环节，保留原来字符串操作环节状态
                if (pos < Source.Length && Source[pos] == '{')
                {
                    inStrings.Insert(0, inString);
                    inString = false;
                }
                //其他字符退出字符串处理环节，回到上一环节
                else
                {
                    inString = inStrings[0];
                    inStrings.RemoveAt(0);
                }
                Token t = new Token(TokenType.String, Source.Substring(startPos, pos - startPos), sPos);
                //如果是采用$让字符串结束了，要把$读去
                if (pos < Source.Length && Source[pos] == '$') pos++;
                return t;
            }
            //读去所有空白
            while (pos < Source.Length && Source[pos] == ' ')
            {
                pos++;
            }
            //如果完了，返回结束
            if (pos == Source.Length)
            {
                return new Token(TokenType.End, null, sPos);
            }
            //如果是数字，循环获取直到非数字为止
            if (Source[pos] >= '0' && Source[pos] <= '9')
            {
                int oldPos = pos;
                while (pos < Source.Length && Source[pos] >= '0' && Source[pos] <= '9')
                {
                    pos++;
                }
                //如果后面是"."，按double数字对待，否则按整形数返回
                if (pos < Source.Length && Source[pos] == '.')
                {
                    pos++;
                    while (pos < Source.Length && Source[pos] >= '0' && Source[pos] <= '9')
                    {
                        pos++;
                    }
                    //位置还原，以便下次读取
                    string str = Source.Substring(oldPos, pos - oldPos);
                    return new Token(TokenType.Double, double.Parse(str), sPos);
                }
                else
                {
                    string str = Source.Substring(oldPos, pos - oldPos);
                    return new Token(TokenType.Int, int.Parse(str), sPos);
                }
            }
            //如果是字符，按标识符对待
            else if ((Source[pos] >= 'a' && Source[pos] <= 'z') || (Source[pos] >= 'A' && Source[pos] <= 'Z')  || Source[pos] == '_')
            {
                int oldPos = pos;
                while (pos < Source.Length && ((Source[pos] >= 'a' && Source[pos] <= 'z') 
                    || (Source[pos] >= 'A' && Source[pos] <= 'Z') || (Source[pos] >= '0' && Source[pos] <= '9') || Source[pos] == '_'))
                {
                    pos++;
                }
                string str = Source.Substring(oldPos, pos - oldPos);
                //是bool常量
                if (str == "False" || str == "True")
                {
                    return new Token(TokenType.Bool, bool.Parse(str), sPos);
                }
                if (str == "null")
                {
                    return new Token(TokenType.Null, null, sPos);
                }
                return new Token(TokenType.Identy, str, sPos);
            }
            //+、-、*、/、>、<、！等后面可以带=的处理
            else if (Source[pos] == '+' || Source[pos] == '-' || Source[pos] == '*' || Source[pos] == '/' || Source[pos] == '%'
                || Source[pos] == '>' || Source[pos] == '^' || Source[pos] == '!')
            {
                //后面继续是'='，返回双操作符，否则，返回单操作符
                if (pos < Source.Length && Source[pos + 1] == '=')
                {
                    string str = Source.Substring(pos, 2);
                    pos += 2;
                    return new Token(TokenType.Oper, str, sPos);
                }
                else
                {
                    string str = Source.Substring(pos, 1);
                    pos += 1;
                    return new Token(TokenType.Oper, str, sPos);
                }
            }
            //=号开始有三种，=本身，==，=>
            else if (Source[pos] == '=')
            {
                if (pos < Source.Length && (Source[pos + 1] == '=' || Source[pos + 1] == '>'))
                {
                    string str = Source.Substring(pos, 2);
                    pos += 2;
                    return new Token(TokenType.Oper, str, sPos);
                }
                else
                {
                    string str = Source.Substring(pos, 1);
                    pos += 1;
                    return new Token(TokenType.Oper, str, sPos);
                }
            }
            //单个操作符
            else if (Source[pos] == '(' || Source[pos] == ')' || Source[pos] == ',' || Source[pos] == ';'
                || Source[pos] == '.' || Source[pos] == ':' || Source[pos] == '@' || Source[pos] == '$'
                || Source[pos] == '{' || Source[pos] == '}' || Source[pos] == '[' || Source[pos] == ']')
            {
                //进入字符串处理环节，保留原来状态
                if (Source[pos] == '$')
                {
                    inStrings.Insert(0, inString);
                    inString = true;
                }
                //再次进入原来的环节
                if (Source[pos] == '}' && inStrings.Count != 0)
                {
                    inString = inStrings[0];
                    inStrings.RemoveAt(0);
                }
                string str = Source.Substring(pos, 1);
                pos += 1;
                return new Token(TokenType.Oper, str, sPos);
            }
            else
            {
                throw new Exception(GetExceptionMessage("无效单词"));
            }
        }
    }

    public class Token
    {
        //Token类型
        public TokenType Type { get; set; }

        //Token值
        public object Value { get; set; }

        //单词在串中的起始位置，包括空格
        public int StartPos { get; set; }

        public Token(TokenType type, object value, int startPos)
        {
            Type = type;
            Value = value;
            StartPos = startPos;
        }

        public override string ToString()
        {
            return Type.ToString() + Value;
        }
    }

    public enum TokenType
    {
        Int, Double, Bool, String, Identy, Oper, End, Null
    }

    public class ObjectEvent
    {
        public object Object;
        public EventInfo Event;
    }
}

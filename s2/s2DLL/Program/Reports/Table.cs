using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Com.Aote.ObjectTools;
using System.Collections;
using Com.Aote.Marks;
using Com.Aote.Utils;
using System.ComponentModel;

namespace Com.Aote.Reports
{
    public class Table : Control,INotifyPropertyChanged
    {
        public Table()
        {
            this.DefaultStyleKey = typeof(Table);
            this.Loaded += new RoutedEventHandler(Table_Loaded);
        }

        void Table_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }


        public WebClientInfo WebClientInfo { get; set; }

        #region SumNames 求和字段名称，以","分隔
        private string sumnames = ",";
        /// <summary>
        /// 在开始加载总体信息时，要进行求和的字段名称。以","分隔
        /// </summary>
        public String SumNames 
        {
            get { return this.sumnames; }
            set { this.sumnames = value; }
        }
        #endregion

        #region Count 总共数据个数
        /// <summary>
        /// 总数据个数，在加载总体信息时获得，并赋值。
        /// </summary>
        private int count = -1;
        public int Count
        {
            get
            {
                if (count < 0) return 0;
                return count;
            }
            set
            {
                if (count != value)
                {
                    count = value;
                    OnPropertyChanged("Count");
                }
            }
        }
        #endregion

        /// <summary>
        /// 是否正忙于工作
        /// </summary>
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

        private string filename = "";
        /// <summary>
        /// 报表文件名称，加后缀
        /// </summary>
        public String FileName 
        {
            get { return this.filename; }
            set
            {
                this.filename = value;
            }
        }


        #region ExcelFileName 产生的Excel文件名
        private string excelfilename;
        public string ExcelFileName
        {
            get { return excelfilename; }
            set
            {
                if (excelfilename == value)
                {
                    return;
                }
                excelfilename = value;
                OnPropertyChanged("ExcelFileName");
            }
        }
        #endregion

        private string tablejson;
        /// <summary>
        /// 报表转成json字符串属性，可以用于保存到数据库
        /// </summary>
        public string TableToJson 
        {
            get { return this.tablejson; }
            set
            {
                this.tablejson = value;
                OnPropertyChanged("TableToJson");
            }
        }

        /// <summary>
        /// 把保存的json字符串转成报表显示
        /// </summary>
        public string JsonToTable
        {
            set
            {
                if(value!=null && value!="")
                {
                    jsonToTable(value);
                }
            }
        }

        #region PageIndex 当前页
        /// <summary>
        /// 当前页号，默认为-1，当页号发生变化后，重新加载某页数据。
        /// </summary>
        protected int pageIndex = -1;
        public int PageIndex
        {
            get { return pageIndex; }
            set
            {
                //if (pageIndex != value)
                //{
                    pageIndex = value;
                    //当前页改变，重新加载详细数据，pageIndex小于0时不加载
                    //if (pageIndex >= 0)
                    //{
                        LoadDetail();
                    //}
                //}
            }
        }
        #endregion

        #region PageSize 每页行数
        private int pagesize = 1000;
        /// <summary>
        /// 每页行数,默认100
        /// </summary>
        public int PageSize 
        {
            get { return this.pagesize; }
            set { this.pagesize = value; }
        }
        #endregion

        
        private bool initload = false;
        /// <summary>
        /// 进入页面是否加载，默认不加载
        /// </summary>
        public bool InitLoad 
        {
            set 
            { 
                this.initload = value;
                if (value)
                {
                    Start();
                }
            }
            get { return this.initload; }
        }

        private string condition = "";
        /// <summary>
        /// 查询条件
        /// </summary>
        public string Condition
        {
            get { return this.condition; }
            set
            {
                if (value == null || value == "") return;
                this.condition = value;
                //开始生成报表
                Start();
            }
        }
        /// <summary>
        /// 是否有表头数据，默认没有
        /// </summary>
        private bool HasHead = false;
        /// <summary>
        /// 是否有左侧数据，默认没有
        /// </summary>
        private bool HasLeft = false;

        /// <summary>
        /// 主体数据
        /// </summary>
        private List<BodyDatas> tablebodyitems = new List<BodyDatas>();
        private List<BodyDatas> TableBodyItems 
        {
            get { return this.tablebodyitems; }
            set
            {
                this.tablebodyitems = value;
            }
        }

        
        /// <summary>
        /// 左侧数据
        /// </summary>
        private BodyDatas tableleftitems = new BodyDatas();
        private BodyDatas TableLeftItems 
        {
            get { return this.tableleftitems; }
            set { this.tableleftitems = value; }
        }

        /// <summary>
        /// 表头数据
        /// </summary>
        private BodyDatas tableheaditems = new BodyDatas();
        private BodyDatas TableHeadItems
        {
            get { return this.tableheaditems; }
            set { this.tableheaditems = value; }
        }
        /// <summary>
        /// 当前页面使用的表头资源
        /// </summary>
        private GeneralObject CurrentHead { get; set; }
        /// <summary>
        /// 当前页面使用的左侧资源
        /// </summary>
        private GeneralObject CurrentLeft { get; set; }
        /// <summary>
        /// 当前页面使用的主体资源
        /// </summary>
        private GeneralObject CurrentMain { get; set; }

        /// <summary>
        /// 表头行单元格
        /// </summary>
        private List<Row> headRowTemplate = new List<Row>();
        /// <summary>
        /// 表底行单元格
        /// </summary>
        private List<Row> bottomRowTemplate = new List<Row>();
        /// <summary>
        /// 表头单元格
        /// </summary>
        private List<Cell> headCellTemplate = new List<Cell>();
        /// <summary>
        /// 表头变化单元格
        /// </summary>
        private List<Cell> headChangeCellTemplate = new List<Cell>();
        /// <summary>
        /// 左侧单元格模板
        /// </summary>
        private List<Cell> leftCellTemplate = new List<Cell>();
        /// <summary>
        /// 主体单元格模板
        /// </summary>
        private List<Cell> bodyCellTemplate = new List<Cell>();
        /// <summary>
        /// 表底主体单元格模板
        /// </summary>
        private List<Cell> bottomCellTemplate = new List<Cell>();

       
        //报表对应的Uri
        private string source;
        public string Source
        {
            get { return source; }
            set
            {
                if (this.source == value)
                {
                    return;
                }
                this.source = value;
            }
        }
        

        /// <summary>
        /// 加载文件，获得所有需要执行的sql
        /// </summary>
        private void Init()
        {
            string uri = WebClientInfo.BaseAddress + "/" + FileName;
            //根据uri，去后台获取数据
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (o, a) =>
            {
                IsBusy = false;
                if (a.Error != null)
                {
                    MessageBox.Show("加载报表文件失败！");
                }
                else
                {
                    JsonObject item = JsonValue.Parse(a.Result) as JsonObject;
                    //清除原来的数据
                    this.Clear();
                    //取出所有列定义
                    JsonArray columns = item["columns"] as JsonArray;
                    foreach (JsonObject obj in columns)
                    {
                        int width = obj["width"];
                        Column column = new Column() { Width = width };
                        this.columns.Add(column);
                    }
                    //主体sql
                    JsonArray sqls = item["sqls"] as JsonArray;
                    for (int i = 0; i < sqls.Count; i++)
                    {
                        JsonObject obj = sqls[i] as JsonObject;
                        string name = obj["name"];
                        BodyDatas bd = new BodyDatas(name, obj["sql"], new ObjectList());
                        TableBodyItems.Add(bd);
                    }
                    //表头sql
                    string headsql="";
                    if (item.ContainsKey("headsql")) { headsql = item["headsql"]; }
                    if (headsql != "")
                    {
                        HasHead = true;
                        TableHeadItems.Name = "head";
                        TableHeadItems.Sql = headsql;
                        TableHeadItems.Value = new ObjectList();
                    }
                    //左侧sql
                    string leftsql = "";
                    if (item.ContainsKey("leftsql")) { leftsql = item["leftsql"]; }
                    if (leftsql != "")
                    {
                        HasLeft = true;
                        TableLeftItems.Name = "left";
                        TableLeftItems.Sql = leftsql;
                        TableLeftItems.Value = new ObjectList();
                    }
                    //加载所有单元格模板
                    InitTemplate(item);
                }
            };
            IsBusy = true;
            client.DownloadStringAsync(new Uri(uri));
        }

        /// <summary>
        /// 根据名称获得sql，左侧数据name为left
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetSql(string name)
        {
            //表头数据
            if (name == "head") return TableHeadItems.Sql;
            //左侧数据
            if (name == "left") return TableLeftItems.Sql ;
            for (int i = 0; i < TableBodyItems.Count; i++)
            {
                BodyDatas bd = TableBodyItems[i];
                if (name == bd.Name)
                {
                    return bd.Sql;
                }
            }
            MessageBox.Show("没有找到name为" + name + "的sql！");
            return null;
        }
        /// <summary>
        /// 根据name，设置新的sql，如果原来没有这个name，添加新的sql对象
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sql"></param>
        public void PutSql(string name, string sql)
        {
            if (name == "head") { TableHeadItems.NewSql = sql; return; }
            if (name == "left") 
            { 
                TableLeftItems.NewSql = sql; 
                return; 
            }
            for (int i = 0; i < TableBodyItems.Count; i++)
            {
                BodyDatas bd = TableBodyItems[i];
                if (name == bd.Name)
                {
                    bd.NewSql = sql;
                    return;
                }
                else
                {
                    //如果没有找到name，添加新的sql对象
                    BodyDatas newbd = new BodyDatas();
                    newbd.Name = name;
                    newbd.NewSql = sql;
                    TableBodyItems.Add(newbd);
                }
            }
        }
        /// <summary>
        /// 获得所有sql的json格式，包括左侧sql
        /// </summary>
        /// <returns></returns>
        private string getJsonSql()
        {
            string sql = "";
            string result="";
            if (HasLeft)
            {
                if (TableLeftItems.NewSql != null)
                {
                    sql = TableLeftItems.NewSql;
                }
                else
                {
                    sql = TableLeftItems.Sql;
                }
                sql = sql.Replace("$", "'");
                //sql执行表达式
                Program prog = new Program("$" + sql + "$", this, false);
                sql = prog.Parse(prog.Exp).DynamicInvoke() + "";
                result = "{name:'" + TableLeftItems.Name + "',sql:'" + sql.Replace("'", "$") + "'},";
            }
            if (HasHead)
            {
                if (TableHeadItems.NewSql != null)
                {
                    sql = TableHeadItems.NewSql;
                }
                else
                {
                    sql = TableHeadItems.Sql;
                }
                sql = sql.Replace("$", "'");
                //sql执行表达式
                Program prog = new Program("$" + sql + "$", this, false);
                sql = prog.Parse(prog.Exp).DynamicInvoke() + "";
                result = result + "{name:'" + TableHeadItems.Name + "',sql:'" + sql.Replace("'", "$") + "'},";
            }
            for (int i = 0; i < TableBodyItems.Count; i++)
            {
                BodyDatas bd = TableBodyItems[i];
                sql = "";
                if (bd.NewSql != null)
                {
                    sql = bd.NewSql;
                }
                else
                {
                    sql = bd.Sql;
                }
                sql = sql.Replace("$", "'");
                //sql执行表达式
                Program prog = new Program("$" + sql + "$", this, false);
                sql = prog.Parse(prog.Exp).DynamicInvoke() + "";
                result = result + "{name:'" + bd.Name + "',sql:'" + sql.Replace("'", "$") + "'},";
            }
            result = "[" + result.Substring(0, result.Length - 1) + "]";
            //如果有条件，替换条件
            if (Condition != null && Condition != "")
            {
                //条件中的单引号替换成$,供后台解析
                string condition = Condition.Replace("'", "$");
                result = result.Replace("#condition#", condition);  
            }
            return result;
        }

        /// <summary>
        /// 加载所有数据
        /// </summary>
        public void Start()
        {
            //如果有左侧不加载总数
            if (HasLeft || HasHead)
            {
                PageIndex = 0;
            }
            else
            {
                Load();
                PageIndex = 0;
            }
        }

        /// <summary>
        /// 加载总数
        /// </summary>
        private void Load()
        {
            WebClient client = new WebClient();
            string str = WebClientInfo.BaseAddress + "/sql/" + SumNames;
            Uri uri = new Uri(str);
            string sqls = getJsonSql();
            client.UploadStringCompleted += (o, a) =>
            {
                if (a.Error != null)
                {
                    //MessageBox.Show("加载数据总数失败！");
                    return;
                }
                //更新数据
                JsonObject item = JsonValue.Parse(a.Result) as JsonObject;
                Count = item["Count"];
            };
            client.UploadStringAsync(uri, sqls);
        }

        /// <summary>
        /// 加载每页数据
        /// </summary>
        private void LoadDetail()
        {
            //如果有左侧不加载总数
            string uri = WebClientInfo.BaseAddress + "/sql/" + PageIndex + "/" + PageSize;
            string sqls = getJsonSql();
            WebClient client = new WebClient();
            client.UploadStringCompleted += (o, a) =>
            {
                IsBusy = false;
                if (a.Error != null || a.Result==null)
                {
                    MessageBox.Show("加载报表数据失败！");
                    return;
                }
                JsonObject item = JsonValue.Parse(a.Result) as JsonObject;
                if (HasLeft)
                {
                    JsonArray array = item["left"] as JsonArray;
                    TableLeftItems.Value.FromJson(array);
                }
                if (HasHead)
                {
                    JsonArray array = item["head"] as JsonArray;
                    TableHeadItems.Value.FromJson(array);
                }
                for (int i = 0; i < TableBodyItems.Count; i++)
                {
                    BodyDatas bd = TableBodyItems[i];
                    JsonArray array = item[bd.Name] as JsonArray;
                    bd.Value.FromJson(array);
                }
                //画界面
                UpdateElement();
            };
            IsBusy = true;
            client.UploadStringAsync(new Uri(uri), sqls);
        }
       
        /// <summary>
        /// 清除所有数据
        /// </summary>
        public void Clear()
        {
            HasLeft = false;
            HasHead = false;
            this.cells.Clear();
            this.columns.Clear();
            this.rows.Clear();
            TableBodyItems.Clear();
            TableLeftItems = new BodyDatas();
            TableHeadItems = new BodyDatas();
            bodyCellTemplate.Clear();
            leftCellTemplate.Clear();
            headCellTemplate.Clear();
            headChangeCellTemplate.Clear();
        }

       

        private void UpdateElement()
        {
            this.cells.Clear();
            this.rows.Clear();
            //绘制表头
            UpdateHead();
            //有左侧
            UpdateLeft();
            UpdateBody();
            //绘制表底
            UpdateBottom();
            this.Layout();
        }

       

        /// <summary>
        /// 获得模板
        /// </summary>
        /// <param name="item"></param>
        private void InitTemplate(JsonObject item)
        {
            //取出表头行定义
            JsonArray rows = item["rows"] as JsonArray;
            foreach (JsonObject obj in rows)
            {
                int height = obj["height"];
                string type = obj["type"];
                if (type == "head")
                {
                    Row row = new Row() { Height = height };
                    this.headRowTemplate.Add(row);
                }
                if (type == "bottom")
                {
                    Row row = new Row() { Height = height };
                    this.bottomRowTemplate.Add(row);
                }
            }
            //获得模板
            JsonArray cells = item["cells"] as JsonArray;
            foreach (JsonObject obj in cells)
            {
                int row = obj["row"];
                int column = obj["column"];
                string content = obj["content"];
                int rowspan = obj["rowspan"];
                int columnspan = obj["columnspan"];
                string location = obj["location"];
                int height = obj["height"];
                Cell cell = new Cell() { Row = row, Column = column, Content = content, RowSpan = rowspan, ColumnSpan = columnspan, Location = location, Height = height };
                string type = obj["type"];
                //表头
                if (type == "head" || type == "headchange")
                {
                    this.headCellTemplate.Add(cell);
                }
                //表头变化部分
                if (type == "headchange")
                {
                    this.headChangeCellTemplate.Add(cell);
                }
                //左侧
                if (type == "left")
                {
                    Program prog = new Program("data."+cell.Content, this, false);
                    Delegate d = prog.Parse(prog.Exp);
                    cell.Delegate = d;
                    this.leftCellTemplate.Add(cell);
                }
                //主体
                if (type == "main")
                {
                    this.bodyCellTemplate.Add(cell);
                }
                //表底
                if (type == "bottom")
                {
                    this.bottomCellTemplate.Add(cell);
                }
            }
           
        }

       
        

        /// <summary>
        /// 如果有左侧，根据左侧数据执行主体界面
        /// 如果没有左侧，直接执行主体界面
        /// </summary>
        public void UpdateLeft()
        {
            if (TableLeftItems.Value.Count == 0) return;
            //获得左侧模板包含几行
            int count = getRowNumber(leftCellTemplate);
            //如果有左侧数据，需要根据左侧同时画主体
            for (int i = 0; i < TableLeftItems.Value.Count; i++)
            {
                //画左侧
                foreach (Cell cell in leftCellTemplate)
                {
                    object value = cell.Delegate.DynamicInvoke(new object[] { TableLeftItems.Value[i] });
                    cell.Content = value + "";
                    Cell copy = cell.copyCell();
                    copy.Row = cell.Row + i * count;
                    this.cells.Add(copy);
                }
                //for (int j = 0; j < count; j++)
                //{
                //    //根据模板加一行
                //    Row row = new Row() { Height = leftCellTemplate[0].Height };
                //    this.rows.Add(row);
                //}
            }
        }


       

        /// <summary>
        /// 根据表头，重新绘制，如果有表达式则执行表达式
        /// </summary>
        private void UpdateHead()
        {
            if (TableHeadItems.Value.Count == 0 && TableBodyItems.Count == 0) return;
            //绘制有表头变化部分的表头
            if (HasHead)
            {
                //获得表头变化模板包含几列
                Dictionary<string, int> dic = getColumnNumber(headChangeCellTemplate);
                //根据表头sql数据进行复制
                for (int i = 0; i < TableHeadItems.Value.Count; i++)
                {
                    //画表头
                    foreach (Cell cell in headCellTemplate)
                    {
                        Cell copy = cell.copyCell();
                        Program prog = new Program("data." + copy.Content, this, false);
                        Delegate d = prog.Parse(prog.Exp);
                        copy.Delegate = d;
                        //单元格为表头变化部分
                        if (this.headChangeCellTemplate.Contains(cell))
                        {
                            object value = copy.Delegate.DynamicInvoke(new object[] { TableHeadItems.Value[i] });
                            if (value == null) { value = copy.Content.Replace("$", ""); }
                            copy.Content = value + "";
                            copy.Row = cell.Row;
                            //根据表头变化部分模板复制单元格，横向复制
                            copy.Column = cell.Column + i * dic["count"];
                            this.cells.Add(copy);
                        }
                        //在变化部分左侧的，直接设置单元格，执行表达式
                        else if (cell.Column < dic["min"]) 
                        {
                            copy.Content = copy.Content.Replace("$", "");
                            copy.Row = cell.Row; this.cells.Add(copy); 
                        }
                        //在变化部分的右侧单元格，
                        else if (cell.Column > dic["max"])
                        {
                            copy.Content = copy.Content.Replace("$", "");
                            copy.Row = cell.Row;
                            //根据表头变化部分模板复制单元格，横向复制
                            copy.Column = cell.Column + (TableHeadItems.Value.Count-1) * dic["count"];
                            this.cells.Add(copy);
                        }
                    }
                    //添加列,根据表头变化部分模板横向复制
                    for (int j = 0; j < i * dic["count"]; j++)
                    {
                        Column column = new Column() { Width = columns[0].Width };
                        this.columns.Add(column);
                    }
                }
            }
            //绘制普通表头，执行表达式
            else
            {
                foreach (Cell cell in headCellTemplate)
                {
                    Cell copy = cell.copyCell();
                    Program prog = new Program(copy.Content, this, false);
                    Delegate d = prog.Parse(prog.Exp);
                    copy.Delegate = d;
                    object value = copy.Delegate.DynamicInvoke();
                    copy.Content = value + "";
                    copy.Row = cell.Row;
                    this.cells.Add(copy);
                }
            }
            //复制表头行
            this.rows.AddRange(headRowTemplate);
        }


        private void UpdateBottom()
        {
            if (TableBodyItems[0].Value.Count == 0) return;
            foreach (Cell cell in bottomCellTemplate)
            {
                Cell copy = cell.copyCell();
                Program prog = new Program(copy.Content, this, false);
                Delegate d = prog.Parse(prog.Exp);
                copy.Delegate = d;
                object value = copy.Delegate.DynamicInvoke();
                copy.Content = value + "";
                copy.Row = this.rows.Count - this.headRowTemplate.Count + copy.Row - 1;
                this.cells.Add(copy);
            }
            //复制表行
            this.rows.AddRange(bottomRowTemplate);
        }


        #region 根据主体数据生成界面
        /// <summary>
        /// 根据主体数据生成界面
        /// </summary>
        private void UpdateBody()
        {
            if (TableHeadItems.Value.Count == 0 
                && TableLeftItems.Value.Count == 0 
                && TableBodyItems.Count == 0) return;
            //绘制有表头变化,有左侧的主体
            if (HasHead && HasLeft)
            {
                //获得表头变化模板包含几列
                Dictionary<string, int> dic = getColumnNumber(headChangeCellTemplate);
                for (int i = 0; i < TableHeadItems.Value.Count; i++)
                {
                    CurrentHead = TableHeadItems.Value[i];
                    for (int j = 0; j < TableLeftItems.Value.Count; j++)
                    {
                        CurrentLeft = TableLeftItems.Value[j];
                        //画主体
                        foreach (Cell cell in bodyCellTemplate)
                        {
                            Cell copy = cell.copyCell();
                            Program prog = new Program(copy.Content, this, false);
                            Delegate d = prog.Parse(prog.Exp);
                            copy.Delegate = d;
                            object value = copy.Delegate.DynamicInvoke();
                            copy.Content = value + "";
                            copy.Row = cell.Row + j;
                            if (copy.Column > dic["max"]) { copy.Column = copy.Column + (TableHeadItems.Value.Count - 1) * dic["count"]; }
                            else{ copy.Column = cell.Column + i * dic["count"];}
                            this.cells.Add(copy);
                        }
                        foreach (var item in TableBodyItems[0].Value)
                        {
                            //根据模板加一行
                            Row row = new Row() { Height = bodyCellTemplate[0].Height };
                            this.rows.Add(row);
                        }
                    }
                }
            }
            else if (HasLeft)
            {
                //根据左侧复制主体
                for (int i = 0; i < TableLeftItems.Value.Count; i++)
                {
                    CurrentLeft = TableLeftItems.Value[i];
                    foreach (Cell cell in bodyCellTemplate)
                    {
                        Cell copy = cell.copyCell();
                        Program prog = new Program(copy.Content, this, false);
                        Delegate d = prog.Parse(prog.Exp);
                        copy.Delegate = d;
                        object value = copy.Delegate.DynamicInvoke();
                        copy.Content = value + "";
                        copy.Row = cell.Row + i;
                        copy.Column = cell.Column;
                        this.cells.Add(copy);
                    }
                    //根据模板加一行
                    Row row = new Row() { Height = bodyCellTemplate[0].Height };
                    this.rows.Add(row);
                }
            }
            //只有主体部分，没有左侧，没有表头变化部分
            else
            {
                for (int i = 0; i < TableBodyItems[0].Value.Count; i++)
                {
                    CurrentMain = TableBodyItems[0].Value[i];
                    foreach (Cell cell in bodyCellTemplate)
                    {
                        Cell copy = cell.copyCell();
                        Program prog = new Program(copy.Content, this, false);
                        Delegate d = prog.Parse(prog.Exp);
                        copy.Delegate = d;
                        object value = copy.Delegate.DynamicInvoke();
                        copy.Content = value + "";
                        copy.Row = cell.Row + i;
                        this.cells.Add(copy);
                    }
                    if (bodyCellTemplate.Count != 0)
                    {
                        //根据模板加一行
                        Row row = new Row() { Height = bodyCellTemplate[0].Height };
                        this.rows.Add(row);
                    }
                }
            }
        }
        #endregion

        #region 获得包含几列,最大列，和最小列
        /// <summary>
        /// 获得包含几列,最大列，和最小列
        /// </summary>
        /// <param name="Template"></param>
        /// <returns></returns>
        private Dictionary<string, int> getColumnNumber(List<Cell> Template)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            int min = -1;
            int max = 0;
            for (int i = 0; i < Template.Count; i++)
            {
                Cell cell = Template[i];
                if (min == -1 || cell.Column < min)
                {
                    min = cell.Column;
                }
                if (cell.Column >= min && cell.Column >= max)
                {
                    max = cell.Column;
                }
            }
            result.Add("max", max);
            result.Add("min", min);
            result.Add("count", max - min + 1);
            return result;
        }
        #endregion

        /// <summary>
        /// 获得参数有几行
        /// </summary>
        /// <param name="Template"></param>
        /// <returns></returns>
        private int getRowNumber(List<Cell> Template)
        {
            int min = 0;
            int max = 0;
            for (int i = 0; i < Template.Count; i++)
            {
                Cell cell = Template[i];
                if (min == 0 || cell.Row < min)
                {
                    min = cell.Row;
                }
                if (cell.Row >= min && cell.Row >= max)
                {
                    max = cell.Row;
                }
            }
            return max - min + 1;
        }

        //表格线所在层，从模板里取
        private Canvas lineLayout;

        //单元格所在层，从模板里取
        private Canvas cellLayout;

        //单元格列表，每个单元格主要包括坐标及大小
        List<Cell> cells = new List<Cell>();

        //列信息
        List<Row> rows = new List<Row>();

        //行信息
        List<Column> columns = new List<Column>();

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //获取画线的面板
            this.lineLayout = GetTemplateChild("PART_LineLayout") as Canvas;

            //获取放单元格的面板
            this.cellLayout = GetTemplateChild("PART_CellLayout") as Canvas;
        }

        //计算单元格宽带
        private int GetWidth(Cell cell)
        {
            int result = 0;

            for (int i = 0; i < cell.ColumnSpan; i++)
            {
                result += this.columns[i + cell.Column].Width;
            }

            return result;
        }

        //计算单元格高度
        private int GetHeight(Cell cell)
        {
            int result = 0;

            for (int i = 0; i < cell.RowSpan; i++)
            {
                result += this.rows[i + cell.Row].Height;
            }

            return result;
        }

        //重新绘制
        private void Layout()
        {
            this.TableToJson = tableToJson();
            LayoutColumns();
            LayoutRows();
            LayoutCellWidthAndHeight();
            LayoutLines();
            LayoutCells();
        }

        /// <summary>
        /// 把json转成报表对象
        /// </summary>
        /// <param name="json"></param>
        private void jsonToTable(string json)
        {
            this.cells.Clear();
            this.columns.Clear();
            this.rows.Clear();
            JsonObject item = JsonValue.Parse(json) as JsonObject;
            GeneralObject go = new GeneralObject();
            go.FromJson(item);
            //列
            ObjectList ol = go.GetPropertyValue("Column") as ObjectList;
            foreach (GeneralObject one in ol)
            {
                Column c = new Column();
                c.Width = Int32.Parse(one.GetPropertyValue("Width") + "");
                this.columns.Add(c);
            }
            //行
            ol = go.GetPropertyValue("Row") as ObjectList;
            foreach (GeneralObject one in ol)
            {
                Row r = new Row();
                r.Height = Int32.Parse(one.GetPropertyValue("Height")+"");
                this.rows.Add(r);
            }
            //单元格
            ol = go.GetPropertyValue("Cell") as ObjectList;
            foreach (GeneralObject one in ol)
            {
                Cell c = new Cell();
                c.Content = one.GetPropertyValue("Content") + "";
                c.Column = Int32.Parse(one.GetPropertyValue("Column") + "");
                c.ColumnSpan = Int32.Parse(one.GetPropertyValue("ColumnSpan") + "");
                c.Row = Int32.Parse(one.GetPropertyValue("Row") + "");
                c.RowSpan = Int32.Parse(one.GetPropertyValue("RowSpan") + "");
                c.Location = one.GetPropertyValue("Location") + "";
                this.cells.Add(c);
            }
            //绘制报表
            Layout();
        }

        /// <summary>
        /// 导出excel
        /// </summary>
        public void ToExcel()
        {
            //将hql请求发送到后台，由后台执行查询，把查询结果写入Excel文件
            string uuid = System.Guid.NewGuid().ToString();
            string str = WebClientInfo.BaseAddress + "/excel";
            Uri uri = new Uri(str);
            string json = tableToJson();
            WebClient client = new WebClient();
            client.UploadStringCompleted += new UploadStringCompletedEventHandler(client_UploadStringCompleted);
            client.UploadStringAsync(uri, json);
        }

        void client_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                //获取Excel文件名
                ExcelFileName = e.Result;
                //触发导出完成事件
                OnCompleted();
            }
        }

        #region Completed 导出完成事件
        /// <summary>
        /// 导出完成事件
        /// </summary>
        public event EventHandler Completed;
        public void OnCompleted()
        {
            if (Completed != null)
            {
                Completed(this, null);
            }
        }
        #endregion

        #region 把表格数据转化成json字符串
        /// <summary>
        /// 把表格数据转化成json字符串
        /// </summary>
        /// <returns></returns>
        private string tableToJson()
        {
            string result = "";
            //转化列,记录列宽
            result += "{Column:[";
            for (int i = 0; i < this.columns.Count; i++)
            {
                Column column = this.columns[i];
                //最后一列
                if (i == this.columns.Count - 1)
                {
                    result += "{Width:" + column.Width + "}],"; break;
                }
                result += "{Width:" + column.Width + "},";
            }
            //转化行,记录行高
            result += "Row:[";
            for (int i = 0; i < this.rows.Count; i++)
            {
                Row row = this.rows[i];
                //最后一行
                if (i == this.rows.Count - 1)
                {
                    result += "{Height:" + row.Height + "}],"; break;
                }
                result += "{Height:" + row.Height + "},";
            }
            //转化单元格
            result += "Cell:[";
            for (int i = 0; i < this.cells.Count; i++)
            {
                Cell cell = this.cells[i];
                if (i == this.cells.Count - 1)
                {
                    result += "{Content:'" + cell.Content + "',Column:" + cell.Column + ",ColumnSpan:" + cell.ColumnSpan + ",Row:" + cell.Row + ",RowSpan:" + cell.RowSpan + ",Location:'" + cell.Location + "'}]"; break;
                }
                result += "{Content:'" + cell.Content + "',Column:" + cell.Column + ",ColumnSpan:" + cell.ColumnSpan + ",Row:" + cell.Row + ",RowSpan:" + cell.RowSpan+",Location:'"+cell.Location+"'},";
            }
            result += "}";
            return result;
        }


       

        #endregion

        #region 重画表格线
        //重画表格线
        private void LayoutLines()
        {
            lineLayout.Children.Clear();

            //在面板上根据单元格内容画线
            foreach (Cell cell in this.cells)
            {
                Column column = columns[cell.Column];
                Row row = rows[cell.Row];

                Line topLine = new Line()
                {
                    X1 = column.StartX,
                    X2 = column.StartX + cell.Width,
                    Y1 = row.StartY,
                    Y2 = row.StartY,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1
                };
                Line downLine = new Line()
                {
                    X1 = column.StartX,
                    X2 = column.StartX + cell.Width,
                    Y1 = row.StartY + cell.Height,
                    Y2 = row.StartY + cell.Height,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1
                };
                Line leftLine = new Line()
                {
                    X1 = column.StartX,
                    X2 = column.StartX,
                    Y1 = row.StartY,
                    Y2 = row.StartY + cell.Height,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1
                };
                Line rightLine = new Line()
                {
                    X1 = column.StartX + cell.Width,
                    X2 = column.StartX + cell.Width,
                    Y1 = row.StartY,
                    Y2 = row.StartY + cell.Height,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1
                };
                lineLayout.Children.Add(topLine);
                lineLayout.Children.Add(downLine);
                lineLayout.Children.Add(leftLine);
                lineLayout.Children.Add(rightLine);
            }
        }

        //重新绘制单元格
        private void LayoutCells()
        {
            this.cellLayout.Children.Clear();

            //在面板上根据单元格内容写TextBlock
            foreach (Cell cell in this.cells)
            {
                Column column = columns[cell.Column];
                Row row = rows[cell.Row];

                TextBlock text = new TextBlock();
                text.Text = cell.Content;
                if (cell.Location == "center")
                {
                    text.TextAlignment = TextAlignment.Center;
                }
                else if (cell.Location == "right")
                {
                    text.TextAlignment = TextAlignment.Right;
                }
                else if (cell.Location == "left")
                {
                    text.TextAlignment = TextAlignment.Left;
                }
                Canvas.SetLeft(text, (double)column.StartX);
                Canvas.SetTop(text, (double)row.StartY);
                text.Width = cell.Width;
                text.Height = cell.Height;
                this.cellLayout.Children.Add(text);
            }
        }

        //重新计算单元格宽高
        private void LayoutCellWidthAndHeight()
        {
            //在面板上根据单元格内容写TextBlock
            foreach (Cell cell in this.cells)
            {
                cell.Width = this.GetWidth(cell);
                cell.Height = this.GetHeight(cell);
            }
        }

        //重新计算每列起始位置
        private void LayoutColumns()
        {
            int startX = 0;

            //循环加前面列的宽度
            foreach (Column column in this.columns)
            {
                column.StartX = startX;
                startX += column.Width;
            }
        }

        //重新计算每行起始位置
        private void LayoutRows()
        {
            int startY = 0;

            //循环加前面列的宽度
            foreach (Row row in this.rows)
            {
                row.StartY = startY;
                startY += row.Height;
            }
        }

        #endregion

        /// <summary>
        /// 根据名称，查找所有资源
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object FindResource(string name)
        {
            //如果是left，返回当前左侧数据
            if (name == "left")
            {
                GeneralObject go = new GeneralObject();
                go.CopyFrom(CurrentLeft);
                return go;
            }
            else if (name == "head")
            {
                GeneralObject go = new GeneralObject();
                go.CopyFrom(CurrentHead);
                return go;
            }
            else if (name == "each")
            {
                GeneralObject go = new GeneralObject();
                go.CopyFrom(CurrentMain);
                return go;
            }
            else
            {
                //在主体数据中查询资源
                foreach (BodyDatas item in TableBodyItems)
                {
                    if (item.Name == name)
                    {
                        return item.Value;
                    }
                }
            }
            //调用框架的查找资源
            return FrameworkElementExtension.FindResource(this,name);
        }

        #region PropertyChanged事件
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion
    }




    //主体所有数据
    public class BodyDatas
    {
        public BodyDatas() { }

        public BodyDatas(string name, string sql, ObjectList data) 
        {
            this.Name = name;
            this.Sql = sql;
            this.Value = data;
        }

        public string Name { get; set; }

        public string Sql { get; set; }
        /// <summary>
        /// 临时存放sql
        /// </summary>
        public string NewSql { get; set; }


        private ObjectList value = new ObjectList();
        public ObjectList Value 
        {
            get { return this.value; }
            set { this.value = value; } 
        }
    }

    //单元格，包括坐标及大小
    class Cell:Control
    {
        public int Row = 0;
        public int Column = 0;
        public int RowSpan = 1;
        public int ColumnSpan = 1;
        public string Location = "";
        public int Width;
        public int Height;

        //单元格内容，先考虑文本，不考虑控件
        public string Content = "";
        //表达式代理
        public Delegate Delegate;
        /// <summary>
        /// 复制单元格
        /// </summary>
        /// <returns></returns>
        public Cell copyCell()
        {
            Cell cell = new Cell() { Row = Row, Column = Column, Content = Content, RowSpan = RowSpan, ColumnSpan = ColumnSpan,Location=Location, Width = Width, Height = Height, Delegate = Delegate };
            return cell;
        }

    }

    //列信息，包括列宽及开始坐标
    class Column : IComparable
    {
        public int Number;
        public int Width;
        public int StartX;

        public int CompareTo(object obj)
        {
            Column column = obj as Column;
            return this.Number - column.Number;
        }
    }

    //行信息，包括行高及开始坐标
    class Row : IComparable
    {
        public int Number;
        public int Height;
        public int StartY;

        public int CompareTo(object obj)
        {
            Row row = obj as Row;
            return this.Number - row.Number;
        }
    }

     
}

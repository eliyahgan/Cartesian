using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json;
namespace Cartesian
{
    public partial class CarBuPanel : Panel
    {
        #region 字段 

        //--------------------------字段------------------------------
        private PictureBox background = new PictureBox();
        private Label labPanSize = new Label();
        private List<string> instrus = new List<string> { };//指令集
        private Int32[] lastBox = new Int32[] { }; //lastBox的坐标点
        private string lastAction ="N";//上一个动作
        private Dictionary<char, Int32[]> LastSpecificBox = new Dictionary<char, Int32[]>();//创建保存上次指定操作产生的矩形坐标的字典
        private int[] originBox = new int[4] { 0, 0,0,0 };//原点,使用时采用.clone方法防止出现相同引用
        public int[] panelTempSize = new int[2];//panel大小临时容器,因为this.panel.size.width = XXX会报错,struct陷阱
        //private enum enumAct { Q,R,W,Z,Y,X };
        private string actRuler = "NRQWZYX";//N代表已生成一个玻璃,可以当作最小
        //private List<Int32[][]> btnsBox = new List<Int32[][]> { };//button矩形的坐标点集btnsBox
        public List<btnBox> btnsList = new List<btnBox> { };
        private static int btnId = 1;
        public class btnBox : Button 
        {
            public Int32[] box;
            public Int32[] tembox;
            public string detail;//N的值
            public int id = btnId++;
            public int sizex;
            public int sizey;
            public btnBox(Int32[] box)
            {
                this.box = box;
                sizex = box[2]-box[0];
                sizey = box[3]-box[1];
                detail = sizex + "*" + sizey;
            }
            public void Shift(float n, int ymax,int xmax, int BGx, int BGy,RotateDegree btnsrotate,bool btnsflip)
            {
                tembox = (Int32[])box.Clone();
                if (btnsflip)
                {
                    tembox[0] = xmax - box[2];
                    tembox[2] = xmax - box[0];
                }
                switch ((Int32)btnsrotate)
                {
                    case 0: break;
                    case 1: 
                        tembox[0] = box[1];
                        tembox[1] = ymax - box[2];
                        tembox[2] = box[3];
                        tembox[3] = ymax - box[0];
                        break;
                    case 2: 
                        tembox[0] = xmax - box[2];
                        tembox[1] = ymax - box[3];
                        tembox[2] = xmax - box[0];
                        tembox[3] = ymax - box[1];
                        break;
                    case 3: 
                        tembox[0] = xmax - box[3];
                        tembox[1] = box[0];
                        tembox[2] = xmax - box[1];
                        tembox[3] = box[2];
                        break;
                }

                Location = new System.Drawing.Point((int)Math.Ceiling(tembox[0] * n + BGx), (int)Math.Ceiling((ymax - tembox[3]) * n) + BGy);//BGxy为使btns在BG上定位,因为BG不是父对象
                    //Location = new System.Drawing.Point((int)(box[0] * n+BGx), (int)(ymax - box[3] * n)+BGy);
                    //Console.WriteLine("X坐标为:"+Location.X + "---Y坐标为:" + Location.Y);
                    Size = new System.Drawing.Size(
                        (int)Math.Ceiling((tembox[2] - tembox[0]) * n),
                        (int)Math.Ceiling((tembox[3] - tembox[1]) * n));
                    //location和size都向上取整防止产生空隙
            }
        }
        #endregion
        #region 属性

        //---------------------------属性----------------------------
        //[Browsable(false)]
        private Color _btnsColor1 = SystemColors.Control; 
        [Description("按钮初始颜色")]
        [DefaultValue(typeof(Color), "control")]
        public Color BtnsColor1  {
            get
            {
                return _btnsColor1;
            }
            set
            {
                _btnsColor1 = value;
            }
        }
        private Color _btnsColor2 = Color.Silver;
        [Description("按钮通过之后的颜色")]
        [DefaultValue(typeof(Color), "Silver")]
        public Color BtnsColor2
        {
            get
            {
                return _btnsColor2;
            }
            set 
            {
                _btnsColor2 = value; 
            }
        }
        private bool _btnsFlip = false;
        [Description("btns的排布是否翻转")]
        [DefaultValue(false)]
        public bool BtnsFlip {
            get
            {
                return _btnsFlip;
            }
            set
            {
                _btnsFlip = value;
            }
        }

        public enum RotateDegree
        {
            Default,
            _90,
            _180,
            _270,
        }
        private RotateDegree _btnsrotate = 0;
        [Description("btns顺时针方向的旋转调整角度")]
        [DefaultValue(0)]
        public RotateDegree BtnsRotate//影响panelsize,listbtns顺序,boxs的坐标三个
        {
            get
            {
                return _btnsrotate;
            }
            set
            {
                _btnsrotate = value;
            }
        }

        private bool _isShowLab = true;
        [Description("是否显示原片大小标签 ")]
        [DefaultValue(true)]
        public bool IsShowLab
        {
            get
            {
                return _isShowLab;
            }
            set
            {
                _isShowLab = value;
            }
        }

        #endregion
        
        //--------------------------方法-----------------------------
        public CarBuPanel()
        {
            InitializeComponent();
        }

        public CarBuPanel(IContainer container)//-------------------------程序入口--------------------------
        {
            container.Add(this);
            InitializeComponent();
            //数据输入临时接口,后期可修改GetInstruction
            instrus = GetInstructions();
            foreach (string item in instrus)
            {
                InstruToBtns(item);
            }

            Console.WriteLine(btnsList.Count);
            for (int i = 0; i < btnsList.Count; i++)
            {
                Console.WriteLine(btnsList[i].sizex+","+ btnsList[i].sizey+","+i+",");
            }
            Console.ReadLine();
        }

        /// <summary>
        /// 通过readline方式获得切割指令集
        /// </summary>
        /// <returns>返回string类型指令的List集合</returns>
        private List<string> GetInstructions()
        {
            //批处理
            List<string> convert_output = new List<string>();
            List<string> instructions = new List<string>();
            while (true)//循环获得原数据
            {
                var a = Console.ReadLine();
                if (a == "")
                {
                    break;
                }
                instructions.Add(a);
            }
            if (instructions.Count == 0)
            {
                Console.WriteLine("未接收到指令,使用TEST指令:\n[P2H2RBEA0R80E8xC8XBE3CY4240Z2562V2W2120N1CT7Z1568V2W2120N9Y2AD0Z2120W1568N9W1568N9Z2120W1568NBW1568NBZ2120W1568NBW1568NBZ2120W1568NBW1568NBZ2120W1568NBW1568NBZ175CW294AN1AY1374T3Z2120N3Z294AN14Z311AN20]\n" );
                instructions.Add(@"[P1H1RBEA0R80E8xC8XBE3CY294AT2Z311AN3CT3Z1374N14Z2120N28Y2562T3Z311AN40Z294AN32Y311AT3Z2562N40Z217AN37T2Z15C2N19]
[P2H2RBEA0R80E8xC8XBE3CY4240Z2562V2W2120N1CT7Z1568V2W2120N9Y2AD0Z2120W1568N9W1568N9Z2120W1568NBW1568NBZ2120W1568NBW1568NBZ2120W1568NBW1568NBZ2120W1568NBW1568NBZ175CW294AN1AY1374T3Z2120N3Z294AN14Z311AN20]
[P3H3RBEA0R80E8xC8XBE3CY2562T3Z311AN40Z294AN32Y311AT6Z1F86N33Y294AT8Z1374N14Z2120N28]
[P4H4RBEA0R80E8xC8X2328Y56C2N4CY294AZ217AN27X222EY56C2N4BY294AZ2120N28X4A2EY56C2Z18BAN47Z18BAN4AZ18BAN4AY28F0T2Z24D6N29X2EB8Y3372T2Z175CN21V2Y2562T2Z175CN15]
[P5H5RBEA0R80E8xC8XBE3CY2562T2Z311AN40Z294AN32T2Z175CN15Y311AT6Z1F86N33Y294AT8Z1374N14Z2120N28]
[P6H6RBEA0R80E8xC8XBE3CY2120Z2562N1CT4Z1568NDZ217AN1DZ1F86N18Y2AD0T3Z2120V2W1568NDZ3066W294AN3EZ294AW294AN3AY2120Z1568NDZ294AN2ET4Z1F86N18Y1374Z2120N3T3Z311AN20]
[P7H7RBEA0R80E8xC8XBE3CY1C84Z479AN48T2Z399EN38Y1C84Z399EN39Z2D32V2WD98N5Z51E0V2WDCAT2Q28F0N1Y2120Z2562W2120N1CZ2562W2120N1CZ2562W2120N1CZ2562W2120N24T2Z1374N3Y2562T3Z2120N24Z311AN40Z294AN32]
[P8H8RBEA0R80E8xC8XBE3CY7E18T3Z294AV4W1F86N23Z1F86V3W294AN23Z2120V4W1F86N18]
[P9H9RBEA0R80E8xC8XBE3CY294AZ3372N43Z4614W2562T3Q175CN15T2Z1F86N23Y175CT6Z1F86NAY3F0CZ175CV2W1F86NAT4Z294AV2W1F86N23]
[PAHARBEA0R80E8xC8X7BDEV3Y294AT3Z294AN3AX294AY2058N2DV3Y1F86N23X175CV3Y1F86N13Y217AN16]");
                //instructions.Add("[P2H2RBEA0R80E8xC8XBE3CY4240Z2562V2W2120N1CT7Z1568V2W2120N9Y2AD0Z2120W1568N9W1568N9Z2120W1568NBW1568NBZ2120W1568NBW1568NBZ2120W1568NBW1568NBZ2120W1568NBW1568NBZ175CW294AN1AY1374T3Z2120N3Z294AN14Z311AN20]");
            }

            foreach (string n in instructions)//循环输出转换数据
            {
                //分割
                var parts = Regex.Matches(n, @"[PHRxyXYZWRQVTN][0-9A-F]+")
                                .Cast<Match>()
                                .Select(m => m.Value)
                                .ToList().ToArray();

                //转换为10进制数值
                for (var i = 0; i <= parts.Length - 1; i++)
                {
                    var tempFun = "";
                    var tempValue = "";
                    tempFun = parts[i].Remove(1);
                    tempValue = parts[i].Remove(0, 1);
                    var tempValueInt = Convert.ToInt32(tempValue, 16);

                    //数字除10
                    if (Regex.Match(tempFun, "[xyXYZWRQ]").ToString() != "")
                    {//这个判定不能用null来判断match空对象,而是tostring然后判断空字符串
                        tempValueInt = tempValueInt / 10;//除10是因为数据转换出来发现大了10倍
                    }

                    parts[i] = tempFun + tempValueInt;
                }
                string output = string.Join("-", parts);
                convert_output.Add(output);
            }
            foreach (var item in convert_output)//show converted instructions
            {
                //Console.WriteLine(item);
            }
            return convert_output;
        }

        /// <summary>
        /// 输入一个指令，更新成员变量btnsBox
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private void InstruToBtns(string instruction)
        {
            //初始化字典
            LastSpecificBox['X'] = (int[])originBox.Clone();
            LastSpecificBox['Y'] = (int[])originBox.Clone();
            LastSpecificBox['Z'] = (int[])originBox.Clone();
            LastSpecificBox['W'] = (int[])originBox.Clone();
            LastSpecificBox['Q'] = (int[])originBox.Clone();
            LastSpecificBox['R'] = (int[])originBox.Clone();
            
            //LastSpecificBox.Add('X', (int[])originBox.Clone());
            //LastSpecificBox.Add('Y', (int[])originBox.Clone());
            //LastSpecificBox.Add('Z', (int[])originBox.Clone());
            //LastSpecificBox.Add('W', (int[])originBox.Clone());
            //LastSpecificBox.Add('Q', (int[])originBox.Clone());
            //LastSpecificBox.Add('R', (int[])originBox.Clone());利于多次重复操作时不会出现已存在变量

            //将指令中R代表的原片最大XY改为jk表示
            var instruBuilder = new StringBuilder(instruction);
            instruBuilder[instruction.IndexOf('R')] = 'j';//Xmax
            instruBuilder[instruction.IndexOf('R', instruction.IndexOf('R') + 1)] = 'k';//Ymax,在第一个R之后再检索，相当于找到第二个R
            instruction = instruBuilder.ToString();

            //处理指令的VT动作
            while (instruction.IndexOfAny(new char[] { 'V', 'T', 'M' }) != -1)
            {
                instruction = DealVT(instruction);
            }
            Console.WriteLine(instruction+"\n");

            //split the instruction
            List<string> actions = new List<string>(instruction.Split(new char[] { '-' }));

            //lastBox的不断更新并引用DoAction
            lastBox =(Int32[])originBox.Clone();
            foreach (string Item in actions)
            {
                DoAction(Item);
                Console.WriteLine(Item+"--->("+lastBox[0]+","+lastBox[1]+"),("+lastBox[2]+","+lastBox[3]+@")
===========================================================");//lastbox坐标显示
            }
        }
        #region 实行细节
        /// <summary>
        /// 进行具体切割操作
        /// </summary>
        /// <param name="lastBox"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        //private  DoAction(Int32[][] lastBox, string action)
        private void DoAction(string action)
    {
            int actVlu = Convert.ToInt32(action.Remove(0,1), 10);
            char actName = action[0];
            switch ((int)actName)
            {
                //PHRxyXYZWRQVTN
                case (int)'P': break;
                case (int)'H': break;
                case (int)'j':
                    panelTempSize[0] = actVlu;
                    break;
                case (int)'k':
                    panelTempSize[1] = actVlu;
                    LastSpecificBox['X'][3] = panelTempSize[1];//初始化是由第一个X开始的
                    break;
                case (int)'x': break;
                case (int)'y': break;
                case (int)'X':
                    CarteTrans(actName, actVlu);
                    break;
                case (int)'Y':
                    CarteTrans(actName, actVlu);
                    break;
                case (int)'Z':
                    CarteTrans(actName, actVlu);
                    break;
                case (int)'W':
                    CarteTrans(actName, actVlu);
                    break;
                case (int)'R':
                    CarteTrans(actName, actVlu);
                    break;
                case (int)'Q':
                    CarteTrans(actName, actVlu);
                    break;
                case (int)'N':
                    btnsList.Add(new btnBox(lastBox));
                    foreach( var btnbox in btnsList)
                    {
                        if (btnbox.Size == btnsList[btnsList.Count-1].Size && btnbox.detail == "") 
                        {
                            btnbox.detail = actName + actVlu.ToString();
                            //写入N中的详细信息,实际情况逻辑可调,调用某个方法
                        }
                    }
                    lastAction = actName.ToString();
                    break;
                default: Console.WriteLine("DoAction未识别有效case"); break;

            }
        }
        /// <summary>
        /// 对XYZWRQ实现具体的坐标累计变换,并放入btnList
        /// </summary>
        /// <param name="actName"></param>
        /// <param name="actVlu"></param>
        private void CarteTrans(char actName, int actVlu)//坐标变换
        {
            if (actRuler.IndexOf(lastAction) > actRuler.IndexOf(actName.ToString()))
            {
                if (actRuler.IndexOf(actName) % 2 ==0)
                {//为垂直切割动作时
                    lastBox[2] = lastBox[0]+ actVlu;
                    Console.WriteLine("lower act vertical");
                }
                else
                {//为横向切割动作时
                    lastBox[3] = lastBox[1] + actVlu;
                    Console.WriteLine("lower act Horizontal");
                }
            }
            else
            {
                if (lastAction != "N") {
                    btnsList.Add(new btnBox(lastBox));//避免与N重复操作,保证无N的时候也能正常工作
                }
                lastBox = (int[])LastSpecificBox[actName].Clone();
                //foreach (var item in LastSpecificBox) {
                //    Console.WriteLine("LastSpeBoxof--" + item.Key + "(" + item.Value[0][0] + "," + item.Value[0][1] + "),(" + item.Value[1][0] + "," + item.Value[1][1] + ")");
                //}
                Console.WriteLine("LastSpeBoxof--" + actName + "(" + LastSpecificBox[actName][0] + "," + LastSpecificBox[actName][1] + "),(" + LastSpecificBox[actName][2] + "," + LastSpecificBox[actName][3] + ")");
                if (actRuler.IndexOf(actName) % 2 ==0)
                {//为垂直切割动作时
                    lastBox[0] = lastBox[2];
                    lastBox[2] = lastBox[2] + actVlu;
                    Console.WriteLine("upper act vertical");
                }
                else
                {//为横向切割动作时
                    lastBox[1] = lastBox[3];
                    lastBox[3] = lastBox[3] + actVlu;
                    Console.WriteLine("upper act Horizontal");
                }
            }
            lastAction = actName.ToString();
            LastSpecificBox[actName] = (int[])lastBox.Clone();
            Console.WriteLine("SetLastSpeBoxof--" + actName + "(" + LastSpecificBox[actName][0] + "," + LastSpecificBox[actName][1] + "),(" + LastSpecificBox[actName][2] + "," + LastSpecificBox[actName][3] + ")");

        }
        private string DealVT(string instruction)
        {
            //TEST : [P2H2RBEA0R80E8xC8XBE3CY4240Z2562V2W2120R2120Q2120N1CT7Z1568V2W2120N9Y2AD0Z2120W1568N9W1568N9Z2120W1568NBW1568NBZ2120W1568NBW1568NBZ2120W1568NBW1568NBZ2120W1568NBW1568NBZ175CW294AN1AY1374T3Z2120N3Z294AN14Z311AN20]
            int idxVT = instruction.IndexOfAny(new char[] { 'V', 'T', 'M' });
            int idxAction = instruction.IndexOfAny(new char[] { 'X', 'Y', 'Z', 'W', 'R', 'Q' }, idxVT);
            int idxNextAct = instruction.IndexOfAny(new char[] {'X','Y','Z','W','R','Q'}, idxAction+1);

            if (idxNextAct == -1 || actRuler.IndexOf(instruction[idxAction]) <= actRuler.IndexOf(instruction[idxNextAct]))//如果后面没有方法了或者后一个方法大于等于前一个方法
            {
                idxNextAct = idxAction;
            }
            else 
            {
                while (actRuler.IndexOf(instruction[idxAction]) > actRuler.IndexOf(instruction[idxNextAct]))//如果后一个方法小于前一个方法
                {
                    var a = idxNextAct;
                    idxNextAct = instruction.IndexOfAny(new char[] { 'X', 'Y', 'Z', 'W', 'R', 'Q' }, idxNextAct + 1);
                    if (idxNextAct == -1 || actRuler.IndexOf(instruction[idxAction]) <= actRuler.IndexOf(instruction[idxNextAct]))
                    {//预测已经循环到底了,则复原idxNextAct并跳出循环
                        idxNextAct = a;
                        break;
                    }
                }
                //while循环可以获得最后一个小于原方法的方法位置,在获得方法位置之后,就可以将下一个'-'之前的copy
            }
            int idxEnd = instruction.IndexOf('-', idxNextAct);
            int idxVTEnd = instruction.IndexOf('-', idxVT);
            int VTValue = Convert.ToInt32(instruction.Substring(idxVT + 1, idxVTEnd - idxVT - 1), 10);
            string copyItem = instruction.Substring(idxAction, idxEnd - idxAction+1);
            string copy = copyItem;
            for (var i = 1; i < VTValue; i++)
            {
                copy = copy.Insert(copy.Length, copyItem);
            }//获得批量复制后的copy
            instruction = instruction.Remove(idxVT, idxEnd - idxVT + 1);
            instruction = instruction.Insert(idxVT, copy);
            return instruction;
        }

        #endregion
        public void DrawBtnsPanel(int[] panelTempSize)
        {
            this.background.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.background.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            float n;//缩放倍率
            const int padding = 3;//btns与background的边距
            if ((Int32)BtnsRotate % 2 == 1) {//如果旋转90\270度,panel的XY颠倒
                var temp = panelTempSize[0];
                panelTempSize[0] = panelTempSize[1];
                panelTempSize[1] = temp;
            }
            var tempTempsize = (int[])panelTempSize.Clone();//保存panelTempSize未被缩放时的数据用于shift方法中,
            if (this.Size.Width * 1.0f / panelTempSize[0] > this.Size.Height * 1.0f / panelTempSize[1])
            {
                n = (this.Size.Height * 1.0f - padding) / panelTempSize[1];
                panelTempSize[1] = this.Size.Height-padding;
                panelTempSize[0] = (int)(panelTempSize[0] * n);
            }
            else {
                n = (this.Size.Width * 1.0f - padding) / panelTempSize[0];
                panelTempSize[0] = this.Size.Width - padding;
                panelTempSize[1] = (int)(panelTempSize[1] * n);
            }//判断沿X还是Y方向缩小
            this.background.Location = new System.Drawing.Point((this.Size.Width - panelTempSize[0])/2, (this.Size.Height - panelTempSize[1])/2);
            this.background.Size = new System.Drawing.Size(panelTempSize[0] + padding, panelTempSize[1] + padding);//配置背景
            if (_isShowLab) {//配置label
                this.labPanSize.Text = tempTempsize[0] + "*" + tempTempsize[1];
                this.labPanSize.AutoSize = true;
                this.Controls.Add(labPanSize);
                this.labPanSize.Location = new System.Drawing.Point(
                    labPanSize.Size.Width < background.Location.X + padding ? background.Location.X - labPanSize.Size.Width - padding : 0,
                    labPanSize.Size.Height < background.Location.Y + padding ? background.Location.Y - labPanSize.Size.Height - padding : 0
                    );
                this.labPanSize.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                this.labPanSize.Margin = new System.Windows.Forms.Padding(0);
            }
             
            //}//依据翻转改变渲染btn的顺序
            //switch ((Int32)BtnsRotate)
            //{
            //    case 0: break;
            //    case 1: break;
            //    case 2:
            //        btnsList.Reverse();
            //        break;
            //    case 3: break;
            //}//依据旋转改变渲染btn的顺序
            
            foreach (var item in btnsList) //渲染btns
            {
                item.Shift(n, tempTempsize[1], tempTempsize[0], background.Location.X, background.Location.Y, this.BtnsRotate,this.BtnsFlip);
                item.Margin = new System.Windows.Forms.Padding(0);
                item.BackColor = this.BtnsColor1;
                item.Text = ""+item.id +"/D" +(item.detail == null ? "" : (":" + item.detail));//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //item.Text = item.detail;
                this.Controls.Add(item);
                Font stringFont = new Font("Arial", 16);
                //Graphics a = item.CreateGraphics();
                

                float fontsize = 5F * panelTempSize[0] / 200;
                //var fontsize = 5F * panelTempSize[0]*panelTempSize[0]/
                item.Font = new System.Drawing.Font("宋体", fontsize, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(134)));
                //Console.WriteLine("panelx = {0} and fontsize = {1} and buttonsizex = {2}" + " and this.panelx = " + this.panelTempSize[0], panelTempSize[0], fontsize, item.Size.Width);
                //Console.WriteLine("id="+item.id+";btnX=" + item.Size.Width + ";btnY=" + item.Size.Height + ";locX=" + item.Location.X + ";locY" + item.Location.Y + ";");
            }
            this.Controls.Add(this.background);
        }

        private int currentBtn = 0;//类成员,后续调整可改为直接以光标位置
        public void NextBtn() 
        {
            if (currentBtn <= btnId - 2) {
                //btnsList[currentBtn].BackColor = System.Drawing.Color.Silver;
                btnsList[currentBtn].BackColor = this.BtnsColor2;
                currentBtn++;
                if (currentBtn <= btnId - 2)
                {
                    //private void Click(object sender, EventArgs e){};
                    //btnsList[currentBtn].Click += new System.EventHandler(this.button1_Click_1);
                    //btnsList[currentBtn](null, null);
                    btnsList[currentBtn].Focus();
                }
            }

        }
    } 
}
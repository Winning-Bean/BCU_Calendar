﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Threading;

namespace Shared_Calendar
{
    #region 부드러운 판넬
    // 사용시 윈폼 디자인 안보이네
    class DoubleBufferPanel : Panel
    {
        public DoubleBufferPanel()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true
                );
            this.UpdateStyles();
        }
    }
    #endregion

    //4개 넘어가면...으로 만들기 finish
    //판넬 클릭시 정보 finish
    //일정 추가 finish
    //색상 지정 finish
    //주간 이동 (부분완성) finish
    //다음주로 넘어가는 일정 만들기 finish
    //현재 시간표시 커서로 -> 현재날짜 탑 색칠, 왼쪽 시간에 표시하는걸로
    //탑에 시간설정 finish

    public partial class Week : Form
    {
        Panel insideMain = null;
        List<List<Panel>> day = null;

        DBSchedule dbs = new DBSchedule();
        DBConnection db = Program.DB;
        Color defaultColor = Color.FromArgb(200, 128, 128, 128);

        bool isMainClick = false;
        Panel clickPan;

        #region 둥근 모서리
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect,
          int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        #endregion

        private DateTime m_focus_dt; // 현재 포커스 날짜
        public DateTime FOCUS_DT
        { // 현재 포커스날짜 프로퍼티
            set { m_focus_dt = value; }
        }
        public DateTime Get_focus_dt() { return m_focus_dt; }

        public Week()
        {
            InitializeComponent();
            DoubleBuffered = true;
            day = new List<List<Panel>>(); // 각 시간마다 생성된 패널 (ex: day[1][7] -> 월요일 8 AM)
            insideMain = new Panel();
            insideMain.Location = new Point(20, 0);
            insideMain.Size = new Size(946, 1152); // 966 1512 (1칸당 125, 63) -> 966 1200 (1칸당 125, 50) -> 1152 (48) 
            insideMain.Show();
            insideMain.Paint += new System.Windows.Forms.PaintEventHandler(OnMainPaint);
            insideMain.BackColor = Color.Transparent;
            insideMain.MouseClick += new MouseEventHandler(OnMainClick);
            insideMain.MouseDoubleClick += new MouseEventHandler(OnMainClick);
            m_Main_pan.Controls.Add(insideMain);
            m_Main_pan.VerticalScroll.Minimum = 0;
            m_Main_pan.VerticalScroll.Maximum = 0;
            m_Main_pan.VerticalScroll.Visible = false;
            m_Main_pan.AutoScroll = true;
            m_Left_btn.MouseEnter += new EventHandler(OnShiftEnter);
            m_Left_btn.Name = "0";
            m_Left_btn.MouseLeave += new EventHandler(OnShiftLeave);
            m_Right_btn.MouseEnter += new EventHandler(OnShiftEnter);
            m_Right_btn.MouseLeave += new EventHandler(OnShiftLeave);
            m_Right_btn.Name = "1";

            m_Top_pan.Paint += new System.Windows.Forms.PaintEventHandler(OnTopPaint);

            makeTop();
            makeTime();
            makeDay();
            makeColor();
        }

        #region Week Form Designe
        // 폼 디자인에 사용한 코드


        private void OnTopPaint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            int x = 125;
            Graphics graphics = e.Graphics;
            Pen pen = new Pen(Color.LightGray);
            for (int i = 0; i < 6; i++)
            {
                graphics.DrawLine(pen, x, 0, x, 48);
                x += 125;
            }
            pen = new Pen(Color.LightGray, 3);
            graphics.DrawLine(pen, 0, m_Top_pan.Height, m_Top_pan.Width, m_Top_pan.Height);
            pen.Dispose();
        }

        private void OnMainPaint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            int x = 71;
            int y = 0;
            Graphics graphics = e.Graphics;
            Pen pen = new Pen(Color.Gainsboro);
            for (int i = 0; i < 24; i++)
            {
                graphics.DrawLine(pen, x, y, 956, y);
                y += 48;
            }
            x = 196;
            for (int i = 0; i < 6; i++)
            {
                graphics.DrawLine(pen, x, 5, x, 1507);
                x += 125;
            }
            pen.Dispose();

            if (isMainClick)
            {
                graphics.DrawRectangle(new Pen(Color.FromArgb(150, Color.Black), 2), new Rectangle(clickPan.Location, clickPan.Size));
            }
        }

        private void OnRealTime(object sender, System.Windows.Forms.PaintEventArgs e)
        {

        }

        private void makeTime()
        {
            int y = -11;
            for (int i = 0; i < 25; i++)
            {
                Label pan = new Label();
                pan.Size = new Size(71, 48); // 125
                pan.Location = new Point(0, y);
                //pan.BorderStyle = BorderStyle.FixedSingle;

                pan.TextAlign = ContentAlignment.TopRight;
                pan.Font = new System.Drawing.Font(FontLibrary.HANDOTUM, 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                pan.ForeColor = Color.DimGray;

                if (i < 12)
                    pan.Text = i.ToString() + " AM";
                else if (i > 12 && i < 24)
                {
                    pan.Text = (i % 12).ToString() + " PM";
                }
                else if (i == 12)
                {
                    pan.Text = "Noon";
                    pan.ForeColor = Color.Black;
                    pan.Font = new System.Drawing.Font(FontLibrary.HANDOTUM, 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                }
                else
                    pan.Text = "0 AM";

                y += 48;
                pan.Show();
                insideMain.Controls.Add(pan);
            }

        }

        private void makeTop()
        {
            int x = 0;
            String[] dayEnum = { "Sun", "Mon", "Tue", "Wed", "Thur", "Fri", "Sat" };

            for (int j = 0; j < 7; j++)
            {
                Panel pan = new Panel();
                pan.Size = new Size(125, m_Top_pan.Size.Height);
                pan.Location = new Point(x, 0);
                pan.BackColor = Color.Transparent;
                pan.Name = j.ToString();

                Label day = new Label();
                day.Font = new System.Drawing.Font(FontLibrary.HANDOTUM, 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                day.AutoSize = true;
                day.Location = new Point(10, 10);
                day.Text = dayEnum[j];
                day.ForeColor = Color.LightGray;
                pan.Controls.Add(day);

                Label num = new Label();
                num.Font = new System.Drawing.Font(FontLibrary.HANDOTUM, 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                num.AutoSize = true;
                num.Location = new Point(50, 4);
                num.ForeColor = Color.LightGray;
                pan.Controls.Add(num);

                pan.MouseEnter += new EventHandler(OnMouseEnter);
                pan.MouseLeave += new EventHandler(OnMouseLeave);
                pan.MouseClick += new MouseEventHandler(OnDayMouseClick);
                day.MouseEnter += new EventHandler(OnMouseEnter);
                day.MouseLeave += new EventHandler(OnMouseLeave);
                day.MouseClick += new MouseEventHandler(OnDayMouseClick);
                num.MouseEnter += new EventHandler(OnMouseEnter);
                num.MouseLeave += new EventHandler(OnMouseLeave);
                num.MouseClick += new MouseEventHandler(OnDayMouseClick);
                m_Top_pan.Controls.Add(pan);
                x += 125;
            }
        }

        private void OnShiftEnter(object sender, EventArgs e)
        {
            var pan = (Button)sender;
            if (pan.Name == "0")
                m_Left_btn.BackColor = defaultColor;
            else
                m_Right_btn.BackColor = defaultColor;
        }

        private void OnShiftLeave(object sender, EventArgs e)
        {
            var pan = (Button)sender;
            if (pan.Name == "0")
                m_Left_btn.BackColor = Color.Transparent;
            else
                m_Right_btn.BackColor = Color.Transparent;
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            ;
            if (sender is Panel)
            {
                Panel pan = (Panel)sender;
                pan.BackColor = Color.Gray;
            }
            else if (sender is Label)
            {
                Label lab = (Label)sender;
                Panel pan = (Panel)lab.Parent;
                pan.BackColor = Color.Gray;
            }

        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            if (sender is Panel)
            {
                Panel pan = (Panel)sender;
                pan.BackColor = Color.Transparent;
            }
            else if (sender is Label)
            {
                Label lab = (Label)sender;
                Panel pan = (Panel)lab.Parent;
                pan.BackColor = Color.Transparent;
            }
        }

        private void OnDayMouseClick(object sender, MouseEventArgs e)
        {
            isMainClick = false;
            insideMain.Invalidate();
            int x = 0;
            if (sender is Panel)
            {
                Panel pan = sender as Panel;

                for (int i = 0; i < 7; i++)
                {
                    if (day[i][0].Left >= pan.Location.X)
                    {
                        x = i;
                        break;
                    }
                }
            }
            else
            {
                Label pan = sender as Label;
                for (int i = 0; i < 7; i++)
                {
                    if (day[i][0].Left >= pan.Parent.Location.X)
                    {
                        x = i;
                        break;
                    }
                }
            }

            DateTime days = weekSunday.AddDays(x - startWeek);
            Day d = new Day();
            d.FOCUS_DT = days;
            d.ShowDialog();
            if (m_focus_dt != d.Get_focus_dt())
            {
                m_focus_dt = d.Get_focus_dt();
                resetSchedual();
            }
        }

        private void m_Left_btn_Click(object sender, EventArgs e)
        {
            if (weekSunday.AddDays(1 - weekSunday.Day) > weekSunday.AddDays(-7)) // 첫일 넘어가면
            {
                if (weekSunday.Day == weekSunday.AddDays(1 - weekSunday.Day).Day)
                    weekSunday = weekSunday.AddDays(-1 * weekSunday.Day);
                else
                    weekSunday = weekSunday.AddDays(1 - weekSunday.Day);
            }
            else
                weekSunday = weekSunday.AddDays(-7);
            m_focus_dt = weekSunday;
            //m_Str_focus.Text = m_focus_dt.ToString("yyyy년 MM월 dd일");
            m_Year_lab.Text = m_focus_dt.ToString("yyyy");
            m_Month_lab.Text = m_focus_dt.ToString("MM");


            clearObject();
            CurrWeek();

            Thread.Sleep(100); // 너무빨리 계속 누를경우 DB 에러발생 강제로 지연
        }

        public void resetSchedual()
        {
            if (scList != null)
                clearObject();
            CurrWeek();

            Thread.Sleep(100);
        }

        private void m_Right_btn_Click(object sender, EventArgs e)
        {
            if (DateTime.DaysInMonth(weekSunday.Year, weekSunday.Month) < weekSunday.Day + 7) // 끝일 넘어가면
                weekSunday = weekSunday.AddDays(DateTime.DaysInMonth(weekSunday.Year, weekSunday.Month) - weekSunday.Day + 1);
            else
                weekSunday = weekSunday.AddDays(7);
            m_focus_dt = weekSunday;
            //m_Str_focus.Text = m_focus_dt.ToString("yyyy년 MM월 dd일");
            m_Year_lab.Text = m_focus_dt.ToString("yyyy");
            m_Month_lab.Text = m_focus_dt.ToString("MM");


            clearObject();
            CurrWeek();

            Thread.Sleep(100);
        }


        private void makeDay()
        {
            int x = 71;
            for (int j = 0; j < 7; j++)
            {
                int y = 0;
                day.Add(new List<Panel>());
                for (int i = 0; i < 24; i++)
                {
                    Panel pan = new Panel();
                    pan.Size = new Size(125, 48);
                    pan.Location = new Point(x, y);
                    pan.Name = 0.ToString();
                    //pan.BorderStyle = BorderStyle.Fixed3D;
                    y += 48;
                    //pan.Show();
                    //insideMain.Controls.Add(pan);
                    day[j].Add(pan);
                }
                x += 125;
            }
        }

        List<Panel> colorPan = null;

        private void makeColor()
        {
            db.AdapterOpen("select * from COLOR_TB");
            DataSet ds = new DataSet();
            db.Adapter.Fill(ds, "COLOR_TB");
            DataTable dt = ds.Tables["COLOR_TB"];
            int loca = -300; // m_Mid_pan.Width - 80; // -----
            DBColor dbc = new DBColor();
            colorPan = new List<Panel>();

            for (int i = 0; i < dt.Rows.Count; i++) // 그레이색 제외해야함
            {
                DataRow dr = dt.Rows[i];
                Panel pan = new Panel();
                pan.Size = new Size(18, 18);
                pan.Location = new Point(2, loca);
                pan.BorderStyle = BorderStyle.FixedSingle;
                pan.BackColor = dbc.GetColorInsertCRCD(dr[0].ToString());
                pan.Name = dr[0].ToString();
                pan.Visible = false;
                pan.Click += new EventHandler(OnColorClick);
                loca += 22;
                colorPan.Add(pan);
                m_Main_pan.Controls.Add(pan);
            }
        }

        #endregion

        private void Week_Load(object sender, EventArgs e)
        {
            //m_Str_focus.Text = m_focus_dt.ToString("yyyy년 MM월 dd일");
            m_Year_lab.Text = m_focus_dt.ToString("yyyy");
            m_Month_lab.Text = m_focus_dt.ToString("MM");

            //m_Left_btn.MouseEnter += new EventHandler(OnShiftEnter);
            //m_Left_btn.MouseLeave += new EventHandler(OnShiftLeave);
            //m_Right_btn.MouseEnter += new EventHandler(OnShiftEnter);
            //m_Right_btn.MouseLeave += new EventHandler(OnShiftLeave);

            m_Main_pan.AutoScrollPosition = new Point(0, 300); // 휠 포지션을 가운데로 설정
            CurrWeek(); // 실질적인 일정 생성 메서드
        }

        DateTime weekSunday;

        DateTime startWeekDate;
        DateTime endWeekDate;
        int startWeek;
        int endWeek;

        private void CurrWeek()
        { // 현재날짜가 몇주차인지 구하는 메서드 -> 현재 주에 월요일부터 표현하기위해
          //현재시간을 년 월 일로 나눔

            int currYear = Int32.Parse(m_focus_dt.ToString("yyyy"));
            int currMonth = Int32.Parse(m_focus_dt.ToString("MM"));
            int currDay = Int32.Parse(m_focus_dt.ToString("dd"));
            if (currDay < 7 && new DateTime(currYear, currMonth, currDay).DayOfWeek != DayOfWeek.Sunday)
            {
                DateTime first = weekSunday.AddDays(1 - currDay);
                while (true)
                {
                    if (first.DayOfWeek == DayOfWeek.Sunday)
                    {
                        startWeek = (int)((new DateTime(currYear, currMonth, currDay)).DayOfWeek);
                        startWeekDate = new DateTime(currYear, currMonth, currDay);
                        endWeek = 7;
                        endWeekDate = new DateTime(currYear, currMonth, currDay + endWeek - startWeek); // 하루 더 많음
                        createWeek(first.AddDays(-7));
                        return;
                    }
                    first = first.AddDays(1);
                }
            }

            int weekCount; // 몇주차인지 확인하는 카운트
            weekSunday = new DateTime(); // 그 주차에 일요일을 구함

            if (new DateTime(currYear, currMonth, 1).DayOfWeek == DayOfWeek.Sunday)
                weekCount = -1;  // 주차를 구할때 첫일이 일요일이면 첫주차가 0이 아닌 1이 되므로 -1로 정의
            else
                weekCount = 0;//5 7 8 11 12 14 


            for (int i = 1; i <= currDay; i++) // 1일부터 현재날까지 돌면서 몇주차인지를 구함
            {
                if (new DateTime(currYear, currMonth, i).DayOfWeek == DayOfWeek.Sunday)
                {
                    weekSunday = new DateTime(currYear, currMonth, i);
                    weekCount++;
                }
            }
            startWeek = 0;
            startWeekDate = weekSunday;
            endWeek = 7;
            endWeekDate = startWeekDate.AddDays(7);
            if (weekSunday.Day + 7 > DateTime.DaysInMonth(weekSunday.Year, weekSunday.Month))
            {
                endWeek = (DateTime.DaysInMonth(weekSunday.Year, weekSunday.Month) + 1) - weekSunday.Day;
                endWeekDate = weekSunday.AddDays(endWeek - startWeek);
            }

            createWeek(weekSunday);
        }

        private void createObject()
        {
            makeTime();
            scList = new List<List<DateTime>>();
            overlapSCList = new List<List<int>>();
            dataRowList = new List<DataRow>();
        }

        private void clearObject()
        {
            isMainClick = false;
            insideMain.Invalidate();
            insideMain.Controls.Clear();
            scList.Clear();
            overlapSCList.Clear();
            dataRowList.Clear();
            scPan.Clear();
            for (int i = 0; i < colorPan.Count; i++)
                colorPan[i].Visible = false; // ----

            for (int i = 0; i < 7; i++)
            {
                m_Top_pan.Controls[i].Controls[0].ForeColor = Color.LightGray;
                m_Top_pan.Controls[i].Controls[1].ForeColor = Color.LightGray;
            }
        }
        List<List<DateTime>> scList = null;
        List<List<int>> overlapSCList = null;
        List<DataRow> dataRowList = null;

        private void createWeek(DateTime weekSunday)
        {
            createObject();

            int m = m_Top_pan.Controls.Count;
            for (int i = 0; i < 7; i++) // 몇일인지 표시
                m_Top_pan.Controls[i].Controls[1].Text = weekSunday.AddDays(i).Day.ToString();

            int turm = endWeek - startWeek;
            DataTable dt = null;

            if (db.FR_CD != null) // 친구 코드가 있다면 친구 일정 보여주기
                dt = dbs.Get_Week_Schedule(true, db.FR_CD, weekSunday.AddDays(startWeek), turm, db.IS_PB);
            else if (db.GR_CD != null) // 그룹이라면 그룹스케줄 표시
                dt = dbs.Get_Week_Schedule(false, db.GR_CD, weekSunday.AddDays(startWeek), turm, 1);
            else
                dt = dbs.Get_Week_Schedule(true, db.UR_CD, weekSunday.AddDays(startWeek), turm, db.IS_PB);
            for (int i = startWeek; i < endWeek; i++)
            {
                if (i == 0)
                {
                    m_Top_pan.Controls[i].Controls[0].ForeColor = Color.Red;
                    m_Top_pan.Controls[i].Controls[1].ForeColor = Color.Red;
                }
                else if (i == 6)
                {
                    m_Top_pan.Controls[i].Controls[0].ForeColor = Color.Blue;
                    m_Top_pan.Controls[i].Controls[1].ForeColor = Color.Blue;
                }
                else
                {
                    m_Top_pan.Controls[i].Controls[0].ForeColor = Color.Black;
                    m_Top_pan.Controls[i].Controls[1].ForeColor = Color.Black;
                }
            }
            for (int k = 0; k < dt.Rows.Count; k++)
            {
                DataRow dr = dt.Rows[k];
                scList.Add(new List<DateTime>());
                scList.Last().Add(DateTime.Parse(dr[4].ToString()));
                scList.Last().Add(DateTime.Parse(dr[5].ToString()));
                overlapSCList.Add(new List<int>());
                overlapSCList.Last().Add(0); // 곂치는 카운트
                overlapSCList.Last().Add(0); // 순서
                dataRowList.Add(dr);
                //CreateSCPan(ref dr, i);
                //MessageBox.Show(i.ToString() + " : " + Convert.ToDateTime(dr[4].ToString()).Hour.ToString());
            }
            //dt.Clear();
            overlapMethod();
            CreateSCPan();
        }



        private void overlapMethod()
        { // 곂치는 일정을 조사하는 재귀함수를 시작하기 위한 메서드
            int skipValue = -1;
            int maxValue = -1;
            for (int k = 0; k < scList.Count;)
            { // 모든 스케줄 만큼 반복한다
                if (skipValue > -1)
                {// 스킵을 위한 구문 재귀가 끝나면 곂치는 수만큼 스킵하는 용도
                    overlapSCList[k][0] = maxValue;
                    --skipValue; // 곂치는 개수만큼 스킵해줌
                    if (skipValue == -1) // 1이면 끝났으므로 maxValue 초기화
                        maxValue = -1;
                    ++k;
                    continue;
                }
                skipValue += overlapLoop(scList[k][1], k, -1, ref maxValue);
            } //scList : ([k] - 일정 카운트), ([1] : 0 - 시작일 , 1 - 끝나는일)
        }

        private int overlapLoop(DateTime preEnd, int currVal, int count, ref int maxValue)
        { // 곂치는 일정을 재귀함수로 찾는다. 곂칠시 사이즈, 순서를 지정하기위한 용도
            if (count > -1) // 처음 스케줄은 곂치는게 없으니 리턴구문을 스킵한다
            {
                if (scList[currVal][0].Day > preEnd.Day) // 현재 일이 전단계 일보다 더 크면 절대 곂칠수없다
                    return 0;
                else if (scList[currVal][0].Day == preEnd.Day) // 일이같지만 시간대가 곂치지않으면 곂칠수없다
                {
                    if (((preEnd.Hour * 10) + (preEnd.Minute / 10)) <= ((scList[currVal][0].Hour * 10) + (scList[currVal][0].Minute / 10)))
                        return 0; //  현재 시간, 분이 전단계보다 더 크다면 곂치지않는다.
                }
            }// overlapSCList : ([] : 일정 카운트), ([] : 0 - 최대로 곂치져있는 값, 1 -  현재 곂치는 순서) 
            overlapSCList[currVal][1] = ++count; // 곂치는게 몇번째 순서인지 지정
            maxValue = maxValue < count ? count : maxValue; // 곂치는대로 사이즈를 작아지기때문에 최대 몇번곂친건지를 위해 설정

            int size = 1; // 후에 리턴해 그만큼 곂치는지 검사하지 않아도된다, 자기자신은 항상 포함되니 1로 정의
            for (int k = currVal + 1; k < scList.Count;)
            {
                int value = overlapLoop(scList[currVal][1], k, count, ref maxValue);
                if (value == 0) // 결과가 0이 나오면 그다음을 볼필요도없이 리턴 (시간순서로 검색되기떄문에)
                    return size; // 지금까지 쌓아준 size를 리턴
                size += value; // 만약 1이상 나왔으면 더 찾아볼 필요성이있음
                k += value;
            }
            return size;
        }

        List<List<DateTime>> overDate = new List<List<DateTime>>();
        List<List<Panel>> scPan = new List<List<Panel>>();

        int[] panCountWidth = { 125, 63, 42, 31 }; //125, 62.5 , 41.66... ,31.25
        int[] panCountPosition = { 0, 63, 42, 31 }; // 사이즈만큼 더해줌여
        float[] panTextSize = { 12F, 9F, 8F, 7F };
        // 곂치는 개수에따라 width 사이즈를 조정해줄 배열
        public void CreateSCPan() // DataRow를 읽고 색을 입혀줌
        {
            for (int k = 0; k < scList.Count; k++)
            {
                //MessageBox.Show(dataRowList[k][1].ToString());
                DateTime startDate = scList[k][0];
                DateTime endDate = scList[k][1];
                Panel startPanel = day[(int)startDate.DayOfWeek][startDate.Hour];
                int startMinute = scList[k][0].Minute / 10;
                int startPlusMinute = startMinute * 8;
                Panel endPanel = day[(int)endDate.DayOfWeek][endDate.Hour];
                int endMinute = scList[k][1].Minute / 10;
                int endPlusMinute = endMinute * 8; // 끝나는시간에 더 추가해야할 height 크기

                if (startWeekDate > startDate)
                {
                    startDate = startWeekDate;
                    startPanel = day[(int)startDate.DayOfWeek][0];
                }

                Panel pan = new DoubleBufferPanel();
                pan.Name = dataRowList[k][0].ToString(); // 판넬에 코드 부여
                if (dataRowList[k][7].ToString() != "") // 널이 아니면 고유 색상지정
                    pan.BackColor = (new DBColor()).GetColorInsertCRCD(dataRowList[k][7].ToString(), 200);
                else // 널이면 랜덤
                    pan.BackColor = defaultColor;
                pan.MouseClick += new MouseEventHandler(OnPanelClick); // 판넬 클릭시 정보출력
                pan.MouseDoubleClick += new MouseEventHandler(OnPanelClick);

                if (overlapSCList[k][1] > 3) // 4개 이상 넘어가면
                {
                    for (int i = 0; i < scPan.Last().Count; i++)
                    {
                        Panel morePan = scPan.Last()[i];

                        if (morePan.Controls.Count != 0)
                            morePan.Controls[0].Visible = false;
                        if (morePan.Name.Length <= 7)
                        {
                            morePan.Paint += new PaintEventHandler(OnMorePaint);
                            morePan.MouseClick += new MouseEventHandler(OnMorePanelClick);
                        }
                        morePan.Name += dataRowList[k][0].ToString();
                        morePan.BackColor = defaultColor;
                    }
                    continue;
                }

                scPan.Add(new List<Panel>());
                int overlapNum = overlapSCList[k][0] > 3 ? 3 : overlapSCList[k][0];

                Label lb = CreateSCText(dataRowList[k], panTextSize[overlapNum]); // 텍스트를 만들어줌
                pan.Controls.Add(lb);

                if (startDate.Year != endDate.Year || startDate.Month != endDate.Month || startDate.Day != endDate.Day) // 시작일 끝일이 다름
                {
                    if (endWeekDate <= endDate) // 일주일 표시를 넘어가면
                    {
                        endDate = endWeekDate.AddSeconds(-1);
                        endPanel = day[(int)endDate.DayOfWeek][23];
                        endPlusMinute = 48;
                    }
                    pan.Location = new Point(startPanel.Location.X + (panCountPosition[overlapNum] * overlapSCList[k][1])
                        , (startPanel.Top + startPlusMinute)); //startPanel.Location.Y
                    pan.Size = new Size(panCountWidth[overlapNum], insideMain.Size.Height - (startPanel.Top + startPlusMinute));
                    pan.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pan.Size.Width, pan.Size.Height, 13, 13));
                    pan.Paint += new System.Windows.Forms.PaintEventHandler(OnPanPaint); // 사이드를 칠하기위함
                    scPan.Last().Add(pan);
                    insideMain.Controls.Add(pan);
                    for (int m = (int)startDate.DayOfWeek + 1; m < (int)endDate.DayOfWeek; m++) // 시작일 끝나는일 중간 일정
                    {
                        Panel middle = new DoubleBufferPanel();
                        middle.Name = pan.Name;
                        middle.BackColor = pan.BackColor;
                        middle.Size = new Size(panCountWidth[overlapNum], insideMain.Size.Height);
                        middle.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, middle.Size.Width, middle.Size.Height, 13, 13));
                        middle.Location = new Point(day[m][0].Location.X + (panCountPosition[overlapNum] * overlapSCList[k][1])
                        , day[m][0].Location.Y);
                        middle.Paint += new System.Windows.Forms.PaintEventHandler(OnPanPaint); // 사이드를 칠하기위함
                        middle.MouseClick += new MouseEventHandler(OnPanelClick);
                        middle.MouseDoubleClick += new MouseEventHandler(OnPanelClick);
                        scPan.Last().Add(middle);
                        insideMain.Controls.Add(middle);
                    }
                    if (startDate.DayOfWeek != DayOfWeek.Saturday)
                    {
                        Panel last = new DoubleBufferPanel();
                        last.Name = pan.Name;
                        last.BackColor = pan.BackColor;
                        last.Location = new Point(endPanel.Location.X + (panCountPosition[overlapNum] * overlapSCList[k][1]), 0);
                        last.Size = new Size(panCountWidth[overlapNum], endPanel.Top + endPlusMinute);
                        last.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, last.Size.Width, last.Size.Height, 13, 13));
                        last.Paint += new System.Windows.Forms.PaintEventHandler(OnPanPaint); // 사이드를 칠하기위함
                        last.MouseClick += new MouseEventHandler(OnPanelClick);
                        last.MouseDoubleClick += new MouseEventHandler(OnPanelClick);
                        scPan.Last().Add(last);
                        insideMain.Controls.Add(last);
                    }
                }
                else // 시작일 끝일이 같을경우
                {
                    pan.Location = new Point(startPanel.Location.X + (panCountPosition[overlapNum] * overlapSCList[k][1])
                        , (startPanel.Top + startPlusMinute));
                    pan.Size = new Size(panCountWidth[overlapNum], (endPanel.Top + endPlusMinute) - (startPanel.Top + startPlusMinute));
                    //(좌측상단여백, 우측상단여백, 컨트롤 넓이, 컨트롤 높이, 가로 모서리 원기울기, 세로 모서리 원기울기)
                    pan.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pan.Size.Width, pan.Size.Height, 13, 13));

                    //pan.BorderStyle = BorderStyle.FixedSingle;
                    // 사이즈 조절
                    pan.Paint += new System.Windows.Forms.PaintEventHandler(OnPanPaint); // 사이드를 칠하기위함
                    scPan.Last().Add(pan);
                    insideMain.Controls.Add(pan);
                }
            }
        }

        private void OnMorePaint(object sender, PaintEventArgs e)
        {
            var pan = (Panel)sender;
            Graphics g = e.Graphics;
            Pen p = new Pen(defaultColor, 3);
            int y = 0;

            while (y < pan.Height)
            {
                g.DrawLine(p, 0, y + 20, pan.Width, y);
                y += 20;
            }
            p.Dispose();
        }

        Panel clickPanel = new Panel();
        private void OnPanelClick(object sender, MouseEventArgs e)
        {
            isMainClick = false;
            insideMain.Invalidate();
            Panel pan = (Panel)sender;
            db.ExecuteReader("select * from SCHEDULE_TB where SC_CD = '" + pan.Name + "'");
            if (!db.Reader.Read())
                return;
            if (e.Clicks == 1)
            {
                clickPanel.BorderStyle = BorderStyle.None;
                pan.BorderStyle = BorderStyle.FixedSingle;
                clickPanel = pan;
                m_focus_dt = DateTime.Parse(db.Reader[4].ToString());

                if (db.FR_CD != null)
                    return;
                for (int i = 0; i < colorPan.Count; i++)
                    colorPan[i].Visible = true; // ----
            }
            if (e.Clicks == 2)
            {
                if (db.FR_CD != null)
                    return;
                Schedule ms = new Schedule(pan.Name);
                //ms.NameTxt = db.Reader[1].ToString();
                //ms.ExTxt = db.Reader[2].ToString();
                ms.FOCUS_DT = m_focus_dt;
                //ms.StateCheck = Int32.Parse(db.Reader[3].ToString());
                ms.ShowDialog();
                m_focus_dt = ms.StrDate;
            }
            //m_Str_focus.Text = m_focus_dt.ToString("yyyy년 MM월 dd일");
        }



        private void OnMorePanelClick(object sender, MouseEventArgs e)
        {// ...을 클릭했을경우
            for (int i = 0; i < colorPan.Count; i++)
                colorPan[i].Visible = false; // ----
            Panel pan = (Panel)sender;
            Week_MoreInfo wm = new Week_MoreInfo(pan.Name);
            wm.Location = pan.PointToScreen(new Point(e.X, e.Y));
            wm.Show();
            if (wm.DialogResult == DialogResult.OK)
            {
                m_focus_dt = wm.FocusDate;
                //m_Str_focus.Text = m_focus_dt.ToString("yyyy년 MM월 dd일");
            }
        }


        private void OnPanPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Panel pan = ((Panel)sender);
            Color panColor = pan.BackColor;
            Color sideColor = Color.FromArgb(((int)panColor.A), (int)panColor.R, (int)panColor.G, (int)panColor.B); // 좀 진하게
            Pen p = new Pen(sideColor);
            p.Width = 10;
            g.DrawLine(p, 0, 0, 0, pan.Size.Height);
        }

        private void OnMainClick(object sender, MouseEventArgs e)
        {// 클릭시 어디를 클릭했는지 찾고 찾는날에 시간을 반환해줌
            clickPanel.BorderStyle = BorderStyle.None;
            for (int i = 0; i < colorPan.Count; i++)
                colorPan[i].Visible = false; // ----

            if (e.Clicks == 1)
            {
                int x = 0;
                int y = 0;
                for (int i = 0; i < 7; i++)
                {
                    if (day[i][0].Right >= e.Location.X)
                    {
                        x = i;
                        break;
                    }
                }
                for (int i = 0; i < 24; i++)
                {
                    if (day[x][i].Bottom >= e.Location.Y)
                    {
                        y = i;
                        break;
                    }
                }
                DateTime clickDate = weekSunday.AddDays(x - startWeek);
                clickDate = clickDate.AddHours(y);
                m_focus_dt = clickDate;
                //m_Str_focus.Text = m_focus_dt.ToString("yyyy년 MM월 dd일");

                isMainClick = true;
                clickPan = day[x][y];
                insideMain.Invalidate();
            }
            else if (e.Clicks == 2)
            {
                if (db.FR_CD != null)
                    return;
                Schedule ms = new Schedule();
                ms.FOCUS_DT = m_focus_dt;
                ms.ShowDialog();
                m_focus_dt = ms.StrDate;
            }
        }

        private void OnColorClick(object sender, EventArgs e)
        {
            if (db.FR_CD != null)
                return;
            Panel pan = (Panel)sender;
            DBColor dbc = new DBColor();

            db.ExecuteNonQuery("UPDATE SCHEDULE_TB SET SC_CR_FK = '" + pan.Name +
                "' WHERE SC_CD = '" + clickPanel.Name + "'");
            //clickPanel.BackColor = dbc.GetColorInsertCRCD(pan.Name, 200);
            Color c = dbc.GetColorInsertCRCD(pan.Name, 200);
            int i = 0;
            for (; i < scPan.Count;)
            {
                if (clickPanel.Name == scPan[i][0].Name)
                    break;
                i++;
            }

            for (int k = 0; k < scPan[i].Count; k++)
            {
                scPan[i][k].BackColor = c;
            }
        }


        private Label CreateSCText(DataRow dr, float size)
        {
            Label txt = new Label();
            txt.TextAlign = ContentAlignment.TopLeft;
            txt.Font = new System.Drawing.Font(FontLibrary.HANDOTUM, size, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            txt.Location = new Point(5, 5);
            txt.AutoSize = true; // this.label1.Size = new System.Drawing.Size(46, 18);
                                 //txt.BackColor = Color.Transparent; // 안한게 더 예쁜데?
            if (size <= 10F)
                txt.Text = dr[1].ToString()[0].ToString(); // 한글자만
            else
                txt.Text = dr[1].ToString();

            return txt;
        }


        #region Garbage Method

        // overlapSCList 0행( 일정번호) 1행 (0열 : 곂치는 일정개수  1열: 곂칠때 순서)
        // scList 0행:( 일정번호) 1행 (0열 시작일 1열: 끝일)
        //private void overlapCount() // 순서를 for문으로 구현 -> 버그발생 ->  overlapMethod 로 재구현(재귀)
        //{ 
        //    for (int k = 0; k < scList.Count;)
        //    {
        //        int saveK = k;
        //        DateTime dtStart = scList[k][0];
        //        DateTime dtEnd = scList[k][1];
        //        int count = 0; // 순서가 몇번쨰인지

        //        for (int p = k + 1; p < scList.Count; p++)
        //        {// 시작일이 같을때 시간이 곂치면
        //            if (dtEnd.Day < scList[p][0].Day)
        //                break;
        //            else if (dtEnd.Day == scList[p][0].Day)
        //            {
        //                if (((dtEnd.Hour * 10) + (dtEnd.Minute / 10)) <= ((scList[p][0].Hour * 10) + (scList[p][0].Minute / 10)))
        //                    break;
        //            }

        //            if (dtEnd < scList[p][1])
        //                dtEnd = scList[p][1]; // 곂치면 그 아래 일정 끝나는 시간을 넣음
        //            overlapSCList[p][1] = ++count; // 그다음 순서니깐 더해준값을 넣음
        //            ++k;

        //        }
        //        for (int m = saveK; m <= saveK + count; m++) // 몇번 곂쳤는지 넣어줌
        //        {
        //            overlapSCList[m][0] = count;
        //        }
        //        ++k;
        //    }
        //}

        //private void muliSCPan(ref Panel cre, int i, ref DataRow dr, ref DateTime beginSC, ref DateTime endSC)
        //{

        //    if (beginSC.Day != endSC.Day) // 시작일과 끝일이 다르면
        //    {
        //        Panel beginPosition = day[Convert.ToInt32(beginSC.DayOfWeek)][beginSC.Hour];
        //        Panel endPosition = day[Convert.ToInt32(endSC.DayOfWeek)][endSC.Hour];

        //        cre.Size = new Size(beginPosition.Size.Width, insideMain.Size.Height - beginPosition.Top);
        //        // 다음날까지 이어져야되니 맨 아래까지 만든다
        //        for (int x = (int)beginSC.DayOfWeek + 1; x < (int)endSC.DayOfWeek; x++)
        //        {
        //            Panel middleDay = new Panel();
        //            middleDay.Size = new Size(cre.Width, insideMain.Height);
        //            middleDay.Location = new Point(day[x][0].Location.X, day[x][0].Location.Y);
        //            middleDay.Name = cre.Name;
        //            middleDay.BackColor = cre.BackColor;
        //            middleDay.Paint += new System.Windows.Forms.PaintEventHandler(OnPanPaint); // 사이드를 칠하기위함

        //            //overlapSCPan(ref middleDay, x, ref dr, ref beginSC, ref endSC);
        //            //statusSC[x].Add(middleDay);
        //            //insideMain.Controls.Add(middleDay);
        //        }

        //        Panel nextDay = new Panel();
        //        nextDay.Size = new Size(cre.Width, endPosition.Bottom);
        //        nextDay.Location = new Point(endPosition.Location.X, 0);
        //        nextDay.Name = cre.Name;
        //        nextDay.BackColor = cre.BackColor;
        //        nextDay.Paint += new System.Windows.Forms.PaintEventHandler(OnPanPaint); // 사이드를 칠하기위함

        //        //overlapSCPan(ref nextDay, (int)endSC.DayOfWeek, ref dr, ref beginSC, ref endSC);
        //        //statusSC[(int)endSC.DayOfWeek].Add(nextDay);
        //        //insideMain.Controls.Add(nextDay);
        //    }
        //}

        // 곂치는 일정 표시방법
        // 1. 곂치는 일정을 위에 덮어씌운다 (곂치는 일정이 많아질수록 버그발생) overlapSCPan
        // 2. 곂치는 일정을 작게 바꾸고 크기를 줄인다 overlapLoop , CreateSCPan
        //private void overlapSCPan(ref Panel cre, int i, ref DataRow dr, ref DateTime beginSC, ref DateTime endSC)
        //{
        //    if (statusSC[i].Any()) // 리스트에 엘리먼트가 있는지 확인
        //    {
        //        for (int k = 0; k < statusSC[i].Count; k++)
        //        {
        //            if (statusSC[i][k].Top <= cre.Top && statusSC[i][k].Bottom >= cre.Bottom)
        //            { // 자기가 완전히 안에 들어가면
        //                if (statusSC[i][k].Top == cre.Top && statusSC[i][k].Controls.Count > 0) // 만약 시작일도 곂쳐버리면 안에 들어가는 텍스트를 내림, 짤라진 레이블을위해 텍스트가 있으면도 포함
        //                    if (typeof(Label) == statusSC[i][k].Controls[0].GetType()) // 라벨이있는 판넬이면 라벨을 내림
        //                        cre.Controls[0].Location = new Point(10, cre.Controls[0].Location.Y + 30);
        //                cre.Location = new Point(6, cre.Top - statusSC[i][k].Top);
        //                statusSC[i][k].Controls.Add(cre);
        //                statusSC[i].Add(cre);
        //                break;
        //            }
        //            else if (statusSC[i][k].Bottom > cre.Top && statusSC[i][k].Bottom < cre.Bottom)
        //            { // 윗부분 들어가고 아랫부분 나올경우
        //                if (statusSC[i][k].Top == cre.Top && statusSC[i][k].Controls.Count > 0) // 만약 시작일도 곂쳐버리면 안에 들어가는 텍스트를 내림, 짤라진 레이블을위해 텍스트가 있으면도 포함
        //                    if (typeof(Label) == statusSC[i][k].Controls[0].GetType()) // 라벨이있는 판넬이면 라벨을 내림
        //                        cre.Controls[0].Location = new Point(10, cre.Controls[0].Location.Y + 30);

        //                Panel plus = new Panel();
        //                plus.Location = new Point(statusSC[i][k].Location.X + 6, statusSC[i][k].Bottom);
        //                if (beginSC.Day != endSC.Day) // 다음날까지 일정이있으면 맨 아래까지
        //                    plus.Size = new Size(statusSC[i][k].Width - 6, insideMain.Height - cre.Top);
        //                else
        //                    plus.Size = new Size(statusSC[i][k].Width - 6, cre.Bottom - statusSC[i][k].Bottom);
        //                plus.BackColor = cre.BackColor;
        //                plus.Name = dr[0].ToString();
        //                plus.Paint += new System.Windows.Forms.PaintEventHandler(OnPanPaint); // 사이드를 칠하기위함
        //                                                                                      //plus.BorderStyle = BorderStyle.FixedSingle;

        //                cre.Location = new Point(6, cre.Top - statusSC[i][k].Top);
        //                cre.Size = new Size(cre.Size.Width - 6, statusSC[i][k].Bottom - cre.Top);

        //                statusSC[i][k].Controls.Add(cre);
        //                insideMain.Controls.Add(plus);
        //                statusSC[i].Add(plus);
        //                break;
        //            }
        //            if (k == statusSC[i].Count - 1) // 하나도 해당없으면 기존에있는거 추가
        //            {
        //                statusSC[i].Add(cre);
        //                insideMain.Controls.Add(cre);
        //            }
        //        }
        //    }
        //    else // 하나도 해당없으면 기존에있는거 추가
        //    {
        //        statusSC[i].Add(cre);
        //        insideMain.Controls.Add(cre);
        //    }
        //}

        #endregion


    }
}

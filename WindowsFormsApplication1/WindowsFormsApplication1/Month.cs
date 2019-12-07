﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Month : Form
    {

        private DBConnection db = Program.DB;
        private DBSchedule sc_db = new DBSchedule();
        private DBColor cr_db = new DBColor();

        public Month()
        {
            InitializeComponent();
            AutoScrollMinSize = new Size(int.MinValue, int.MinValue);

            Set_SC_Panel();
        }

        private void dm_pan_Click(object sender, EventArgs e) // 일정 패널 클릭시 이벤트 처리
        { 
            for (int i = 1; i < 36; i++)
            {
                string Panel_nm = "MonthDay" + i.ToString() + "_panel"; // 해당 패널에 삽입
                Panel MonthPanel = (Panel)this.Controls.Find(Panel_nm, true)[0];
                MonthPanel.BorderStyle = BorderStyle.None;
            }
            ((Panel)((Panel)sender).Parent).BorderStyle = BorderStyle.FixedSingle;
            sc_db.FOCUST_DT = new DateTime(m_nowYear,m_nowMonth,Convert.ToInt32(((Panel)((Panel)sender).Parent).Controls[1].Text.ToString()));
        }

        private void dm_sc_Click(object sender, EventArgs e)// 일정 클릭시 이벤트 처리
        { 
            for (int i = 1; i < 36; i++)
            {
                string Panel_nm = "MonthDay" + i.ToString() + "_panel"; // 해당 패널에 삽입
                Panel MonthPanel = (Panel)this.Controls.Find(Panel_nm, true)[0];
                MonthPanel.BorderStyle = BorderStyle.None;
            }
            ((Panel)((Panel)((Label)sender).Parent).Parent).BorderStyle = BorderStyle.FixedSingle;
            sc_db.FOCUST_DT = new DateTime(m_nowYear, m_nowMonth, Convert.ToInt32(((Panel)((Panel)((Label)sender).Parent).Parent).Controls[1].Text.ToString()));
        }

        private void dm_dt_Click(object sender, EventArgs e)// 날짜 클릭시 이벤트 처리
        {
            for (int i = 1; i < 36; i++)
            {
                string Panel_nm = "MonthDay" + i.ToString() + "_panel"; // 해당 패널에 삽입
                Panel MonthPanel = (Panel)this.Controls.Find(Panel_nm, true)[0];
                MonthPanel.BorderStyle = BorderStyle.None;
            }
            ((Panel)((Label)sender).Parent).BorderStyle = BorderStyle.FixedSingle;
            sc_db.FOCUST_DT = new DateTime(m_nowYear, m_nowMonth, Convert.ToInt32(((Label)sender).Text.ToString()));
        }

        private void Set_SC_Panel() // 일정 패널 동적 생성, 동적 이벤트 생성
        {
            System.Windows.Forms.Panel pan;
            for (int i = 1; i < 36; i++)
            {
                string Panel_nm = "MonthDay" + i.ToString() + "_panel"; // 해당 패널에 삽입
                Panel MonthPanel = (Panel)this.Controls.Find(Panel_nm, true)[0];

                pan = new System.Windows.Forms.Panel(); // 일정 패널 동적 생성
                pan.VerticalScroll.Maximum = 0;
                pan.VerticalScroll.Visible = false;
                pan.AutoScroll = true;
                pan.Location = new System.Drawing.Point(0, 25);
                pan.Name = "Sc" + i.ToString() + "_pan";
                pan.Size = new System.Drawing.Size(MonthPanel.Width, MonthPanel.Height - 25);

                pan.Click += new System.EventHandler(this.dm_pan_Click);

                // 일정 패널 추가
                MonthPanel.Controls.Add(pan);
            }
        }

        // 클릭 되어 있는 날짜 설정
        private int m_nowYear; // 현재 연도
        private int m_nowMonth; // 현재 월
        private int m_nowDay; // 현재 일자
        private int m_nowWeek; // 현재 요일

        private DateTime m_FirstDay; // 현재 월의 1일
        private int m_LastDay; // 현재 월의 마지막 날
        private int m_FirstWeek; // 현재 월의 1일 요일

        private void Set_Month_Today()
        {
            // 오늘날짜(시스템날짜)에 맞게 월간 설정 함수

            for (int i = 1; i < 36; i++)
            {
                string Panel_nm = "MonthDay" + i.ToString() + "_panel"; // 해당 패널 클리어
                Panel MonthPanel = (Panel)this.Controls.Find(Panel_nm, true)[0];
                if (MonthPanel.Controls.Count > 1)
                {
                    MonthPanel.Controls[1].Dispose();
                }

                string sc_Panel_nm = "Sc" + i.ToString() + "_pan"; ; // 해당 일정 클리어
                Panel scPanel = (Panel)this.Controls.Find(sc_Panel_nm, true)[0];
                for (int j = 0; j < scPanel.Controls.Count; j++)
                {
                    scPanel.Controls[j].Dispose();

                    scPanel.VerticalScroll.Maximum = 0;
                    scPanel.VerticalScroll.Visible = false;
                    scPanel.HorizontalScroll.Maximum = 0;
                    scPanel.HorizontalScroll.Visible = false;
                }
                scPanel.Height = MonthPanel.Height - 25;
            }

            DateTime m_today = sc_db.FOCUST_DT;

            m_nowYear = m_today.Year; // 현재 연도
            m_nowMonth = m_today.Month; // 현재 월
            m_nowDay = m_today.Day; // 현재 일자
            m_nowWeek = (int)m_today.DayOfWeek; // 현재 요일

            m_LastDay = DateTime.DaysInMonth(m_nowYear, m_nowMonth); // 현재 월의 마지막 날
            m_FirstDay = new DateTime(m_nowYear, m_nowMonth, 1);
            m_FirstWeek = (int)m_FirstDay.DayOfWeek; // 현재 월의 1일 요일

            m_Year_btn.Text = m_nowYear.ToString() + "년";
            m_MonthNum.Text = m_nowMonth.ToString();

            Set_Month_Days(m_LastDay, m_FirstDay);
        }

        private void Set_Month_Days(int m_LastDay, DateTime m_day)
        {
            // 월의 모든 날짜 설정 함수
            System.Windows.Forms.Label label;
            for (int i = 1; i < m_LastDay + 1; i++)
            {
                int DayPenel_num = m_FirstWeek + i;
                string Panel_nm = "MonthDay" + DayPenel_num.ToString() + "_panel"; // 해당 패널에 삽입
                Panel MonthPanel = (Panel)this.Controls.Find(Panel_nm, true)[0];

                label = new System.Windows.Forms.Label(); // 날짜 레이블 동적 생성
                label.Location = new System.Drawing.Point(5, 5);
                label.Name = "Day" + i.ToString() + "_lbl";
                label.Size = new System.Drawing.Size(25, 15);
                label.Text = i.ToString();

                label.Click += new System.EventHandler(this.dm_dt_Click);

                if (DayPenel_num % 7 == 0)
                {
                    label.ForeColor = Color.RoyalBlue;
                }
                else if ((DayPenel_num - 1) % 7 == 0)
                {
                    label.ForeColor = Color.Red;
                }

                // 날짜 추가
                MonthPanel.Controls.Add(label);
                //MonthPanel.Click += new System.EventHandler(this.dm_pan_Click);

                // 일정 추가
                Set_Schedule(m_day.AddDays(i - 1), DayPenel_num);
            }

        }

        private void Set_Schedule(DateTime NowDay, int DayPenel_num)
        {
            // 일정 표시 함수
            DataTable sc_day_tb = sc_db.Get_Day_Schedule(true, db.UR_CD, NowDay);
            DataRow[] rows = sc_day_tb.Select();
            int lbl_nm = (int)NowDay.Day; // 현재 일자

            string Panel_nm = "Sc" + DayPenel_num.ToString() + "_pan"; // 해당 패널에 삽입
            Panel ScPanel = (Panel)this.Controls.Find(Panel_nm, true)[0];

            int bs_sc = ScPanel.Controls.Count; // 이어지던 일정
            int bs_y = 0; // 이어지는 일정의 로케이션
            int lo_y = -25; // 이어이는 일정 위의 로케이션
            List<int> bs_lo = new List<int>(); // 비어있는 로케이션 리스트
            int lo_nm = 0; // 로케이션 리스트 커서
            int add_lbl_now = 0;

            if (bs_sc > 0) // 이어지던 일정이 있을 시
            {
                for (int i = 0; i < bs_sc; i++)
                {
                    int now_bs = Convert.ToInt32(ScPanel.Controls[i].Name.Substring(ScPanel.Controls[i].Name.Length-1,1))-1;
                    if (ScPanel.Controls[i].Location.Y != 25*now_bs) // 중간에 비어있는 공간이 있으면
                    {
                        bs_lo.Add(25 * i);
                    }
                }

                if (ScPanel.Controls[0].Location.Y > 0) // 이어지던 일정이 상위에 존재하지 않을시
                {
                    bs_y = ScPanel.Controls[0].Location.Y - 25;
                }
                else
                {
                    lo_y = bs_sc;
                }

            }

            System.Windows.Forms.Label label;
            for (int i = 0; i < rows.Length; i++)
            {

                lo_y += 25;

                label = new System.Windows.Forms.Label(); // 레이블 동적 생성
                label.Location = new System.Drawing.Point(0, lo_y); // 이어지는 일정 밑에 삽입
                label.Name = "sc" + lbl_nm.ToString() + "_lbl" + i.ToString();
                label.Size = new System.Drawing.Size(ScPanel.Width, 25);
                label.Padding = new Padding(5, 0, 5, 0);
                label.Text = rows[i]["SC_NM"].ToString();
                label.TextAlign = ContentAlignment.MiddleLeft;

                label.Click += new System.EventHandler(this.dm_sc_Click);

                if (bs_lo.Count > lo_nm)
                {
                    
                    if (lo_y < bs_lo[lo_nm]) // 겹치는 일정에 삽입된다면
                    {
                        lo_y = bs_lo[lo_nm];
                        label.Location = new System.Drawing.Point(0, lo_y); ;
                    }
                    lo_nm += 1;

                }

                Color sc_cr_bs; // 일정의 SC_CR_FK - 베이스컬러
                Color sc_cr_ft; // 일정의 SC_CR_FK - 글자컬러

                if (rows[i]["SC_CR_FK"] != DBNull.Value) // CR_FK이 있다면 해당 컬러 설정
                {
                    sc_cr_bs = cr_db.GetColorInsertCRCD(rows[i]["SC_CR_FK"].ToString());
                    sc_cr_ft = Color.Black;
                }
                else // CR_FR이 없다면 기본 컬러 설정
                {
                    sc_cr_bs = Color.Gainsboro;
                    sc_cr_ft = Color.Black;
                }

                if (lbl_nm < Convert.ToDateTime(rows[i]["SC_END_DT"]).Day) // 하루종일 이상의 일정이라면 백컬러 지정
                {
                    label.BackColor = sc_cr_bs;
                    label.ForeColor = Color.Black;

                    TimeSpan df_time = Convert.ToDateTime(rows[i]["SC_END_DT"]).Subtract(Convert.ToDateTime(rows[i]["SC_STR_DT"]));
                    if (df_time.Hours > 0 || df_time.Days > 1) // 다음날까지 이어지는 일정이라면 다음 패널에도 레이블 추가
                    {
                        if (df_time.Hours > 0) { df_time = df_time.Add(TimeSpan.FromDays(1)); } // 시간이 넘는다면 패널을 하나 더 생성
                        for (int j = 1; j < df_time.Days; j++) // 차이나는 일자만큼 해당 패널에 추가레이블 생성
                        {
                            if(j == 1) add_lbl_now += 1;
                            System.Windows.Forms.Label add_label;
                            add_label = new System.Windows.Forms.Label();
                            add_label.Location = new System.Drawing.Point(0, label.Location.Y);
                            add_label.Name = "add_sc" + (lbl_nm + j).ToString() + "_lbl" + (bs_sc + add_lbl_now).ToString();
                            add_label.Size = new System.Drawing.Size(ScPanel.Width, 25);
                            add_label.BackColor = sc_cr_bs;
                            
                            add_label.Click += new System.EventHandler(this.dm_sc_Click);

                            string Add_Panel_nm = "Sc" + (DayPenel_num+j).ToString() + "_pan"; // 해당 패널에 삽입
                            Panel Add_ScPanel = (Panel)this.Controls.Find(Add_Panel_nm, true)[0];
                            Add_ScPanel.Controls.Add(add_label);
                        }
                    }
                }
                else
                {
                    label.BackColor = Color.Transparent;
                    label.ForeColor = sc_cr_bs;
                }


                ScPanel.Controls.Add(label); // 패널에 레이블 추가
            }

        }

        private void Month_Load(object sender, EventArgs e)
        {
            Set_Month_Today(); // 오늘 날짜 세팅
        }

        private void LastMonth_btn_Click(object sender, EventArgs e) // 전 달 보기
        {
            sc_db.FOCUST_DT = sc_db.FOCUST_DT.AddMonths(-1);
            Set_Month_Today();
        }

        private void NextMonth_btn_Click(object sender, EventArgs e) // 후 달 보기
        {
            sc_db.FOCUST_DT = sc_db.FOCUST_DT.AddMonths(1);
            Set_Month_Today();
        }
    }
}

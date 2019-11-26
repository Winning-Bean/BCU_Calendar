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
    public partial class Login : Form
    {
        bool isSighUp, isSamePass;

        DBConnection db = null;

        public Login()
        {
            InitializeComponent();
            // 초기값 설정
            isSighUp = false;
            isSamePass = false;
            panel5.Visible = false;
            panel6.Visible = false;
            textBox3.Visible = false;
            textBox4.Visible = false;
            label3.Visible = false;
            textBox1.Text = "ID";
            textBox2.Text = "Password";
            textBox1.ForeColor = Color.Gray;
            textBox2.ForeColor = Color.Gray;
            textBox3.ForeColor = Color.Gray;
            textBox4.ForeColor = Color.Gray;

            try
            {
                db = new DBConnection();
            }catch(Exception e)
            {
                MessageBox.Show("데이터베이스 연결 실패\n" + e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!(isSighUp))
            {
                panel5.Visible = true;
                panel6.Visible = true;
                textBox3.Visible = true;
                textBox4.Visible = true;
                button1.Text = "Sign-up";
                textBox1.Text = "ID";
                textBox2.Text = "Password";
                textBox2.PasswordChar = '\0';
                textBox3.Text = "Name";
                textBox4.Text = "Rewrite Password";
                textBox1.ForeColor = Color.Gray;
                textBox2.ForeColor = Color.Gray;
                textBox3.ForeColor = Color.Gray;
                textBox4.ForeColor = Color.Gray;
                isSighUp = true;
            }
            else
            {
                panel5.Visible = false;
                panel6.Visible = false;
                textBox3.Visible = false;
                textBox4.Visible = false;
                button1.Text = "Sign-in";
                textBox1.Text = "ID";
                textBox2.Text = "Password";
                label3.Visible = false;
                textBox1.ForeColor = Color.Gray;
                textBox2.ForeColor = Color.Gray;
                textBox3.ForeColor = Color.Gray;
                textBox4.ForeColor = Color.Gray;
                isSighUp = false;

            }

        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (isSighUp)
            {
                if (isSamePass)
                {
                    String strID = textBox1.Text;
                    String strPass = textBox2.Text;
                    String strName = textBox3.Text;
                    try
                    {
                        string command = "insert into USER_TB values(seq_userid.nextval, '" + strID + "', '" + strPass + "', '" + strName + "')";

                        if (db.ExecuteNonQuery(command) < 1)
                        {
                            MessageBox.Show("데이터베이스 오류!!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        MessageBox.Show("회원가입 되었습니다!", "완료", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        button2.PerformClick();
                    }
                    catch (DataException DE)
                    {
                        MessageBox.Show("데이터베이스 오류!! \n" + DE.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception E)
                    {
                        MessageBox.Show("데이터베이스 오류!! \n" + E.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                String strID = textBox1.Text;
                String strPass = textBox2.Text;
                String strName = textBox3.Text;
                try
                {
                    db.ExecuteReader("select * from USER_TB where (UR_ID = '" + strID + "') AND (UR_PW = '" + strPass + "')");
                    if (db.Reader.Read())
                    {
                        MessageBox.Show("로그인 되었습니다!", "완료", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        db.Close();
                        Hide();
                        Form Login = new Login();
                        Login.ShowDialog();
                        Close();
                        return;

                    }
                    MessageBox.Show("로그인 실패하였습니다!!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    db.Close();
                    return;
                }
                catch (DataException DE)
                {
                    MessageBox.Show("데이터베이스 오류!! \n" + DE.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        private void label2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void textBox4_Enter(object sender, EventArgs e)
        {
            textBox4.Text = "";
            textBox4.PasswordChar = '*';
            textBox4.ForeColor = Color.Black;
        }

        private void textBox3_Enter(object sender, EventArgs e)
        {
            textBox3.Text = "";
            textBox3.ForeColor = Color.Black;
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox1.ForeColor = Color.Black;
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            textBox2.Text = "";
            textBox2.PasswordChar = '*';
            textBox2.ForeColor = Color.Black;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (isSighUp)
            {
                if (!(textBox2.Text.Equals(textBox4.Text)))
                {
                    isSamePass = true;
                    label3.Text = "비밀번호가 같지 않습니다.";
                    label3.ForeColor = Color.Red;
                    label3.Visible = true;
                }
                else
                {
                    isSamePass = false;
                    label3.Text = "비밀번호가 같습니다.";
                    label3.ForeColor = Color.Green;
                }
            }
        }


        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (!(textBox4.Text.Equals(textBox2.Text)))
            {
                isSamePass = true;
                label3.Text = "비밀번호가 같지 않습니다.";
                label3.ForeColor = Color.Red;
                label3.Visible = true;
            }
            else
            {
                isSamePass = false;
                label3.Text = "비밀번호가 같습니다.";
                label3.ForeColor = Color.Green;
            }
        }
    }
}
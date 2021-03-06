﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApplication1
{
    public partial class Form2 : Form
    {

        string newpass;
        string depath = @"E:\Password.txt";
        string s;
        string key = "кокон";
        string key_on_s = ""; //Ключ длиной строки
        int x = 0, y = 0; //Координаты нового символа из таблицы Виженера
        int registr = 0; //Регистр символа
        char dublicat;
        string result = "";
        bool flag;
        int r = 0;

       
        public Form2()
        {
            InitializeComponent();
        }

        public static char tabula_recta(int p, int r)
        {
            int shift = 0;
            char[,] tabula_recta = new char[32, 32]; //Таблица Виженера
            string alfabet = "абвгдежзийклмнопрстуфхцчшщьыъэюя";
            //Формирование таблицы
            for (int i = 0; i < 32; i++)
                for (int j = 0; j < 32; j++)
                {
                    shift = j + i;
                    if (shift >= 32) shift = shift % 32;
                    tabula_recta[i, j] = alfabet[shift];
                }
            return tabula_recta[p, r];
        }


        private void Form2_Load(object sender, EventArgs e)
        {

            
        }


        private void button2_Click(object sender, EventArgs e)
        {
           
     }
           
        

        private void label1_Click(object sender, EventArgs e)
        {

        }

        public string Decode()
        {
            s = File.ReadAllText(depath);
            //Выполение дешифрования
            //Формирование строки, длиной шифруемой, состоящей из повторений ключа
            for (int i = 0; i < s.Length; i++)
            {
                key_on_s += key[i % key.Length];
            }
            //Дешифрование при помощи таблицы
            for (int i = 0; i < s.Length; i++)
            {
                //Если не кириллица
                if (((int)(s[i]) < 1040) || ((int)(s[i]) > 1103))
                    result += s[i];
                else
                {
                    //Поиск в первом столбце строки, начинающейся с символа ключа
                    int l = 0;
                    flag = false;
                    //Пока не найден символ
                    while ((l < 32) && (flag == false))
                    {
                        //Если символ найден
                        if (key_on_s[i] == tabula_recta(l, 0))
                        {
                            //Запоминаем в х номер строки
                            x = l;
                            flag = true;
                        }
                        l++;
                    }
                    //Уменьшаем временно регистр прописной буквы
                    if ((Convert.ToInt16(s[i]) < 1072) && (Convert.ToInt16(s[i]) >= 1040))
                    {
                        dublicat = Convert.ToChar(Convert.ToInt16(s[i]) + 32);
                        registr = 1;
                    }
                    else
                    {
                        registr = 0;
                        dublicat = s[i];
                    }
                    l = 0;
                    flag = false;
                    //Пока не найден столбец в первой строке с символом строки
                    while ((l < 32) && (flag == false))
                    {
                        //Проверка совпадения
                        if (dublicat == tabula_recta(x, l))
                        {
                            //Запоминаем номер столбца
                            y = l;
                            flag = true;
                        }
                        l++;
                    }
                    //Увеличиваем регистр буквы до прописной
                    if (registr == 1)
                    {
                        //Изменяем символ на первоначальный регистр
                        dublicat = Convert.ToChar(Convert.ToInt16(tabula_recta(0, y)) - 32);
                        result += dublicat;
                    }
                    else
                        result += tabula_recta(0, y);
                }
            }
            return result;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == Decode())
            {
                Hide();
                MessageBox.Show("Новый пароль создан");
            }
            else
            {
                    MessageBox.Show("Повторите попытку");
                    textBox1.Clear();
                    Application.Exit();
                   
                }  
        }



        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            textBox1.UseSystemPasswordChar = true;
        }
    }
}

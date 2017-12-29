using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace WindowsFormsApplication1
{
    public partial class Form2 : Form
    {
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
            search_external_drives(comboBox1);    //поиск внешних накопителей
            comboBox1.SelectedIndex = 0;
        }


        private void search_external_drives(ComboBox input) //поиск носителей
        {
            string mydrive;
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady && (d.DriveType == DriveType.Removable))
                {
                    mydrive = d.Name;
                    input.Items.Add(mydrive);
                }
            }
            if (input.Items.Count == 0)
            {
                input.Items.Add("Внешние носители отсутствуют");
            }
        }

        class FileReader //класс для работы с носителями информации (жесткий диск - файл) работа с жестким диском - путем открытия файла.
        {
            const uint GENERIC_READ = 0x80000000; //для чтения
            const uint GENERIC_WRITE = 0x40000000; //для записи
            const uint OPEN_EXISTING = 3; //тип открытия файла
            const uint FILE_SHARE_READ = 0x00000001;
            const uint FILE_SHARE_WRITE = 0x00000002;
            System.IntPtr handle; //дескриптор окна
            [System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
            //Kernel32.dll - динамически подключаемая библиотека, предоставляющая приложениям многие базовые функции
            //API Win32, в частности: управление памятью, операции ввода/вывода, создание процессов и потоков и функции синхронизации.
            //System.Runtime.InteropServices - пространство имен
            //DllImport - вызов неуправляемого кода из управляемого приложения
            //"kernel32" - библиотека
            //SetLastError - значение true, чтобы показать, что вызывающий объект вызовет SetLastError
            //ThrowOnUnmappableChar - исключение каждый раз Interop маршалер встречает unmappable характер
            //CharSet = System.Runtime.InteropServices.CharSet.Ansi - Указывает, какой набор знаков должны использовать маршалированные строки
            unsafe static extern System.IntPtr CreateFile
            (
            string FileName, // имя файла
            uint DesiredAccess, // режим доступа
            uint ShareMode, // режим 
            uint SecurityAttributes, // атрибуты безопасности
            uint CreationDisposition, // как создать
            uint FlagsAndAttributes, // атрибуты файла 
            int hTemplateFile // дескриптор файла шаблона
            );
            [System.Runtime.InteropServices.DllImport("kernel32", SetLastError = false)]
            unsafe static extern bool ReadFile
            (
            System.IntPtr hFile, // дескриптор файла
            void* pBuffer, // буфер данных
            int NumberOfBytesToRead, // количество байт для чтения
            int* pNumberOfBytesRead, // количество прочитанных байт 
            int Overlapped // перекрывающий буфер
            );
            [System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true)]
            unsafe static extern bool WriteFile
            (
            System.IntPtr hFile, // дескриптор файла
            void* pBuffer, // буфер данных 
            int NumberOfBytesToWrite, // количество байт для чтения
            int* pNumberOfBytesWritten, // количество прочитанных байт 
            int Overlapped // перекрывающийся буфер
            );
            [System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true)]
            unsafe static extern bool CloseHandle
            (
            System.IntPtr hObject // обращение к объекту
            );
            public bool OpenRead(string FileName) //работа с нулевым сектором
            {
                // открытие существующего файла для чтения 
                handle = CreateFile(FileName, GENERIC_READ, FILE_SHARE_READ, 0, OPEN_EXISTING, 0, 0); //параметры открытия
                //в дискрипторе окна вызывается ф-ия создания файла с параметрами чтения и открытия 
                if (handle != System.IntPtr.Zero) //если файл существует, озвращаем T, нет - F
                //IntPtr - нулевое поле
                {
                    return true; //устройство найдено
                }
                else
                {
                    return false; //устройство не надено
                }
            }
            public bool OpenWrite(string FileName)
            {
                // открытие существующего файла для записи 
                handle = CreateFile(FileName, GENERIC_WRITE, FILE_SHARE_WRITE, 0, OPEN_EXISTING, 0, 0); //параметры открытия
                //handle - дескриптор окна
                if (handle != System.IntPtr.Zero)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            unsafe public int Read(byte[] buffer, int count) //чтение ячеек flash
            //сборщик мусора среды CLR может 
            //Во избежание этого блок fixed используется для получения указателя на память и его пометки таким образом,
            //что его не смог переместить сборщик мусора. В конце блока fixed память снова становится доступной для перемещения путем сборки мусора.
            //Эта способность называется декларативным закреплением.
            {
                int n = 0;
                   fixed (byte* p = buffer)
                  {
                    if (!ReadFile(handle, p, count, &n, 0)) //параметры чтения
                    {
                       return 0;
                   }
                 }
                return n;
            }
            unsafe public int Write(byte[] buffer, int index, int count) //запись ячеек flash
            {
                int n = 0;
                fixed (byte* p = buffer) //Оператор fixed задает указатель на управляемую переменную
                    //и "закрепляет" эту переменную во время выполнения оператора.
                     {
                         if (!WriteFile(handle, p + index, 512, &n, 0)) //параметры записи
                        {
                           return 0;
                      }
                     }
                    return n;
            }
            public bool Close()
            //bool - используется для объявления переменных для хранения логических значений, true и false.
            {
                return CloseHandle(handle); //закрыть дискриптор откна
            }
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
        public string Recode(string er)
        {
            result = null;
            key_on_s = null;
            s = er;
            //Выполение шифрования
            //Формирование строки, длиной шифруемой, состоящей из повторений ключа
            for (int i = 0; i < s.Length; i++)
            {
                key_on_s += key[i % key.Length];
            }
            //Шифрование при помощи таблицы
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
                        if (dublicat == tabula_recta(0, l))
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
                        dublicat = Convert.ToChar(Convert.ToInt16(tabula_recta(x, y)) - 32);
                        result += dublicat;
                    }
                    else
                        result += tabula_recta(x, y);
                }
            }
            //Вывод на экран зашифрованной строки
            //   File.WriteAllText(depath, result); //Вывод результата в файл
            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"F:\\");
            string password_1 = "";
            string password_2 = "";
            password_1 = textBox1.Text;
            password_2 = textBox2.Text;
            string drive = "\\\\.\\" + comboBox1.Text.Remove(2);
            string codpass = Recode(password_1);//кодирование пароля
            byte[] ByteBuffer = new byte[512];//задаем размер буфера
            byte[] temp = new byte[8];
            bool flag = true;
            FileReader fr = new FileReader();
            if (fr.OpenRead(drive)) //вызов для чтения
            {
                int count = fr.Read(ByteBuffer, 512);

                for (int i = 54; i < 62; i++)
                {
                    temp[i - 54] = ByteBuffer[i];
                }
                fr.Close();

                byte[] oldpass = new byte[8];
                for (int i = 0; i < codpass.Length; i++)
                {
                    oldpass[i] = (byte)codpass[i];
                }

                for (int i = 0; i < 8; i++)
                {
                    if (temp[i] != oldpass[i])
                        flag = false;
                }
                if (flag != false)
                {
                    if (fr.OpenWrite(drive))
                    {
                        string codnewpass;
                        codnewpass = null;
                        codnewpass = Recode(textBox2.Text);
                        byte[] newpass = new byte[8];
                        for (int i = 0; i < codnewpass.Length; i++)
                        {
                            newpass[i] = (byte)codnewpass[i];
                        }
                        for (int i = 54; i < 62; i++)
                        {
                            ByteBuffer[i] = newpass[i - 54];
                        }
                        fr.Write(ByteBuffer, 0, 512);
                        this.Hide();
                        MessageBox.Show("Пароль сменен");

                        Application.Exit();
                    }
                }
                else
                {
                    MessageBox.Show("Попыток:" + (1 - r));
                    r++;
                    if (r >= 3)
                        Application.Exit();
                //    codpass = null;
                 //   temp = null;
                //    oldpass = null;
                    textBox1.Clear();
                    textBox2.Clear();

                }
            }
        
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            textBox1.UseSystemPasswordChar = true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox2.UseSystemPasswordChar = true;
        }
    }
}

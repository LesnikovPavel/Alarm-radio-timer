using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// NAudio libraries
using NAudio;
using NAudio.Wave;
//

namespace Alarm_clock
{
    public partial class Form1 : Form
    {
        bool IsTimeSet = false; // булевая переменная, проверяющая установленно ли время
        int i = 0; // переменная, для подсчёта времени звонка будильника (1 минута)
        bool IsPlaying = false; // булевая переменная, проверяющая звучит ли будильник
        int k = 0; // переменная, для подсчёта времени после сброса звонка (5 минут)
        bool NotPlayingForNow = false; // булевая переменная для "сейчас не играет, заиграет через 5 минут"

        public Form1()
        {
            InitializeComponent();
        }

        Timer t = new Timer(); //таймер для настоящих часов (чтобы "тикали" на экране)
        Timer timer1 = new Timer(); // таймер для мигания двоеточий в часах

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // не даёт пользователю изменять размер формы
            dont_show(); // при открытии приложения не показываем будильник
            
            t.Interval = 1000; // интервал - 1 секунда
            t.Tick += new EventHandler(this.t_Tick); 
            t.Start(); // запускаем отсчёт по одной секунде ("таймер")

            timer1.Interval = 500; // интервал - 0.5 секунд
            timer1.Tick += new EventHandler(this.timer1_Tick);
            timer1.Start(); // запускаем

            label4.Text = "ЧЧ:ММ:СС"; // стандартный текст в поле пользовательского времени

            label6.Text = ""; // HH real
            label8.Text = ""; // MM real
            label10.Text = ""; // SS real

            label7.Text = ""; // : real
            label9.Text = ""; // : real

            string[] music_genere = { "Рок", "Поп", "Джаз" }; // строковый массив с "названиями" радиостанций
            listBox1.Items.AddRange(music_genere); // добавляем в listBox1 вышеописанные "названия"

            label15.Text = "";
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////              БУДИЛЬНИК               ///////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        int hh_real; // Реальное время определённое глобально для синхронизации
        int mm_real;
        int ss_real;

        int hh_alarm; //Пользовательское время
        int mm_alarm;
        int ss_alarm = 0; // ноль, потому что, в принципе, нужно лишь для запуска будильника

        private void t_Tick(object sender, EventArgs e) //установка реального времени в label3
        {           
            int hh = DateTime.Now.Hour; // Реальное время
            int mm = DateTime.Now.Minute;
            int ss = DateTime.Now.Second;
            
            label6.Text = "";
            label8.Text = ""; // прост пустота 
            label10.Text = "";

            if (hh < 10)
                label6.Text += "0" + hh;
            else
                label6.Text += hh;
            
            if (mm < 10)
                label8.Text += "0" + mm;
            else
                label8.Text += mm;
            
            if (ss < 10)
                label10.Text += "0" + ss;
            else
                label10.Text += ss;

            synchronize_time(); // проверка времени
            hh_real = hh;
            mm_real = mm;
            ss_real = ss;

            if (IsPlaying == true) // для того чтобы будильник "пел" ровно одну минуту
            {
                i++;
                if (i == 10) //вообще надо 60, но пока для удобства 20 секунд
                {
                    stop_sound();
                    i = 0;
                    NotPlayingForNow = true;
                    IsPlaying = false; // ????
                }
            }

            if (NotPlayingForNow == true)
            {
                k++;
                if (k == 10) // вообще то надо 300
                {
                    alarm_sound();
                    k = 0;
                    NotPlayingForNow = false;
                    IsPlaying = false;
                }
            }

            string title = "";
            if (RadioPlaying == true)
            {
                if (player.currentMedia != null)
                {
                    if (player.controls.currentItem.name != null) // прописываем в title
                        title = player.controls.currentItem.name;
                    this.Text = "Now playing: " + title;
                }

            }

            timer_time_change(); // timer check
        }

        private void timer1_Tick(object sender, EventArgs e) //тик для двоеточий
        {
            IsInputTimeChanged(); //проверяет изменилось ли время в полях, что ввёл пользователь
            label7.Text = "";
            label9.Text = "";

            label7.Visible = !label7.Visible; // видимость/невидимость двоеточия
            label7.Text += ":";

            label9.Visible = !label9.Visible; // видимость/невидимость двоеточия
            label9.Text += ":";
        }

        private bool IsAlarmTimeOK() // сначала проверка праивльности введения времени
        {
            bool flaggy = true;

            if (string.IsNullOrWhiteSpace(numericUpDown1.Text)) //определение hh_alarm;
                flaggy = false;
            else
            {
                hh_alarm = Convert.ToInt32(numericUpDown1.Text);
                if (hh_alarm < 0 || hh_alarm > 24)
                    flaggy = false;
            }
            
            if (string.IsNullOrWhiteSpace(numericUpDown2.Text)) //определение mm_alarm;
                flaggy = false;
            else
            {
                mm_alarm = Convert.ToInt32(numericUpDown2.Text);
                if (mm_alarm < 0 || mm_alarm > 60)
                    flaggy = false;
            }
            
            if (flaggy == false)
                MessageBox.Show("Вы указали время не правильно.\r\nПовторите ввод или обратитесь к справке.", "Ошибка", MessageBoxButtons.OK);
            return flaggy;
        }
        
        private void alarm_time() // потом передача введённого времени по нажатию button1 (Установить) в label4
        {
            label4.Text = "";
            int hh_alarm = Convert.ToInt32(numericUpDown1.Text);
            int mm_alarm = Convert.ToInt32(numericUpDown2.Text);
            string time = "";


            if (hh_alarm < 10)
                time+= "0" + numericUpDown1.Text;
            else
                time += numericUpDown1.Text;
            if (hh_alarm == 24)
                time = "00";

            time += " : ";

            if (mm_alarm == 60)
                time += "00";
            else
            {
                if (mm_alarm < 10)
                    time += "0" + numericUpDown2.Text;
                else
                    time += numericUpDown2.Text;
            }
            
            time += " : 00";
            
            label4.Text = time; //установка пользовательского времени
        }
        
        private void button1_Click(object sender, EventArgs e) // Вкл. будил
        {
            bool flag;

            flag = IsAlarmTimeOK();
            if (flag == true)
            {
                alarm_time(); //вызов установки времени будильника
                IsTimeSet = true;
            }

            if (IsTimerSet == true) //выключаем таймер, не важно вкл он или нет
                IsTimerSet = false;
            button8_Click(sender, e); // выключаем радио, не важно вкл он или нет
        }

        bool time_has_come = false;

        private void synchronize_time() // функция ежесекундной проверки времени
        {
            if (IsTimeSet == true)
                if (hh_real == hh_alarm)
                    if (mm_real == mm_alarm)
                        if (ss_real == ss_alarm)
                        {
                            alarm_sound();
                            time_has_come = true;
                        }
        }

        //NAudio
        IWavePlayer waveOutDevice;
        AudioFileReader audioFileReader;

        private void alarm_sound()
        {
            this.Text = "Alarm";
            this.Icon = new Icon("C:\\Users\\Yokz\\Desktop\\Alarm_clock\\alarm_on_icon.ico");
            waveOutDevice = new WaveOut(); //необходимо для проигрывания мп3
            audioFileReader = new AudioFileReader("C:\\Users\\Yokz\\Desktop\\Alarm_clock\\Song.mp3"); //сам mp3
            
            waveOutDevice.Volume = (float)0.1; //Громкость звука (от 0 до 1)
            waveOutDevice.Init(audioFileReader);
            waveOutDevice.Play();
            IsPlaying = true;
        }

        private void stop_sound()
        {
            if (waveOutDevice != null)
                waveOutDevice.Stop();
            IsPlaying = false;
            this.Text = "Alarm";
            this.Icon = new Icon("C:\\Users\\Yokz\\Desktop\\Alarm_clock\\main_alarm_icon.ico");
        }
        
        private void button2_Click(object sender, EventArgs e) //Выкл. будил.
        {
            stop_sound();
            k = 0;
            NotPlayingForNow = false;
        }

        private void button3_Click(object sender, EventArgs e) //Сброс
        {
            //label4.Text = "ЧЧ:ММ:СС";
            stop_sound();
            k = 0;
            if (time_has_come == true)
                NotPlayingForNow = true;
            time_has_come = false;
        }

        private void IsInputTimeChanged() // (тик на это выше) провека введённого пользователем времени
        {
            if (numericUpDown1.Text == "24")
                numericUpDown1.Text = "0";

            if (numericUpDown2.Text == "60")
                numericUpDown2.Text = "0";
        }

        private void button4_Click(object sender, EventArgs e) // Уст. время
        {
            bool flag;
            flag = IsAlarmTimeOK();
            if (flag == true)
            { alarm_time(); } //вызов установки времени будильника

            if (IsTimerSet == true) //выключаем таймер, не важно вкл он или нет
                IsTimerSet = false;
            button8_Click(sender, e); // выключаем радио, не важно вкл он или нет
        }

        private void button5_Click(object sender, EventArgs e) // Отобразить будильник
        {
            button4.Visible = true;
            button1.Visible = true;
            button3.Visible = true;
            button2.Visible = true;

            label11.Visible = true;
            label5.Visible = true;

            numericUpDown1.Visible = true;
            numericUpDown2.Visible = true;

            label2.Visible = true;
            label4.Visible = true;
        }

        private void dont_show() // функция скрытия будильника
        {
            button4.Visible = false;
            button1.Visible = false;
            button3.Visible = false;
            button2.Visible = false;

            label11.Visible = false;
            label5.Visible = false;

            numericUpDown1.Visible = false;
            numericUpDown2.Visible = false;

            label2.Visible = false;
            label4.Visible = false;
        }

        private void button6_Click(object sender, EventArgs e) // скрыть будильник
        { dont_show(); }

        ///////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////                РАДИО                 ///////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();
        bool RadioPlaying;
        private void button7_Click(object sender, EventArgs e) // Play
        {
            player.controls.play();
            RadioPlaying = true;

            if (IsTimerSet == true) //выключаем таймер, не важно вкл он или нет
                IsTimerSet = false;
            button2_Click(sender, e); // выключаем будильник, не важно вкл он или нет
        }

        private void button8_Click(object sender, EventArgs e) // Stop
        {
            if (player.URL != null)
            {
                player.controls.stop();
                this.Text = "Alarm";
                this.Icon = new Icon("C:\\Users\\Yokz\\Desktop\\Alarm_clock\\main_alarm_icon.ico");
            }
            RadioPlaying = false;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            stop_sound();
            this.Text = "Radio";
            this.Icon = new Icon("C:\\Users\\Yokz\\Desktop\\Alarm_clock\\radio.ico");

            if (listBox1.SelectedItem.ToString() == "Рок")
            {
                player.URL = "http://193.34.51.71:80/";
            }
            if (listBox1.SelectedItem.ToString() == "Поп")
            {
                player.URL = "http://uk2.internet-radio.com:31076/";
            }
            if (listBox1.SelectedItem.ToString() == "Джаз")
            {
                player.URL = "http://uk4.internet-radio.com:8042/";
            }

            RadioPlaying = true;

            //button8_Click(sender, e); // Остановка проигрывания, ибо иначе оно само начинает играть (хз почему, magic)
        }

        int sound_volume; // уровень громкости радио
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            sound_volume = Convert.ToInt32(trackBar1.Value.ToString());
            player.settings.volume = sound_volume; // установка громкости
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////                ТАЙМЕР                ///////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        int mm_timer;
        int ss_timer;
        int timer_time;
        bool IsTimerSet = false;

        private void button9_Click(object sender, EventArgs e) // Запуск (таймера)
        {
            button2_Click(sender, e); // выключаем будильник, не важно вкл он или нет
            button8_Click(sender, e); // выключаем радио, не важно вкл он или нет

            IsTimerSet = true;

            mm_timer = Convert.ToInt32(numericUpDown3.Text);
            ss_timer = Convert.ToInt32(numericUpDown4.Text);

            this.Text = "Timer";
            this.Icon = new Icon("C:\\Users\\Yokz\\Desktop\\Alarm_clock\\timer.ico");

            if (mm_timer == 0 && ss_timer == 0)
            {
                IsTimerSet = false;
                this.Text = "Alarm";
                this.Icon = new Icon("C:\\Users\\Yokz\\Desktop\\Alarm_clock\\main_alarm_icon.ico");
            }

            timer_time = mm_timer * 60 + ss_timer; // v sekundah

            label15.Text = mm_timer + ":" + ss_timer;
        }

        private void timer_time_change()
        {
            if (IsTimerSet == true)
            {
                timer_time--;

                mm_timer = Convert.ToInt32(timer_time / 60);
                ss_timer = timer_time - mm_timer * 60;

                label15.Text = mm_timer + ":" + ss_timer;

                if (mm_timer == 0)
                    if (ss_timer == 0)
                    {
                        IsTimerSet = false;
                        MessageBox.Show("Time is up!", "Timer message");

                        this.Text = "Alarm";
                        this.Icon = new Icon("C:\\Users\\Yokz\\Desktop\\Alarm_clock\\main_alarm_icon.ico");
                    }

            }
        }
            
    }
}
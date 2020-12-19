using System;
using System.Windows.Forms;

namespace DesuBot.Classes
{
    public class TimeClass
    {
        public int hour;
        public int minute;
        public int year;
        public int month;
        public int day;
        public int space;

        public TimeClass(int hour, int minute, int year, int month, int day, int space)
        {
            this.hour = hour;
            this.year = year;
            this.month = month;
            this.day = day;
            this.minute = minute;
            this.space = space;
        }

        public void Time() //Логика времени
        {
            if (space > 12)
            {
                MessageBox.Show("Промежуток между постами не может быть больше 12 часов.");
            }
            else
            {
                int mbtime = hour + space;
                if (mbtime >= 24)
                {
                    hour = 1;
                    day++;
                }
                else if (space <= 0)
                {
                    MessageBox.Show("Post time spasce: <= 0!");
                }
                else
                {
                    hour += space;
                }
                if (day > DateTime.DaysInMonth(year, month))
                {
                    month++;
                    day = 1;
                }
                if (month > 12)
                {
                    month = 1;
                    year++;
                }
            }
        }
    }
}

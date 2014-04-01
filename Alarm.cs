using System;
using System.Media;

namespace Tomato
{
    public static class Alarm
    {
        public static void ActivityEnd()
        {
            PlayAlarm("chimes");
        }

        public static void PauseNotify()
        {
            PlayAlarm("chimes");
        }

        private static void PlayAlarm(string alarmId)
        {
            string currentDirectory = Environment.CurrentDirectory;

            try
            {
                SoundPlayer simpleSound = new SoundPlayer(currentDirectory + "\\alarm.wav");
                simpleSound.Play();
            }
            catch (Exception) { }
        }
    }
}
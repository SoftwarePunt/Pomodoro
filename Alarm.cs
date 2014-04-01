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
            String currentDirectory = Environment.CurrentDirectory;
            SoundPlayer simpleSound = new SoundPlayer(currentDirectory + "\\Alarm05_converted.wav");
            simpleSound.Play();
        }
    }
}
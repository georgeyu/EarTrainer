using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Toub.Sound.Midi;

namespace EarTrainer
{
    internal class Model
    {
        internal event EventHandler<string> ResultChanged = delegate { };

        private const int MIDDLE_C = 39;
        private const int OCTAVE = 12;
        private const int OFF_BY_ONE = 1;
        private const int NO_TIME = 0;
        private const int DEFAULT_CHANNEL = 1;
        private const int PIANO_OFFSET = 3;
        private const int MAX_VOLUME = 127;
        private const int END_TIME_MILLISECS = 2000;
        private const int INTERVAL_TIME_MILLISECS = 1000;
        private const int DEFAULT_OCTAVE = 4;
        private const string INTERVAL_UP = "up";
        private const string INTERVAL_DOWN = "down";
        private const string INTERVAL_NONE = "no interval";
        private const string REGEX_NOTE = "[A-Z]#?";
        private const string REGEX_DIGIT = "\\d";

        private bool note = true;
        private int note1 = MIDDLE_C;
        private int note2 = MIDDLE_C;
        private Random random = new Random();
        private SoundName[] noteNames;
        private SoundName[] intervalNames;

        public Model()
        {
            List<SoundName> noteNamesList = new List<SoundName>();
            string[] notes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

            for (int i = 0; i < OCTAVE; i++)
            {
                noteNamesList.Add(new SoundName(i, notes[i]));
            }

            noteNames = noteNamesList.ToArray();
            List<SoundName> intervalNamesList = new List<SoundName>();
            string[] intervals = { "P1", "m2", "M2", "m3", "M3", "P4", "A4", "P5", "m6", "M6", "m7", "M7", "P8" };

            for (int i = 0; i <= OCTAVE; i++)
            {
                intervalNamesList.Add(new SoundName(i, intervals[i]));
            }

            intervalNames = intervalNamesList.ToArray();
        }

        private void GetNewNote()
        {
            note = true;
            note1 = random.Next(MIDDLE_C, MIDDLE_C + OCTAVE);
        }

        private void GetNewInterval()
        {
            GetNewNote();
            note = false;
            note2 = random.Next(note1 - OCTAVE, note1 + OCTAVE + OFF_BY_ONE);
        }

        private void Play(string note, string interval = INTERVAL_NONE, string direction = INTERVAL_UP)
        {
            try
            {
                MidiPlayer.OpenMidi();
                MidiPlayer.Play(new NoteOn(NO_TIME, DEFAULT_CHANNEL, note, MAX_VOLUME));

                if (interval != INTERVAL_NONE)
                {
                    Thread.Sleep(INTERVAL_TIME_MILLISECS);
                    MidiPlayer.Play(new NoteOff(NO_TIME, DEFAULT_CHANNEL, note, MAX_VOLUME));
                    int note2 = ParseName(note);

                    switch (direction)
                    {
                        case INTERVAL_UP:
                            note2 += ParseIntervalName(interval);
                            break;
                        case INTERVAL_DOWN:
                            note2 -= ParseIntervalName(interval);
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    MidiPlayer.Play(new NoteOn(NO_TIME, DEFAULT_CHANNEL, ParseNote(note2), MAX_VOLUME));
                }

                Thread.Sleep(END_TIME_MILLISECS);
            }
            finally
            {
                MidiPlayer.CloseMidi();
            }
        }

        private string ParseNote(int note)
        {
            return String.Format(
                "{0}{1}",
                noteNames.Where(x => x.Sound == (note - PIANO_OFFSET + OCTAVE) % OCTAVE).First().Name,
                (note - PIANO_OFFSET) / OCTAVE + OFF_BY_ONE);
        }

        private int ParseName(string name)
        {
            int note = noteNames.Where(x => x.Name == (new Regex(REGEX_NOTE)).Match(name).Value).First().Sound;
            int octave = Int32.Parse((new Regex(REGEX_DIGIT)).Match(name).Value);
            return note + PIANO_OFFSET + (octave - OFF_BY_ONE) * OCTAVE;
        }

        private string GetResult()
        {
            if (note)
            {
                return ParseNote(note1);
            }

            return String.Format(
                "{0} {1}: {2} -> {3}",
                GetInterval(),
                note1 > note2 ? INTERVAL_DOWN : INTERVAL_UP,
                ParseNote(note1),
                ParseNote(note2));
        }

        private string GetInterval()
        {
            return intervalNames.Where(x => x.Sound == Math.Abs(note1 - note2)).First().Name;
        }

        private string GetDirection()
        {
            return note2 > note1 ? INTERVAL_UP : INTERVAL_DOWN;
        }

        private int ParseIntervalName(string interval)
        {
            return intervalNames.Where(x => x.Name == interval).First().Sound;
        }

        internal string[] GetNoteNames()
        {
            return noteNames.Select(x => String.Format("{0}{1}", x.Name, DEFAULT_OCTAVE)).ToArray();
        }

        internal string[] GetIntervalNames()
        {
            return intervalNames.Select(x => x.Name).ToArray();
        }

        internal string[] GetDirectionNames()
        {
            return new string[] { INTERVAL_UP, INTERVAL_DOWN, INTERVAL_NONE };
        }

        internal void OnNote()
        {
            ResultChanged(this, String.Empty);
            GetNewNote();
            Play(ParseNote(note1));
        }

        internal void OnInterval()
        {
            ResultChanged(this, String.Empty);
            GetNewInterval();
            Play(ParseNote(note1), GetInterval(), GetDirection());
        }

        internal void OnRepeat()
        {
            Play(ParseNote(note1), GetInterval(), GetDirection());
        }

        internal void OnShow()
        {
            ResultChanged(this, GetResult());
        }

        internal void OnPlay(string note, string interval, string direction)
        {
            Play(note, interval, direction);
        }
    }

    internal class SoundName
    {
        public int Sound { get; private set; }
        public string Name { get; private set; }

        public SoundName(int sound, string name)
        {
            Sound = sound;
            Name = name;
        }
    }
}

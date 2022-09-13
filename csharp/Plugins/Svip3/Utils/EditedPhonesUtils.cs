﻿using OpenSvip.Model;
using System;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class EditedPhonesUtils
    {
        public static Phones Decode(Xstudio.Proto.Note note)
        {
            if (note.ConsonantLen > 0)
            {
                return new Phones
                {
                    HeadLengthInSecs = (float)(note.ConsonantLen / 480.0 * 60.0 / TempoUtils.SongTempoList[0].BPM)
                };
            }
            else
            {
                return null;
            }
        }

        public static int Encode(OpenSvip.Model.Note note)
        {
            int consonantLength = 0;
            if (note.EditedPhones != null && note.EditedPhones.HeadLengthInSecs > 0)
            {
                float noteHeadLengthInSecs = note.EditedPhones.HeadLengthInSecs;
                double phoneStartInSecs = TempoUtils.Synchronizer.GetActualSecsFromTicks(note.StartPos) - noteHeadLengthInSecs;
                double phoneStartInTicks = TempoUtils.Synchronizer.GetActualTicksFromSecs(phoneStartInSecs);
                consonantLength = (int)Math.Round(TempoUtils.Synchronizer.GetActualTicksFromTicks(note.StartPos) - phoneStartInTicks);
            }
            return consonantLength;
        }
    }
}
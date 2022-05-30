﻿using OpenSvip.Framework;
using OpenSvip.Model;
using FlutyDeer.MidiPlugin;

namespace FlutyDeer.MidiStream
{
    internal class MidiConverter : IProjectConverter
    {

        public Project Load(string path, ConverterOptions options)
        {
            return new MidiDecoder
            {
                IsImportTimeSignatures = options.GetValueAsBoolean("importTimeSignatures", true),
                IsImportLyrics = options.GetValueAsBoolean("importLyrics", false),
                LyricEncoding = options.GetValueAsEnum("lyricEncoding", LyricEncodings.UTF8BOM),
                Channel = options.GetValueAsInteger("channel", 0),
                ErrorMidiFilePolicy = options.GetValueAsEnum("errorMidiFilePolicy", ErrorMidiFilePolicyOption.Abort)
            }.DecodeMidiFile(path);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            new MidiEncoder
            {
                IsExportLyrics = options.GetValueAsBoolean("exportLyrics", true),
                Transpose = options.GetValueAsInteger("transpose", 0),
                IsUseCompatibleLyric = options.GetValueAsBoolean("compatibleLyric", false),
                IsRemoveSymbols = options.GetValueAsBoolean("removeSymbols", true),
                LyricEncoding = options.GetValueAsEnum("lyricEncoding", LyricEncodings.UTF8BOM),
                PPQ = options.GetValueAsInteger("ppq", 480),
                SemivowelPreShift = options.GetValueAsInteger("semivowelPreShift", 0)
            }.EncodeMidiFile(project, path);
        }
    }
}
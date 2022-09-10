﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenSvip.Library;
using OpenSvip.Model;

namespace Json2DiffSinger.Utils
{
    public static class ProjectUtil
    {
        /// <summary>
        /// 重设时间轴，转换为单一曲速的工程
        /// </summary>
        /// <param name="project"></param>
        /// <param name="tempo">默认 125，梯相当于毫秒值</param>
        public static void ResetTimeAxis(this Project project, int tempo = 125)
        {
            var synchronizer = new TimeSynchronizer(project.SongTempoList, isAbsoluteTimeMode: true, defaultTempo: tempo);
            foreach (var track in project.TrackList.OfType<SingingTrack>())
            {
                foreach (var note in track.NoteList)
                {
                    var end = (int) Math.Round(synchronizer.GetActualTicksFromTicks(note.StartPos + note.Length));
                    note.StartPos = (int) Math.Round(synchronizer.GetActualTicksFromTicks(note.StartPos));
                    note.Length = end - note.StartPos;
                }
            }

            project.SongTempoList = new List<SongTempo>
            {
                new SongTempo
                {
                    BPM = tempo, Position = 0
                }
            };
        }
        
        /// <summary>
        /// 按时间间隔将音符序列切割为多个分段。
        /// </summary>
        /// <param name="project"></param>
        /// <param name="maxInterval">切割间隔阈值（毫秒）</param>
        /// <returns></returns>
        public static IEnumerable<Tuple<double, Project>> SplitIntoSegments(this Project project, int maxInterval = 300)
        {
            var track = project.TrackList.OfType<SingingTrack>().FirstOrDefault();
            if (track == null || !track.NoteList.Any())
            {
                return Array.Empty<Tuple<double, Project>>();
            }
            
            project.ResetTimeAxis();
            var result = new List<Tuple<double, Project>>();
            var buffer = new List<Note>
            {
                track.NoteList.First()
            };

            var curSegInterval = track.NoteList.First().StartPos;
            for (var i = 1; i < track.NoteList.Count; ++i)
            {
                var prev = track.NoteList[i - 1];
                var cur = track.NoteList[i];
                var interval = cur.StartPos - prev.StartPos - prev.Length;
                if (interval >= maxInterval)
                {
                    var prepareSpace = Math.Min(600, (int) (curSegInterval * 0.8));
                    var segNoteStartPos = buffer.First().StartPos;
                    buffer.ForEach(note => note.StartPos = note.StartPos - segNoteStartPos + prepareSpace);
                    curSegInterval = interval;
                    var segment = new Project
                    {
                        SongTempoList = project.SongTempoList,
                        TimeSignatureList = new List<TimeSignature>
                        {
                            new TimeSignature
                            {
                                BarIndex = 0, Numerator = 4, Denominator = 4
                            }
                        },
                        TrackList = new List<Track>(1)
                        {
                            new SingingTrack
                            {
                                NoteList = buffer
                            }
                        }
                    };
                    result.Add(new Tuple<double, Project>((segNoteStartPos - prepareSpace) / 1000.0, segment));
                    
                    buffer = new List<Note>();
                }
                buffer.Add(cur);
            }
            
            {
                var prepareSpace = Math.Min(600, (int) (curSegInterval * 0.8));
                var segNoteStartPos = buffer.First().StartPos;
                buffer.ForEach(note => note.StartPos = note.StartPos - segNoteStartPos + prepareSpace);

                var segment = new Project
                {
                    SongTempoList = project.SongTempoList,
                    TimeSignatureList = new List<TimeSignature>
                    {
                        new TimeSignature
                        {
                            BarIndex = 0, Numerator = 4, Denominator = 4
                        }
                    },
                    TrackList = new List<Track>(1)
                    {
                        new SingingTrack
                        {
                            NoteList = buffer
                        }
                    }
                };
                result.Add(new Tuple<double, Project>((segNoteStartPos - prepareSpace) / 1000.0, segment));
            }

            return result;
        }
    }
}
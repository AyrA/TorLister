using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorLister
{
    public class Version
    {
        /// <summary>
        /// Gets or sets Version Numbers
        /// </summary>
        public int[] Versions
        { get; set; }

        /// <summary>
        /// Initializes empty Version
        /// </summary>
        public Version()
        {
            Versions = null;
        }

        /// <summary>
        /// Initializes Version from String
        /// </summary>
        /// <remarks>Version String is a comma separated string with single digits or X-Y ranges</remarks>
        /// <param name="VersionString">Version string</param>
        public Version(string VersionString)
        {
            List<int> VersionList = new List<int>();

            foreach (var VEntry in VersionString.Replace(" ","").Trim().Trim(',').Split(',').Select(m => m.Trim()).Where(m => m.Length > 0))
            {
                if (VEntry.Split('-').Length == 2)
                {
                    var Start = int.Parse(VEntry.Split('-')[0]);
                    var End = int.Parse(VEntry.Split('-')[1]);
                    if (End < Start)
                    {
                        throw new ArgumentException("Invalid Range. End was before Start: " + VEntry);
                    }
                    if (End - Start > 9999)
                    {
                        throw new ArgumentException("Possible Memory exhaustion attempt. Range contains more than 9999 Entries");
                    }
                    for (var i = Start; i <= End; i++)
                    {
                        VersionList.Add(i);
                    }
                }
                else
                {
                    VersionList.Add(int.Parse(VEntry));
                }
            }
            //Return Sorted List
            Versions = VersionList.OrderBy(m => m).Distinct().ToArray();
        }

        /// <summary>
        /// Serializes this Instance into a Version String
        /// </summary>
        /// <returns>Version String</returns>
        public override string ToString()
        {
            //Return Zero Version if none was specified
            if (Versions == null || Versions.Length == 0)
            {
                return "0";
            }

            var LastVersion = Versions.Min();
            var Segments = new List<string>();

            Segments.Add(LastVersion.ToString());

            foreach (var V in Versions.OrderBy(m => m).Distinct().Skip(1))
            {
                //Check if version Range
                if (V == LastVersion + 1)
                {
                    ++LastVersion;
                    if (!Segments.Last().EndsWith("-"))
                    {
                        Segments[Segments.Count - 1] += "-";
                    }
                }
                else
                {
                    //Complete previous Range
                    if (Segments.Last().EndsWith("-"))
                    {
                        Segments[Segments.Count - 1] += LastVersion.ToString();
                    }
                    LastVersion = V;
                    Segments.Add(V.ToString());
                }
            }
            //Complete last Segment if still open
            if (Segments.Last().EndsWith("-"))
            {
                Segments[Segments.Count - 1] += LastVersion.ToString();
            }

            return string.Join(",", Segments);
        }
    }
}

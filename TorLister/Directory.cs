using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TorLister
{
    public class Directory
    {
        public string VoteStatus
        { get; private set; }

        public int ConsensusMethod
        { get; private set; }

        public DateTime ValidAfter
        { get; private set; }

        public DateTime FreshUntil
        { get; private set; }

        public DateTime ValidUntil
        { get; private set; }

        public int[] VotingDelays
        { get; private set; }

        public string[] ClientVersions
        { get; private set; }

        public string[] ServerVersions
        { get; private set; }

        public string[] KnownFlags
        { get; private set; }

        public Dictionary<string, Version> RecommendedClientVersions
        { get; private set; }

        public Dictionary<string, Version> RecommendedRelayVersions
        { get; private set; }

        public Dictionary<string, Version> RequiredClientVersions
        { get; private set; }

        public Dictionary<string, Version> RequiredRelayVersions
        { get; private set; }

        public Dictionary<string, int> Params
        { get; private set; }

        public RandValue SharedRandPreviousValue
        { get; private set; }

        public RandValue SharedRandCurrentValue
        { get; private set; }

        /// <summary>
        /// Parses a Network Consensus
        /// </summary>
        /// <param name="DirectorySource">Network Consensus</param>
        public Directory(string DirectorySource)
        {
            using (var MS = new MemoryStream(Encoding.UTF8.GetBytes(DirectorySource.Replace("\n", "\r\n")), false))
            {
                using (var SR = new StreamReader(MS))
                {
                    //Verify Version
                    if (SR.ReadLine() != "network-status-version 3 microdesc")
                    {
                        throw new ArgumentException("Directory Source doesn't looks like a Version 3 Microdescriptor");
                    }
                }
            }
        }
    }
}

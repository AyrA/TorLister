﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorLister
{
    /// <summary>
    /// Provides a Thread safe and case insensitive file based Cache
    /// </summary>
    public class Cache
    {
        /// <summary>
        /// Name of Cache File
        /// </summary>
        public const string CACHEFILE = "cache.bin";

        /// <summary>
        /// Provides a global Cache Lock
        /// </summary>
        private static object locker = new object();

        /// <summary>
        /// Cache Entries directly
        /// </summary>
        private static CacheEntry[] Entries;

        /// <summary>
        /// Gets all cache Entry Names
        /// </summary>
        public static string[] Names
        {
            get
            {
                return Entries == null ? null : Entries.Select(m => m.Name).ToArray();
            }
        }

        /// <summary>
        /// Loads the Cache
        /// </summary>
        static Cache()
        {
            ReloadCache();
        }

        /// <summary>
        /// Gets an Entry from the Cache
        /// </summary>
        /// <param name="Name">Entry Name</param>
        /// <returns>Cache Entry</returns>
        public static CacheEntry Get(string Name)
        {
            return Get(Name, TimeSpan.MaxValue);
        }

        /// <summary>
        /// Gets an Entry from the Cache that is not older than the specified MaxAge Value
        /// </summary>
        /// <param name="Name">Entry Name</param>
        /// <param name="MaxAge">Maximum permitted Age</param>
        /// <returns>Cache Entry</returns>
        public static CacheEntry Get(string Name, TimeSpan MaxAge)
        {
            if (Name == null)
            {
                Name = "";
            }
            return Entries.FirstOrDefault(m => m.Name.ToLower() == Name.ToLower() && DateTime.UtcNow.Subtract(m.Created) < MaxAge);
        }

        /// <summary>
        /// Removes an Entry from the Cache
        /// </summary>
        /// <param name="Name">Entry Name</param>
        /// <param name="Write">Write Changes to Disk immediately</param>
        public static void Remove(string Name, bool Write = false)
        {
            lock (locker)
            {
                if (Name == null)
                {
                    Name = "";
                }
                Entries = Entries.Where(m => m.Name.ToLower() != Name.ToLower()).ToArray();
                if (Write)
                {
                    SaveCache();
                }
            }
        }

        /// <summary>
        /// Adds an Entry to the Cache
        /// </summary>
        /// <param name="Name">Name</param>
        /// <param name="Data">Data</param>
        /// <param name="Write">Write Changes to Disk immediately</param>
        /// <remarks>If Data is null or has 0 Length the Entry is removed</remarks>
        public static void Add(string Name, byte[] Data, bool Write = false)
        {
            lock (locker)
            {
                if (Data == null || Data.Length == 0)
                {
                    Remove(Name, Write);
                }
                else
                {
                    if (Name == null)
                    {
                        Name = "";
                    }

                    if (Entries.Count(m => m.Name.ToLower() == Name.ToLower()) > 0)
                    {
                        var E = Entries.First(m => m.Name.ToLower() == Name.ToLower());
                        var I = Array.IndexOf(Entries, E);
                        E.Created = DateTime.UtcNow;
                        E.Data = Data;
                        Entries[I] = E;
                    }
                    else
                    {
                        Entries = Entries.Concat(new CacheEntry[] { new CacheEntry()
                        {
                            Name = Name,
                            Created = DateTime.UtcNow,
                            Data = Data
                        }}).ToArray();
                    }

                    if (Write)
                    {
                        SaveCache();
                    }
                }
            }
        }

        /// <summary>
        /// Loads Cache from Disk, discarding any Memory Entries
        /// </summary>
        public static void ReloadCache()
        {
            lock (locker)
            {
                Entries = null;
                if (File.Exists(CACHEFILE))
                {
                    try
                    {
                        using (var FS = File.OpenRead(CACHEFILE))
                        {
                            using (var Decomp = new GZipStream(FS, CompressionMode.Decompress))
                            {
                                using (var BR = new BinaryReader(Decomp))
                                {
                                    Entries = new CacheEntry[BR.ReadInt32()];
                                    for (var i = 0; i < Entries.Length; i++)
                                    {
                                        Entries[i] = new CacheEntry()
                                        {
                                            Name = Encoding.UTF8.GetString(BR.ReadBytes(BR.ReadInt32())),
                                            Created = new DateTime(BR.ReadInt64(), DateTimeKind.Utc),
                                            Data = BR.ReadBytes(BR.ReadInt32())
                                        };
                                    }
                                }
                            }
                        }

                    }
                    catch
                    {
                        Entries = null;
                    }
                }
            }
        }

        /// <summary>
        /// Saves Cache to Disk
        /// </summary>
        public static void SaveCache()
        {
            lock (locker)
            {
                if (Entries == null || Entries.Length == 0)
                {
                    if (File.Exists(CACHEFILE))
                    {
                        File.Delete(CACHEFILE);
                    }
                }
                else
                {
                    using (var FS = File.Create(CACHEFILE))
                    {
                        using (var Comp = new GZipStream(FS, CompressionLevel.Optimal))
                        {
                            using (var BW = new BinaryWriter(Comp))
                            {
                                BW.Write(Entries.Length);
                                foreach (var E in Entries)
                                {
                                    if (string.IsNullOrEmpty(E.Name))
                                    {
                                        BW.Write(0);
                                    }
                                    else
                                    {
                                        BW.Write(Encoding.UTF8.GetByteCount(E.Name));
                                        BW.Write(Encoding.UTF8.GetBytes(E.Name));
                                    }
                                    BW.Write(E.Created.ToUniversalTime().Ticks);
                                    BW.Write(E.Data.Length);
                                    BW.Write(E.Data);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents a Cache Entry
    /// </summary>
    public class CacheEntry
    {
        public string Name;
        public DateTime Created;
        public byte[] Data;
    }
}
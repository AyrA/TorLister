﻿using System.IO.Compression;
using System.Text;

namespace TorLister.Tools
{
    /// <summary>
    /// Provides a Thread safe and case insensitive file based Cache
    /// </summary>
    public class Cache
    {
        /// <summary>
        /// Name of Cache File
        /// </summary>
        private const string CACHEFILE = "cache.bin";

        private static readonly string _cachePath = Path.Combine(Utils.AppPath, CACHEFILE);

        public static string CachePath => _cachePath;

        /// <summary>
        /// Provides a global Cache Lock
        /// </summary>
        private static readonly object locker = new();

        /// <summary>
        /// Cache Entries directly
        /// </summary>
        private static CacheEntry[]? Entries;

        /// <summary>
        /// Gets all cache Entry Names
        /// </summary>
        public static string[]? Names
        {
            get
            {
                return Entries?.Select(m => m.Name ?? "").ToArray();
            }
        }

        /// <summary>
        /// Gets if the Data in Memory has been modified and needs to be written do Disk
        /// </summary>
        public static bool Dirty
        { get; private set; }

        /// <summary>
        /// Loads the Cache
        /// </summary>
        static Cache()
        {
            Dirty = false;
            ReloadCache();
        }

        /// <summary>
        /// Gets an Entry from the Cache
        /// </summary>
        /// <param name="Name">Entry Name</param>
        /// <returns>Cache Entry</returns>
        public static CacheEntry? Get(string Name)
        {
            return Get(Name, TimeSpan.MaxValue);
        }

        /// <summary>
        /// Gets an Entry from the Cache that is not older than the specified MaxAge Value
        /// </summary>
        /// <param name="Name">Entry Name</param>
        /// <param name="MaxAge">Maximum permitted Age</param>
        /// <returns>Cache Entry</returns>
        public static CacheEntry? Get(string Name, TimeSpan MaxAge)
        {
            Name ??= "";
            return Entries?.FirstOrDefault(m => m.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase) && DateTime.UtcNow.Subtract(m.Created) < MaxAge);
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
                if (Entries != null && Entries.Length > 0)
                {
                    Name ??= "";
                    if (Entries.Any(m => m.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        Dirty = true;
                        Entries = Entries.Where(m => !m.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                        if (Write)
                        {
                            SaveCache();
                        }
                    }
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
                    Name ??= "";
                    Entries ??= [];
                    if (Entries.Any(m => m.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var E = Entries.First(m => m.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase));
                        var I = Array.IndexOf(Entries, E);
                        E.Created = DateTime.UtcNow;
                        E.Data = Data;
                        Entries[I] = E;
                    }
                    else
                    {
                        Entries =
                        [
                            .. Entries,
                            new CacheEntry(Name, DateTime.UtcNow, Data)
                        ];
                    }
                    Dirty = true;
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
                Dirty = false;
                Entries = null;
                if (File.Exists(CachePath))
                {
                    try
                    {
                        using var FS = File.OpenRead(CachePath);
                        using var Decomp = new GZipStream(FS, CompressionMode.Decompress);
                        using var BR = new BinaryReader(Decomp);
                        Entries = new CacheEntry[BR.ReadInt32()];
                        for (var i = 0; i < Entries.Length; i++)
                        {
                            Entries[i] = new CacheEntry(
                                Encoding.UTF8.GetString(BR.ReadBytes(BR.ReadInt32())),
                                new DateTime(BR.ReadInt64(), DateTimeKind.Utc),
                                BR.ReadBytes(BR.ReadInt32()));
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
                    if (File.Exists(CachePath))
                    {
                        File.Delete(CachePath);
                    }
                }
                else
                {
                    using var FS = File.Create(CachePath);
                    using var Comp = new GZipStream(FS, CompressionLevel.Optimal);
                    using var BW = new BinaryWriter(Comp);
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
                Dirty = false;
            }
        }
    }

    /// <summary>
    /// Represents a Cache Entry
    /// </summary>
    public class CacheEntry(string name, DateTime created, byte[] data)
    {
        public string Name = name;
        public DateTime Created = created;
        public byte[] Data = data;
    }
}

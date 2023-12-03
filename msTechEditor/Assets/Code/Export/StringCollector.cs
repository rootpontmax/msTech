using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace msTech.Export
{
    public interface IStringCollector
    {
        void AddStrings(string[] strings);
        void CreateRegister();
        void Export(string folder);

        int GetStringId(string str);
    }

    public class StringCollector : IStringCollector
    {
        public StringCollector()
        {
            _stringSet = new HashSet<string>();
            _stringList = new List<string>(CAPACITY);
            _dictStrToId = new Dictionary<string, int>(CAPACITY);
        }

        public void AddStrings(string[] strings)
        {
            if ( null != strings && strings.Length > 0 )
                for ( int i = 0; i < strings.Length; ++i )
                    if ( !string.IsNullOrEmpty(strings[i]) )
                        _stringSet.Add(strings[i]);
        }

        public void CreateRegister()
        {
            // Collect all unique strings in list
            _stringList.Clear();
            foreach ( string s in _stringSet )
                _stringList.Add(s);

            // And make string-id dictionary
            for ( int i = 0; i < _stringList.Count; ++i )
                _dictStrToId.Add(_stringList[i], i);
        }

        public void Export(string folder)
        {
            string binFolderPath = folder + "/Common";
            ExportTools.CreateDirectoriesForPath(folder, "/Common");
            
            /*
            // Check exsistance of Bin folder
            string binFolderPath = folder + "/Common";
            if ( !Directory.Exists(binFolderPath) )
                Directory.CreateDirectory(binFolderPath);
            */

            // Save strings
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(_stringList.Count);
            for ( int i = 0; i < _stringList.Count; ++i )
            {
                byte[] stringDataBytes = System.Text.Encoding.UTF8.GetBytes(_stringList[i]);
                bw.Write(stringDataBytes.Length);
                bw.Write(stringDataBytes);
            }

            bw.Flush();
            string filename = binFolderPath + "/strings.msd";
            File.WriteAllBytes(filename, ms.ToArray() );
        }

        public int GetStringId(string str)
        {
            if ( _dictStrToId.TryGetValue(str, out int id ) )
                return id;

            Debug.LogError("Can't find ID for string " + str);
            return -1;
        }


        private static readonly int CAPACITY = 10000;

        private readonly HashSet<string> _stringSet;
        private readonly List<string> _stringList;
        private readonly Dictionary<string, int> _dictStrToId;
    }
}

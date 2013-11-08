using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PSWinCom.Gateway.Client
{
    public class MmsPart
    {
        private string _name;
        private byte[] _data;

        public string Name { get { return _name; } set { _name = value; } }
        public byte[] Data { get { return _data; } set { _data = value; } }

        public MmsPart(byte[] data, string name)
        {
            _name = name;
            _data = data;
        }

        /// <summary>
        /// Creates a MmsPart from a stream, you will need to specify a name for the part
        /// </summary>
        /// <param name="stream">The stream containing the data</param>
        /// <param name="name">Name of the part (f.ex. message.txt)</param>
        public static MmsPart FromStream(Stream stream, string name)
        {
            try
            {
                var data = new byte[stream.Length];
                stream.Read(data, 0, (int)stream.Length);
                return new MmsPart(data, name);
            }
            catch (Exception e)
            {
                throw new MmsException("Failed to load content from stream", e);
            }
        }

        /// <summary>
        /// Creates a MmsPart from a file and specifies another part name
        /// </summary>
        /// <param name="filename">A valid path to a existing file</param>
        /// <param name="partname">The name of the part in the mms file</param>
        public static MmsPart FromFile(string filename, string partname)
        {
            try
            {
                using (var stream = File.OpenRead(filename))
                {
                    return MmsPart.FromStream(stream, partname);
                }
            }
            catch (Exception e)
            {
                throw new MmsException("Failed to load content from file", e);
            }
        }

        /// <summary>
        /// Creates a MmsPart from a file
        /// </summary>
        /// <param name="filename">A valid path to a existing file</param>
        public static MmsPart FromFile(string filename)
        {
            var name = new FileInfo(filename).Name;
            return MmsPart.FromFile(filename, name);
        }

        /// <summary>
        /// Save the content of this part to a file in the given folder using the original filename
        /// </summary>
        /// <param name="path">A valid path to a directory</param>
        public void SaveTo(string path)
        {
            var name = Regex.Replace(_name, "[<>]", "_");
            SaveAs(Path.Combine(path, name));
        }

        /// <summary>
        /// Save the content of this part to the given filename
        /// </summary>
        /// <param name="filename">Full filename to save as</param>
        public void SaveAs(string filename)
        {
            try
            {
                if (_data.Length > 0 && filename != null & filename.Length > 0)
                    using (FileStream stream = File.Create(filename))
                        stream.Write(_data, 0, _data.Length);
            }
            catch (Exception e)
            {
                throw new MmsException("Failed to save part content to file", e);
            }
        }
    }
}

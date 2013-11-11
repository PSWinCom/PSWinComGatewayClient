using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PSWinCom.Gateway.Client
{
    public class MmsFile
    {
        public List<MmsPart> Parts { get; set; }

        public MmsFile()
        {
            Parts = new List<MmsPart>();
        }

        public MmsFile(params MmsPart[] parts)
            : this(parts.AsEnumerable())
        {

        }

        public MmsFile(IEnumerable<MmsPart> parts)
        {
            Parts = parts.ToList();
        }

        internal byte[] GetBytes()
        {
            using (MemoryStream msZipOutput = new MemoryStream())
            {

                using (var os = new ZipOutputStream(msZipOutput) { IsStreamOwner = false, UseZip64 = UseZip64.Off })
                {
                    foreach (var part in Parts)
                    {
                        os.PutNextEntry(new ZipEntry(part.Name));
                        os.Write(part.Data, 0, part.Data.Length);
                        os.CloseEntry();
                    }
                    os.Finish();
                    os.Close();

                    return msZipOutput.ToArray();
                }
            }
        }

        public void AddPart(string filename)
        {
            Parts.Add(MmsPart.FromFile(filename));
        }

        public void AddPart(string name, Stream stream)
        {
            Parts.Add(MmsPart.FromStream(stream, name));
        }

        public void AddTextPart(string message, string name)
        {
            Parts.Add(MmsPart.FromText(message, name));
        }
        /// <summary>
        /// Save all MMS message parts as one compressed file. Suitable for packing content
        /// for storing and later sending.
        /// </summary>
        /// <param name="filename">Filename to save as.</param>
        public void SaveCompressed(string filename)
        {
            if (!String.IsNullOrEmpty(filename))
                using (FileStream stream = File.Create(filename))
                    SaveCompressed(stream);
        }

        /// <summary>
        /// Save all MMS message parts as a zip to a stream. Suitable for packing content
        /// for storing in database or such,
        /// </summary>
        /// <param name="stream">Stream to save to.</param>
        public void SaveCompressed(Stream stream)
        {
            if (Parts.Count == 0)
                throw new MmsException("No content to save.");

            byte[] data = GetBytes();

            if (data.Length > 0)
                stream.Write(data, 0, data.Length);
        }

        public static MmsFile FromZip(string filename)
        {
            try
            {
                using (FileStream fileStream = File.Open(filename, FileMode.Open, FileAccess.Read))
                    return MmsFile.FromZip(fileStream);
            }
            catch (Exception e)
            {
                throw new MmsException("Failed to load content from file", e);
            }
        }

        public static MmsFile FromZip(Stream stream)
        {
            var file = new MmsFile();

            ZipInputStream zip = new ZipInputStream(stream); // Do NOT dispose, as it will close stream aswell, wich might not be what user wants
            ZipEntry entry;
            while ((entry = zip.GetNextEntry()) != null)
            {
                string fileName = entry.Name;
                if (fileName != String.Empty)
                {
                    int size = 2048;
                    int len = 0;
                    using (var mspart = new MemoryStream(size))
                    {
                        byte[] partData = new byte[size];
                        while (true)
                        {
                            size = zip.Read(partData, 0, partData.Length);
                            len += size;
                            if (size > 0)
                                mspart.Write(partData, 0, size);
                            else
                                break;
                        }
                        file.Parts.Add(new MmsPart(mspart.ToArray(), fileName));
                    }
                }
            }
            zip.Close();

            return file;
        }

        public static implicit operator byte[](MmsFile file)
        {
            return file.GetBytes();
        }
    }
}
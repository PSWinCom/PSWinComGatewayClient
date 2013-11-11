using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PSWinCom.Gateway.MMS.Client
{
	/// <summary>
	/// An MMSMessagePart object holds the name and content data for each part of the
	/// MMS message. It will typically be a picture, sound, text or similar.
	/// </summary>
	public class MMSMessagePart
	{
		private int _PartId;
		private string _Name;
		private byte[] _Data;

		#region Accessors

		/// <summary>
		/// Name of this content part. Typically this is the filename of the content file.
		/// </summary>
		public string Name { get { return _Name; } set { _Name = value; } }
		/// <summary>
		/// Numeric id number of this particular part.
		/// </summary>
		public int PartId { get { return _PartId; } set { _PartId = value; } }
		/// <summary>
		/// A byte array holding the data of this content part.
		/// </summary>
		public byte[] Data { get { return _Data; } set { _Data = value; } }
		
		#endregion


		/// <summary>
		/// Default constructor
		/// </summary>
		public MMSMessagePart()
		{


		}

		/// <summary>
		/// Construct new MMSMessagePart from file
		/// </summary>
		/// <param name="filename"></param>
		public MMSMessagePart(string filename)
		{
			Load(filename);
		}

		/// <summary>
		/// Load content from file into this MMS part
		/// </summary>
		/// <param name="filename">Complete path and filename of content file to load</param>
		public void Load(string filename)
		{
			try 
			{
				using(FileStream streamReader = File.OpenRead(filename))
				{
					_Data = new byte[streamReader.Length];
					streamReader.Read(_Data, 0, (int)streamReader.Length);
					int i = filename.LastIndexOf("\\");
					if(i>0)
						_Name = filename.Substring(i+1);
				}
			} 
			catch(Exception e) 
			{
				throw new MMSException("Failed to load content from file", e);
			}

		}
		
		/// <summary>
		/// Get the content as a string using default (ISO 8859 1) encoding. 
		/// Should only be called on parts that are considered
		/// to be text-parts (Name ending with .txt or similar)
		/// </summary>
		/// <returns>String content of part</returns>
		public string GetAsString()
		{
			return GetAsString(System.Text.Encoding.GetEncoding("ISO-8859-1"));
		}

		/// <summary>
		/// Get the content as a string using a specific encoding. 
		/// Should only be called on parts that are considered
		/// to be text-parts (Name ending with .txt or similar)
		/// </summary>
		/// <param name="enc">Encoding to use</param>
		/// <returns>String content of part</returns>
		public string GetAsString(System.Text.Encoding enc)
		{
			if(_Data.Length > 0)
				return enc.GetString(_Data, 0, _Data.Length);
			else
				return string.Empty;
		}

		/// <summary>
		/// Save the content of this part to a file in the given folder using the original filename
		/// </summary>
		/// <param name="path">A valid path to a directory</param>
		public void Save(string path)
		{
			SaveAs(path, _Name);
		}

		/// <summary>
		/// Save the content of this part to a file in the given folder using the specified filename
		/// </summary>
		/// <param name="path">A valid path to a directory</param>
		/// <param name="filename">Filename to save as</param>
		public void SaveAs(string path, string filename)
		{
			try
			{
				if(!path.EndsWith("\\"))
					path += "\\";

                filename = Regex.Replace(filename, "[<>]", "_");

				if(_Data.Length > 0 && filename != null & filename.Length > 0) 
					using(FileStream streamWriter = File.Create(path + filename))
						streamWriter.Write(_Data, 0, _Data.Length);
			} 
			catch(Exception e) 
			{
				throw new MMSException("Failed to save part content to file", e);
			}
		}
	}
}

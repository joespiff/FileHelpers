#region "  � Copyright 2005-07 to Marcos Meli - http://www.devoo.net"

// Errors, suggestions, contributions, send a mail to: marcos@filehelpers.com.

#endregion

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Diagnostics;

namespace FileHelpers
{

    public sealed class FileTransformEngine
        :FileTransformEngine<object, object>
    {
        public FileTransformEngine(Type sourceType, Type destType):
            base(sourceType, destType)
        {
            mIsNonGenericEngine = true;
        }

    }


	/// <summary>
	/// This class allow you to convert the records of a file to a different record format.
	/// </summary>
	/// <seealso href="quick_start.html">Quick Start Guide</seealso>
	/// <seealso href="class_diagram.html">Class Diagram</seealso>
	/// <seealso href="examples.html">Examples of Use</seealso>
	/// <seealso href="example_datalink.html">Example of the DataLink</seealso>
	/// <seealso href="attributes.html">Attributes List</seealso>
    [DebuggerDisplay("FileTransformanEngine for types: {SourceType.Name} --> {DestinationType.Name}. Source Encoding: {SourceEncoding.EncodingName}. Destination Encoding: {DestinationEncoding.EncodingName}")]
    /// <typeparam name="Source">The source record type.</typeparam>
    /// <typeparam name="Destination">The destination record type.</typeparam>
    public class FileTransformEngine<Source, Destination>
        where Source: class 
        where Destination: class
	{

        #region "  Constructor  "

        /// <summary>Create a new instance of the class.</summary>
		/// <param name="sourceType">The source record Type.</param>
		/// <param name="destType">The destination record Type.</param>

		public FileTransformEngine()
            :this(typeof(Source), typeof(Destination))
		{
		}

        internal FileTransformEngine(Type sourceType, Type destType)
        { 
            			//throw new NotImplementedException("This feature is not ready yet. In the next release maybe work =)");
			ExHelper.CheckNullParam(sourceType, "sourceType");
			ExHelper.CheckNullParam(destType, "destType");
			ExHelper.CheckDifferentsParams(sourceType, "sourceType", destType, "destType");

			mSourceType = sourceType;
			mDestinationType = destType;

			ValidateRecordTypes();

        }


        [Obsolete("You must use the Property ErrorMode instead of passing it to the constructor, use the parameterless constructor", true)]
		public FileTransformEngine(FileHelpers.ErrorMode errorMode)
            :this()
		{
            throw new InvalidOperationException("Obsolete Method, setted to be removed in version 3.0");
        }

		#endregion

		#region "  Private Fields  "

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static object[] mEmptyArray = new object[] { };

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Type mSourceType;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Type mDestinationType;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Encoding mSourceEncoding = Encoding.Default;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Encoding mDestinationEncoding = Encoding.Default;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal bool mIsNonGenericEngine = false;

        private ErrorMode mErrorMode;

        /// <summary>Indicates the behavior of the engine when it found an error.</summary>
        public ErrorMode ErrorMode
        {
            get { return mErrorMode; }
            set
            {
                mErrorMode = value;
                mSourceErrorManager = new ErrorManager(value);
                mDestinationErrorManager = new ErrorManager(value);
            }
        }

        private ErrorManager mSourceErrorManager = new ErrorManager();

        /// <summary>
        /// Allow access the <see cref="ErrorManager"/> of the engine used to read the source file, is null before transform any file
        /// </summary>
        public ErrorManager SourceErrorManager
        {
            get { return mSourceErrorManager; }
        }

        private ErrorManager mDestinationErrorManager = new ErrorManager();

        /// <summary>
        /// Allow access the <see cref="ErrorManager"/> of the engine used to write the destination file, is null before transform any file
        /// </summary>
        public ErrorManager DestinationErrorManager
        {
            get { return mDestinationErrorManager; }
        }


		#endregion

		#region "  TransformFile  " 

		/// <summary>Transform the contents of the sourceFile and write them to the destFile.(use only if you need the array of the transformed records, TransformFileAsync is faster)</summary>
		/// <param name="sourceFile">The source file.</param>
		/// <param name="destFile">The destination file.</param>
		/// <returns>The transformed records.</returns>
		public Destination[] TransformFile(string sourceFile, string destFile)
		{
			ExHelper.CheckNullParam(sourceFile, "sourceFile");
			ExHelper.CheckNullParam(destFile, "destFile");
			ExHelper.CheckDifferentsParams(sourceFile, "sourceFile", destFile, "destFile");

			if (mConvert1to2 == null)
				throw new BadUsageException("You must define a method in the class " + SourceType.Name + " with the attribute [TransfortToRecord(typeof(" + DestinationType.Name + "))] that return an object of type " + DestinationType.Name);

			return CoreTransformFile(sourceFile, destFile, mSourceType, mDestinationType, mConvert1to2);
		}


		/// <summary>Transform the contents of the sourceFile and write them to the destFile. (faster and use less memory, best choice for big files)</summary>
		/// <param name="sourceFile">The source file.</param>
		/// <param name="destFile">The destination file.</param>
		/// <returns>The number of transformed records.</returns>
		public int TransformFileAsync(string sourceFile, string destFile)
		{
			ExHelper.CheckNullParam(sourceFile, "sourceFile");
			ExHelper.CheckNullParam(destFile, "destFile");
			ExHelper.CheckDifferentsParams(sourceFile, "sourceFile", destFile, "destFile");

			if (mConvert1to2 == null)
				throw new BadUsageException("You must define a method in the class " + SourceType.Name + " with the attribute [TransfortToRecord(typeof(" + DestinationType.Name + "))] that return an object of type " + DestinationType.Name);

			return CoreTransformAsync(sourceFile, destFile, mSourceType, mDestinationType, mConvert1to2);
		}


		#endregion

//		public string TransformString(string sourceData)
//		{
//			if (mConvert1to2 == null)
//				throw new BadUsageException("You must define a method in the class " + SourceType.Name + " with the attribute [TransfortToRecord(typeof(" + DestinationType.Name + "))] that return an object of type " + DestinationType.Name);
//
//			return CoreTransformAsync(sourceFile, destFile, mSourceType, mDestinationType, mConvert1to2);
//		}


		/// <summary>Transform an array of records from the source type to the destination type</summary>
		/// <param name="sourceRecords">An array of the source records.</param>
		/// <returns>The transformed records.</returns>
		public Destination[] TransformRecords(Source[] sourceRecords)
		{
			if (mConvert1to2 == null)
				throw new BadUsageException("You must define a method in the class " + SourceType.Name + " with the attribute [TransfortToRecord(typeof(" + DestinationType.Name + "))] that return an object of type " + DestinationType.Name);

			return CoreTransformRecords(sourceRecords, mConvert1to2);
			//return CoreTransformAsync(sourceFile, destFile, mSourceType, mDestinationType, mConvert1to2);
		}

		/// <summary>Transform a file that contains source records to an array of the destination type</summary>
		/// <param name="sourceFile">A file containing the source records.</param>
		/// <returns>The transformed records.</returns>

		public Destination[] ReadAndTransformRecords(string sourceFile)
		{
			if (mConvert1to2 == null)
				throw new BadUsageException("You must define a method in the class " + SourceType.Name + " with the attribute [TransfortToRecord(typeof(" + DestinationType.Name + "))] that return an object of type " + DestinationType.Name);

			FileHelperAsyncEngine engine = new FileHelperAsyncEngine(mSourceType, mSourceEncoding);
            engine.ErrorMode = this.ErrorMode;
            mSourceErrorManager = engine.ErrorManager;
            mDestinationErrorManager = new ErrorManager(ErrorMode);

			ArrayList res = new ArrayList();

			engine.BeginReadFile(sourceFile);
			foreach (Source record in engine)
			{
				res.Add(CoreTransformOneRecord(record, mConvert1to2));
			}
			engine.Close();

			return (Destination[]) res.ToArray(mDestinationType);
		}

		#region "  Transform Internal Methods  "

        			
        private Destination[] CoreTransform(StreamReader sourceFile, StreamWriter destFile, Type sourceType, Type destType, MethodInfo method)
        {
            if (mIsNonGenericEngine)
            {
                FileHelperEngine sourceEngine = new FileHelperEngine(mSourceType, mSourceEncoding);
                FileHelperEngine destEngine = new FileHelperEngine(mDestinationType, mDestinationEncoding);

                sourceEngine.ErrorMode = this.ErrorMode;
                destEngine.ErrorManager.ErrorMode = this.ErrorMode;

                mSourceErrorManager = sourceEngine.ErrorManager;
                mDestinationErrorManager = destEngine.ErrorManager;

                object[] source = sourceEngine.ReadStream(sourceFile);
                Destination[] transformed = CoreTransformRecords((Source[]) source, method);

                destEngine.WriteStream(destFile, (object[]) transformed);

                return transformed;
            }
            else
            {

                FileHelperEngine<Source> sourceEngine = new FileHelperEngine<Source>(mSourceEncoding);
                FileHelperEngine<Destination> destEngine = new FileHelperEngine<Destination>(mDestinationEncoding);

                sourceEngine.ErrorMode = this.ErrorMode;
                destEngine.ErrorManager.ErrorMode = this.ErrorMode;

                mSourceErrorManager = sourceEngine.ErrorManager;
                mDestinationErrorManager = destEngine.ErrorManager;

                Source[] source = sourceEngine.ReadStream(sourceFile);
                Destination[] transformed = CoreTransformRecords(source, method);

                destEngine.WriteStream(destFile, transformed);

                return transformed;
            }
		}

		private Destination[] CoreTransformRecords(Source[] sourceRecords, MethodInfo method)
		{
			ArrayList res = new ArrayList(sourceRecords.Length);
			
			for (int i = 0; i < sourceRecords.Length; i++)
			{
				res.Add(CoreTransformOneRecord(sourceRecords[i], method));
			}
			return (Destination[]) res.ToArray(mDestinationType);
		}

		
		private Destination[] CoreTransformFile(string sourceFile, string destFile, Type sourceType, Type destType, MethodInfo method)
		{
			Destination[] tempRes;

			using (StreamReader fs = new StreamReader(sourceFile, mSourceEncoding, true))
			{
				using (StreamWriter ds = new StreamWriter(destFile, false, mDestinationEncoding))
				{
					tempRes = CoreTransform(fs, ds, sourceType, destType, method);
					ds.Close();
				}
				
				fs.Close();
			}


			return tempRes;
	}

		private int CoreTransformAsync(string sourceFile, string destFile, Type sourceType, Type destType, MethodInfo method)
		{
			FileHelperAsyncEngine sourceEngine = new FileHelperAsyncEngine(sourceType);
			FileHelperAsyncEngine destEngine = new FileHelperAsyncEngine(destType);

            sourceEngine.ErrorMode = this.ErrorMode;
            destEngine.ErrorMode = this.ErrorMode;

            mSourceErrorManager = sourceEngine.ErrorManager;
            mDestinationErrorManager = destEngine.ErrorManager;

			sourceEngine.Encoding = mSourceEncoding;
			destEngine.Encoding = mDestinationEncoding;

			sourceEngine.BeginReadFile(sourceFile);
			destEngine.BeginWriteFile(destFile);

			foreach (Source record in sourceEngine)
			{
				destEngine.WriteNext(CoreTransformOneRecord(record, method));
			}
			
			sourceEngine.Close();
			destEngine.Close();

			return sourceEngine.TotalRecords;
		}

		private static Destination CoreTransformOneRecord(Source record, MethodInfo method)
		{
			return (Destination) method.Invoke(record, mEmptyArray);
		}

		#endregion

		#region "  Properties  "

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        MethodInfo mConvert1to2 = null;
		//MethodInfo mConvert2to1 = null;

		/// <summary>The source record Type.</summary>
		public Type SourceType
		{
			get { return mSourceType; }
		}

		/// <summary>The destination record Type.</summary>
		public Type DestinationType
		{
			get { return mDestinationType; }
		}

		/// <summary>The Encoding of the Source File.</summary>
		public Encoding SourceEncoding
		{
			get { return mSourceEncoding; }
			set { mSourceEncoding = value; }
		}

		/// <summary>The Encoding of the Destination File.</summary>
		public Encoding DestinationEncoding
		{
			get { return mDestinationEncoding; }
			set { mDestinationEncoding = value; }
		}


		#endregion

		#region "  Helper Methods  "

		private void ValidateRecordTypes()
		{
			mConvert1to2 = GetTransformMethod(SourceType, DestinationType);
			//			mConvert2to1 = GetTransformMethod(DestinationType, SourceType);

			//			if (mConvert2to1 == null)
			//				throw new BadUsageException("You must define a method in the class " + RecordType2.Name + " with the attribute [TransfortToRecord(typeof(" + RecordType2.Name + "))]");
		}

		private MethodInfo GetTransformMethod(Type sourceType, Type destType)
		{
			MethodInfo res = null;
			
			MethodInfo[] methods = sourceType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
			foreach (MethodInfo m in methods)
			{
				if (m.IsDefined(typeof (TransformToRecordAttribute), false))
				{
					TransformToRecordAttribute ta = (TransformToRecordAttribute) m.GetCustomAttributes(typeof (TransformToRecordAttribute), false)[0];
					if (ta.TargetType == destType)
					{
						if (m.ReturnType != destType)
							throw new BadUsageException("The method " + m.Name + " must return an object of type " + destType.Name + " (not " + m.ReturnType.Name + ")");
						else if (m.GetParameters().Length > 0)
							throw new BadUsageException("The method " + m.Name + " must have not parameters");
						else
							res = m;

						break;
					}
				}
			}

			return res;
		}

		#endregion

	}
}
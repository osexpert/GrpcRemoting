using System;
using System.Runtime.Serialization;

namespace GrpcRemoting.RemoteDelegates
{
    /// <summary>
    /// Describes a remote delegate.
    /// </summary>
    [DataContract]
    [Serializable]
    public class RemoteDelegateInfo
    {
        [DataMember]
        private string _delegateTypeName;

        [DataMember]
        private bool _hasResult;


        /// <summary>
        /// Creates a new instance of the RemoteDelegateInfo class.
        /// </summary>
        /// <param name="delegateTypeName">Type name of the client delegate</param>
        /// <param name="hasResult">Has result</param>
		public RemoteDelegateInfo(string delegateTypeName, bool hasResult)
        {
            _delegateTypeName = delegateTypeName;
			_hasResult = hasResult;
        }
        
        /// <summary>
        /// Gets the type name of the client delegate.
        /// </summary>
        public string DelegateTypeName => _delegateTypeName;

        /// <summary>
        /// HasResult
        /// </summary>
        public bool HasResult => _hasResult;
    }
}
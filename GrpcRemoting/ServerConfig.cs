using System;
using System.Diagnostics.CodeAnalysis;

namespace GrpcRemoting
{
    /// <summary>
    /// Describes the configuration settings of a CoreRemoting service instance.
    /// </summary>
    public class ServerConfig
    {
        /// <summary>
        /// Set this to overide the default Activator.CreateInstance
        /// </summary>
        public Func<Type, object> CreateInstance;

		public bool EnableGrpcDotnetServerBidirStreamNotClosedHacks;
    }
}

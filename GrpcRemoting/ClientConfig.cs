using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace GrpcRemoting
{
    /// <summary>
    /// Provides configuration settings for a CoreRemoting client instance.
    /// </summary>
    public class ClientConfig
    {
        /// <summary>
        /// Set to be notified before a method call
        /// </summary>
        public Action<Type, MethodInfo> BeforeMethodCall;

		public bool EnableGrpcDotnetServerBidirStreamNotClosedHacks;
    }
}

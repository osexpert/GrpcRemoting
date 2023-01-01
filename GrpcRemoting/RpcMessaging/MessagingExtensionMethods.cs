using System;
using System.Collections.Generic;
using GrpcRemoting.Serialization.Bson;

namespace GrpcRemoting.RpcMessaging
{
    /// <summary>
    /// Extension methods for messaging.
    /// </summary>
    public static class MessagingExtensionMethods
	{

        /// <summary>
        /// Unwraps parameter values and parameter types from a deserialized MethodCallMessage.
        /// </summary>
        /// <param name="callMessage">MethodCallMessage object</param>
        /// <param name="parameterValues">Out: Unwrapped parameter values</param>
        /// <param name="parameterTypes">Out: Unwrapped parameter types</param>
        public static void UnwrapParametersFromDeserializedMethodCallMessage(
            this MethodCallMessage callMessage, 
            out object[] parameterValues,
            out Type[] parameterTypes)
        {
            parameterTypes = new Type[callMessage.Parameters.Length];
            parameterValues = new object[callMessage.Parameters.Length];

            for (int i = 0; i < callMessage.Parameters.Length; i++)
            {
                var parameter = callMessage.Parameters[i];
                var parameterType = Type.GetType(parameter.ParameterTypeName);
                if (parameterType == null)
                    throw new Exception("Parameter type not found: " + parameter.ParameterTypeName);
                parameterTypes[i] = parameterType;

                parameterValues[i] =
                    parameter.IsValueNull
                        ? null
                        : parameter.Value is Envelope envelope
                            ? envelope.Value == null 
                                ? null
                                : Convert.ChangeType(envelope.Value, envelope.Type)
                            : parameter.Value;
            }
        }
    }
}
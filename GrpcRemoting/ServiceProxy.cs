using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using GrpcRemoting.RemoteDelegates;
using GrpcRemoting.RpcMessaging;
using stakx.DynamicProxy;

namespace GrpcRemoting
{
	public class GrpcRemotingClientProxy<T> : AsyncInterceptor
	{
		GrpcRemotingClient pClient;
		string pServiceName;

		public GrpcRemotingClientProxy(GrpcRemotingClient cli)
		{
			pClient = cli;
			pServiceName = typeof(T).Name;
		}

		protected override void Intercept(IInvocation invocation)
		{
			var args = invocation.Arguments;
			var targetMethod = invocation.Method;

			var arguments = MapArguments(args);

			pClient.CallbackToSetCallContext(targetMethod);

			var mess = pClient.pMessBuild.BuildMethodCallMessage(pClient.pSerializer, pServiceName, targetMethod, arguments);

			var wm = new WireCallMessage() { Data = mess };

			var bytes = pClient.pSerializer.Serialize(wm);

			MethodCallResultMessage resultMessage = null;
			
			pClient.Invoke(bytes, async (callback, res) =>
			{
				resultMessage = await HandleResponseAsync(callback, res, args).ConfigureAwait(false);
			});

			if (resultMessage == null)
			{
				invocation.ReturnValue = null;
				return;
			}

			if (resultMessage.Exception != null)
			{
				throw resultMessage.Exception.Capture();
			}

			var parameterInfos = targetMethod.GetParameters();

			foreach (var outParameterValue in resultMessage.OutParameters)
			{
				var parameterInfo =
					parameterInfos.First(p => p.Name == outParameterValue.ParameterName);

				args[parameterInfo.Position] =
					outParameterValue.IsOutValueNull
						? null
						: outParameterValue.OutValue is Envelope outParamEnvelope
							? outParamEnvelope.Value
							: outParameterValue.OutValue;
			}

			invocation.ReturnValue = resultMessage.ReturnValue;
		}

		private async Task<MethodCallResultMessage> HandleResponseAsync(byte[] callback, Func<byte[], Task> res, object[] args)
		{
			var callbackData = pClient.pSerializer.Deserialize<WireResponseMessage>(callback);

			switch (callbackData.ResponseType)
			{
				case ResponseType.Result:
					return (MethodCallResultMessage)callbackData.Data;

				case ResponseType.Delegate:
					{
						var dele = (DelegateCallMessage)callbackData.Data;

						var d = (Delegate)args[dele.Position];

						// not possible with async here?
						object result = null;
						Exception exception = null;

						try
						{
							// FIXME: but we need to know if the delegate has a result or not???!!!
							result = d.DynamicInvoke(dele.Arguments);
						}
						catch (Exception ex) when (!dele.OneWay) // PS: not eating exceptions here. what happen to the exception??
						{
							Exception ex2 = null;
							if (ex is TargetInvocationException tie)
								ex2 = tie.InnerException;

							exception = ex2.GetType().IsSerializable ? ex2 : new RemoteInvocationException(ex2.Message);
						}

						if (dele.OneWay)
							return null;

						DelegateCallResultMessage msg = null;
						if (exception != null)
							msg = new DelegateCallResultMessage() { Exception = exception };
						else
							msg = new DelegateCallResultMessage() { Result = result };

						var data = pClient.pSerializer.Serialize(msg);
						await res(data).ConfigureAwait(false);
					}
					break;
				default:
					throw new Exception();
			}

			return null;
		}

		protected override async ValueTask InterceptAsync(IAsyncInvocation invocation)
		{
			var args = invocation.Arguments;
			var targetMethod = invocation.Method;

			var arguments = MapArguments(args);

            pClient.CallbackToSetCallContext(targetMethod);

            var mess = pClient.pMessBuild.BuildMethodCallMessage(pClient.pSerializer, pServiceName, targetMethod, arguments);

			var wm = new WireCallMessage() { Data = mess };

			var bytes = pClient.pSerializer.Serialize(wm);

			MethodCallResultMessage resultMessage = null;

			//await?
			await pClient.InvokeAsync(bytes, async (callback, reqq) =>
			{
				resultMessage = await HandleResponseAsync(callback, reqq, args.ToArray()).ConfigureAwait(false);
			}).ConfigureAwait(false);

			if (resultMessage == null)
			{
				invocation.Result = null;
				return;
			}

			if (resultMessage.Exception != null)
			{
				throw resultMessage.Exception.Capture();
			}

			// out|ref not possible with async

			invocation.Result = resultMessage.ReturnValue;
		}

		/// <summary>
		/// Maps non serializable arguments into a serializable form.
		/// </summary>
		/// <param name="arguments">Arguments</param>
		/// <returns>Array of arguments (includes mapped ones)</returns>
		private object[] MapArguments(IEnumerable<object> arguments)
		{
			return arguments.Select(argument =>
			{
				var type = argument?.GetType();

				if (MapDelegateArgument(type, argument, out var mappedArgument))
					return mappedArgument;
				else
					return argument;

			}).ToArray();
		}

		/// <summary>
		/// Maps a delegate argument into a serializable RemoteDelegateInfo object.
		/// </summary>
		/// <param name="argumentType">Type of argument to be mapped</param>
		/// <param name="argument">Argument to be wrapped</param>
		/// <param name="mappedArgument">Out: Mapped argument</param>
		/// <returns>True if mapping applied, otherwise false</returns>
		private bool MapDelegateArgument(Type argumentType, object argument, out object mappedArgument)
		{
			if (argumentType == null || !typeof(Delegate).IsAssignableFrom(argumentType))
			{
				mappedArgument = argument;
				return false;
			}

			var meth = argumentType.GetMethod("Invoke");
			//var delegateReturnType = meth?.ReturnType;

//			bool hasResult = false;
	//		if (delegateReturnType != typeof(void))
		//	{
			//	hasResult = true;
			//}

			var remoteDelegateInfo =
				new RemoteDelegateInfo(
					delegateTypeName: argumentType.FullName, hasResult: meth.ReturnType != typeof(void));

			mappedArgument = remoteDelegateInfo;
			return true;
		}


  
    }

    public static class ExceptionExtensions
    {
		public static Exception Capture(this Exception ex)
		{
			ExceptionDispatchInfo.Capture(ex);
			return ex;
		}
    }
}

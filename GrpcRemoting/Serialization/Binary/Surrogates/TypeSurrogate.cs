/*
 * Based on code from Microsoft.Bot.Builder
 * https://github.com/CXuesong/BotBuilder.Standard
 * branch: netcore20+net45
 * BotBuilder.Standard/CSharp/Library/Microsoft.Bot.Builder/Fibers/NetStandardSerialization.cs
 * BotBuilder.Standard/CSharp/Library/Microsoft.Bot.Builder/Fibers/Serialization.cs
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace GrpcRemoting.Serialization.Binary
{

	public sealed class TypeSurrogate : ISerializationSurrogateEx
	{
		public bool Handles(Type type, StreamingContext context)
		{
			var handles = typeof(Type).IsAssignableFrom(type);
			return handles;
		}

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			var type = (Type)obj;
			// BinaryFormatter in .NET Core 2.0 cannot persist types in System.Private.CoreLib.dll
			// that are not forwareded to mscorlib, including System.RuntimeType
			info.SetType(typeof(TypeReference));
			info.AddValue("AssemblyName", type.Assembly.FullName);
			info.AddValue("FullName", type.FullName);
		}

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context,
			ISurrogateSelector selector)
		{
			throw new NotSupportedException();
			//var AssemblyQualifiedName = info.GetString("AssemblyQualifiedName");
			//return Type.GetType(AssemblyQualifiedName, true);
		}

		[Serializable]
		internal sealed class TypeReference : IObjectReference
		{

			private readonly string AssemblyName;

			private readonly string FullName;

			public TypeReference(Type type)
			{
				if (type == null) throw new ArgumentNullException(nameof(type));
				AssemblyName = type.Assembly.FullName;
				FullName = type.FullName;
			}

			public object GetRealObject(StreamingContext context)
			{
				var assembly = Assembly.Load(AssemblyName);
				return assembly.GetType(FullName, true);
			}
		}

	}
}

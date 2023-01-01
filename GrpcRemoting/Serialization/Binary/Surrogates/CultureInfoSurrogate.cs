using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace GrpcRemoting.Serialization.Binary
{
	internal class CultureInfoSurrogate : ISerializationSurrogateEx
	{
		public bool Handles(Type type, StreamingContext context)
		{
			var canHandle = type == typeof(CultureInfo);
			return canHandle;
		}

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			var ci = (CultureInfo)obj;
			info.AddValue("UseUserOverride", ci.UseUserOverride);
			info.AddValue("Name", ci.Name);
		}

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			return new CultureInfo(info.GetString("Name"), info.GetBoolean("UseUserOverride"));
		}
	}
}

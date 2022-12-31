using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcRemoting.RpcMessaging
{
	[Serializable]
	public class DelegateCallMessage
	{
		public int Position { get; set; }

		public object[] Arguments { get; set; }

		public bool OneWay { get; set; }
	}


	[Serializable]
	public class DelegateCallResultMessage
	{
		public object Result { get; set; }

		public Exception Exception { get; set; }
	}
}

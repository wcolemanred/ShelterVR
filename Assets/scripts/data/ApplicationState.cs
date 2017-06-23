using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFDC.wef.data
{
	abstract class ApplicationState
	{
		public static SalesforceConnection sfConnection { get; set; }
		public static Configuration configuration { get; set; }
	}
}

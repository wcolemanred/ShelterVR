using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFDC.wef
{
	/**
	 * WARNING: Never commit those values to a public repository
	 */
	public abstract class SalesforceAuthConfig
	{
		public const string OAUTH_CONSUMER_KEY = "";
		public const string OAUTH_CONSUMER_SECRET = "";

		// Used for testing individual scenes as standalone, clear these values in production
		public const string DEFAULT_USERNAME = "";
		public const string DEFAULT_PASSWORD = "";
	}
}

using NUnit.Framework;
using System;

namespace Payload.Tests
{
    public class PayloadTestSetup
    {
        public static void initAPI()
        {
            pl.api_key = Environment.GetEnvironmentVariable("API_KEY");
            string url = Environment.GetEnvironmentVariable("API_URL");
            if (url != null)
                pl.api_url = url;
        }
    }
}

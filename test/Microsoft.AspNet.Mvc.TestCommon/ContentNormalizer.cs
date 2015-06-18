// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNet.Testing;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
	public static class ContentNormalizer
	{
		public static string GetNormalizedContent(string input)
        {
            if (TestPlatformHelper.IsMono)
            {
            	var equivalents = new Dictionary<string, string> {
            		{
                        "The [0-9a-zA-Z ]+ field is required.", "RequiredAttribute_ValidationError"
            		},
            		{
            			"jx1PJjLX32-xgQQx2BxnckU9QH9DVKkm4-M5bSK869I", "XuZtb9pOw8cEVclF_STyhCYbkXhxA5pI1-wAWwNSjt0"
            		},
            		{
            			"pwJaxaQxnb-rPAdF2JlAp4xiPNq1XuJFd6TyOOfNF-0", "FpXjET7ic24mqzwBb14gAEyx3UyAzN93VEjjAq-xpMM"
            		},
            		{
            			"XY7YsMemPf8AGU4SIX9ED9eOjK1LOQWu2dmCNmh-pQc", "Zu84-YCnep6_vXSCMhCoRakWV3sGRQT-0GNB9Y3LggE"
            		},
            		{
            			"30cxPex0tA9xEatW7f1Qhnn8tVLAHgE6xwIZhESq0y0", "zMzrhkSY7Xu-14If5sqTivWOLJsgeHnpSGFt8gEULGM"
            		},
            		{
            			"fSxxOr1Q4Dq2uPuzlju5UYGuK0SKABI-ghvaIGEsZDc", "6j_KWFdZt0S2IEXcXCqYO5ALv8-d798jrNnZ0ANKoek"
            		},
            		{
            			"s8JMmAZxBn0dzuhRtQ0wgOvNBK4XRJRWEC2wfzsVF9M", "Jqse5TR9OJ4urpPLWCenCNaw7qORcflRPxCAzGIDNVs"
            		}
            	};

            	var result = input;

                foreach (var kvp in equivalents)
                {
                	result = Regex.Replace(result, kvp.Key, kvp.Value);
                }

                return result;
            }

            return input;
        }
    }  
}
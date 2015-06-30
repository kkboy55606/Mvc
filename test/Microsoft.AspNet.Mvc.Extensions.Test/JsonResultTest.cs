// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Testing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class JsonResultTest
    {
        private static readonly byte[] _abcdUTF8Bytes
            = new byte[] { 123, 34, 102, 111, 111, 34, 58, 34, 97, 98, 99, 100, 34, 125 };

        // Newlines dont have /r in mono.
        private static readonly byte[] _abcdIndentedUTF8Bytes
            = TestPlatformHelper.IsMono ?
            new byte[] { 123, 10, 32, 32, 34, 102, 111, 111, 34, 58, 32, 34, 97, 98, 99, 100, 34, 10, 125 } :
            new byte[] { 123, 13, 10, 32, 32, 34, 102, 111, 111, 34, 58, 32, 34, 97, 98, 99, 100, 34, 13, 10, 125 };

        [Fact]
        public async Task ExecuteResultAsync_UsesDefaultContentType_IfNoContentTypeSpecified()
        {
            // Arrange
            var expected = _abcdUTF8Bytes;

            var context = GetHttpContext();
            var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());

            var result = new JsonResult(new { foo = "abcd" });

            // Act
            await result.ExecuteResultAsync(actionContext);
            var written = GetWrittenBytes(context);

            // Assert
            Assert.Equal(expected, written);
            Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
        }

        [Fact]
        public async Task ExecuteResultAsync_NullEncoding_SetsContentTypeAndDefaultEncoding()
        {
            // Arrange
            var expected = _abcdUTF8Bytes;

            var context = GetHttpContext();
            var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());

            var result = new JsonResult(new { foo = "abcd" });
            result.ContentType = new MediaTypeHeaderValue("text/json");

            // Act
            await result.ExecuteResultAsync(actionContext);
            var written = GetWrittenBytes(context);

            // Assert
            Assert.Equal(expected, written);
            Assert.Equal("text/json; charset=utf-8", context.Response.ContentType);
        }

        [Fact]
        public async Task ExecuteResultAsync_SetsContentTypeAndEncoding()
        {
            // Arrange
            var expected = _abcdUTF8Bytes;

            var context = GetHttpContext();
            var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());

            var result = new JsonResult(new { foo = "abcd" });
            result.ContentType = new MediaTypeHeaderValue("text/json")
            {
                Encoding = Encoding.ASCII
            };

            // Act
            await result.ExecuteResultAsync(actionContext);
            var written = GetWrittenBytes(context);

            // Assert
            Assert.Equal(expected, written);
            Assert.Equal("text/json; charset=us-ascii", context.Response.ContentType);
        }

        [Fact]
        public async Task ExecuteResultAsync_UsesPassedInSerializerSettings()
        {
            // Arrange
            var expected = _abcdIndentedUTF8Bytes;

            var context = GetHttpContext();
            var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Formatting = Formatting.Indented;

            var result = new JsonResult(new { foo = "abcd" }, serializerSettings);

            // Act
            await result.ExecuteResultAsync(actionContext);
            var written = GetWrittenBytes(context);

            // Assert
            Assert.Equal(expected, written);
            Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
        }

        private static HttpContext GetHttpContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            var services = new ServiceCollection();
            services.AddOptions();
            httpContext.RequestServices = services.BuildServiceProvider();

            return httpContext;
        }

        private static byte[] GetWrittenBytes(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            return Assert.IsType<MemoryStream>(context.Response.Body).ToArray();
        }
    }
}
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Razor.Chunks;
using Microsoft.AspNet.Testing;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor.Directives
{
    public class ChunkInheritanceUtilityTest
    {
        [Fact]
        public void GetInheritedChunks_ReadsChunksFromGlobalFilesInPath()
        {
            // Arrange
            var fileProvider = new TestFileProvider();
            fileProvider.AddFile(NormalizePath(@"Views\accounts\_ViewImports.cshtml"), "@using AccountModels");
            fileProvider.AddFile(NormalizePath(@"Views\Shared\_ViewImports.cshtml"), "@inject SharedHelper Shared");
            fileProvider.AddFile(NormalizePath(@"Views\home\_ViewImports.cshtml"), "@using MyNamespace");
            fileProvider.AddFile(NormalizePath(@"Views\_ViewImports.cshtml"),
@"@inject MyHelper<TModel> Helper
@inherits MyBaseType

@{
    Layout = ""test.cshtml"";
}

");
            var defaultChunks = new Chunk[]
            {
                new InjectChunk("MyTestHtmlHelper", "Html"),
                new UsingChunk { Namespace = "AppNamespace.Model" },
            };
            var cache = new DefaultChunkTreeCache(fileProvider);
            var host = new MvcRazorHost(cache);
            var utility = new ChunkInheritanceUtility(host, cache, defaultChunks);

            // Act
            var chunkTrees = utility.GetInheritedChunkTrees(NormalizePath(@"Views\home\Index.cshtml"));

            // Assert
            Assert.Collection(chunkTrees,
                chunkTree =>
                {
                    var viewImportsPath = NormalizePath(@"Views\home\_ViewImports.cshtml");
                    Assert.Collection(chunkTree.Chunks,
                        chunk =>
                        {
                            Assert.IsType<LiteralChunk>(chunk);
                            Assert.Equal(viewImportsPath, chunk.Start.FilePath);
                        },
                        chunk =>
                        {
                            var usingChunk = Assert.IsType<UsingChunk>(chunk);
                            Assert.Equal("MyNamespace", usingChunk.Namespace);
                            Assert.Equal(viewImportsPath, chunk.Start.FilePath);
                        },
                        chunk =>
                        {
                            Assert.IsType<LiteralChunk>(chunk);
                            Assert.Equal(viewImportsPath, chunk.Start.FilePath);
                        });
                },
                chunkTree =>
                {
                    var viewImportsPath = NormalizePath(@"Views\_ViewImports.cshtml");
                    Assert.Collection(chunkTree.Chunks,
                        chunk =>
                        {
                            Assert.IsType<LiteralChunk>(chunk);
                            Assert.Equal(viewImportsPath, chunk.Start.FilePath);
                        },
                        chunk =>
                        {
                            var injectChunk = Assert.IsType<InjectChunk>(chunk);
                            Assert.Equal("MyHelper<TModel>", injectChunk.TypeName);
                            Assert.Equal("Helper", injectChunk.MemberName);
                            Assert.Equal(viewImportsPath, chunk.Start.FilePath);
                        },
                        chunk =>
                        {
                            Assert.IsType<LiteralChunk>(chunk);
                            Assert.Equal(viewImportsPath, chunk.Start.FilePath);
                        },
                        chunk =>
                        {
                            var setBaseTypeChunk = Assert.IsType<SetBaseTypeChunk>(chunk);
                            Assert.Equal("MyBaseType", setBaseTypeChunk.TypeName);
                            Assert.Equal(viewImportsPath, chunk.Start.FilePath);

                        },
                        chunk =>
                        {
                            Assert.IsType<LiteralChunk>(chunk);
                            Assert.Equal(viewImportsPath, chunk.Start.FilePath);
                        },
                        chunk =>
                        {
                            Assert.IsType<StatementChunk>(chunk);
                            Assert.Equal(viewImportsPath, chunk.Start.FilePath);
                        },
                        chunk =>
                        {
                            Assert.IsType<LiteralChunk>(chunk);
                            Assert.Equal(viewImportsPath, chunk.Start.FilePath);
                        });
                });
        }

        [Fact]
        public void GetInheritedChunks_ReturnsEmptySequenceIfNoGlobalsArePresent()
        {
            // Arrange
            var fileProvider = new TestFileProvider();
            fileProvider.AddFile(NormalizePath(@"_ViewImports.cs"), string.Empty);
            fileProvider.AddFile(NormalizePath(@"Views\_Layout.cshtml"), string.Empty);
            fileProvider.AddFile(NormalizePath(@"Views\home\_not-viewimports.cshtml"), string.Empty);
            var cache = new DefaultChunkTreeCache(fileProvider);
            var host = new MvcRazorHost(cache);
            var defaultChunks = new Chunk[]
            {
                new InjectChunk("MyTestHtmlHelper", "Html"),
                new UsingChunk { Namespace = "AppNamespace.Model" },
            };
            var utility = new ChunkInheritanceUtility(host, cache, defaultChunks);

            // Act
            var chunkTrees = utility.GetInheritedChunkTrees(NormalizePath(@"Views\home\Index.cshtml"));

            // Assert
            Assert.Empty(chunkTrees);
        }

        [Fact]
        public void MergeInheritedChunks_MergesDefaultInheritedChunks()
        {
            // Arrange
            var fileProvider = new TestFileProvider();
            fileProvider.AddFile(NormalizePath(@"Views\_ViewImports.cshtml"),
                               "@inject DifferentHelper<TModel> Html");
            var cache = new DefaultChunkTreeCache(fileProvider);
            var host = new MvcRazorHost(cache);
            var defaultChunks = new Chunk[]
            {
                new InjectChunk("MyTestHtmlHelper", "Html"),
                new UsingChunk { Namespace = "AppNamespace.Model" },
            };
            var inheritedChunkTrees = new ChunkTree[]
            {
                new ChunkTree
                {
                    Chunks = new Chunk[]
                    {
                        new UsingChunk { Namespace = "InheritedNamespace" },
                        new LiteralChunk { Text = "some text" }
                    }
                },
                new ChunkTree
                {
                    Chunks = new Chunk[]
                    {
                        new UsingChunk { Namespace = "AppNamespace.Model" },
                    }
                }
            };

            var utility = new ChunkInheritanceUtility(host, cache, defaultChunks);
            var chunkTree = new ChunkTree();

            // Act
            utility.MergeInheritedChunkTrees(chunkTree,
                                            inheritedChunkTrees,
                                            "dynamic");

            // Assert
            Assert.Equal(3, chunkTree.Chunks.Count);
            Assert.Same(inheritedChunkTrees[0].Chunks[0], chunkTree.Chunks[0]);
            Assert.Same(inheritedChunkTrees[1].Chunks[0], chunkTree.Chunks[1]);
            Assert.Same(defaultChunks[0], chunkTree.Chunks[2]);
        }

        private string NormalizePath(string path)
        {
            if (TestPlatformHelper.IsMono)
            {
                return path.Replace("\\", "/");
            }
            
            return path;
        }
    }
}
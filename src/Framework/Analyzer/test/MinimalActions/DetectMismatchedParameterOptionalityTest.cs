// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Microsoft.AspNetCore.Analyzer.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Analyzers.DelegateEndpoints;

public partial class DetectMismatchedParameterOptionalityTest
{
    private TestDiagnosticAnalyzerRunner Runner { get; } = new(new DelegateEndpointAnalyzer());

    [Fact]
    public async Task MatchingRequiredOptionality_DoesNotProduceDiagnostics()
    {
        // Arrange
        var source = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
var app = WebApplication.Create();
app.MapGet(""/hello/{name}"", (string name) => $""Hello {name}"");
";
        // Act
        var diagnostics = await Runner.GetDiagnosticsAsync(source);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task ParameterFromRouteOrQueryQuery_DoesNotProduceDiagnostics()
    {
        // Arrange
        var source = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
var app = WebApplication.Create();
app.MapGet(""/hello"", (string name) => $""Hello {name}"");
";
        // Act
        var diagnostics = await Runner.GetDiagnosticsAsync(source);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task OptionalRouteParamRequiredArgument_ProducesDiagnostics()
    {
        // Console.WriteLine($"Waiting for debugger to attach for {System.Environment.ProcessId}");
        // while (!System.Diagnostics.Debugger.IsAttached)
        // {
        //     Thread.Sleep(100);
        // }
        // Console.WriteLine("Debugger attached");
        // Arrange
        var source = TestSource.Read(@"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
var app = WebApplication.Create();
app.MapGet(""/hello/{name?}"", /*MM*/(string name) => $""Hello {name}"");
");
        // Act
        var diagnostics = await Runner.GetDiagnosticsAsync(source.Source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Same(DiagnosticDescriptors.DetectMismatchedParameterOptionality, diagnostic.Descriptor);
        AnalyzerAssert.DiagnosticLocation(source.DefaultMarkerLocation, diagnostic.Location);
        Assert.Equal("'name' argument should be annotated as optional to match route parameter", diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }


    [Fact]
    public async Task OptionalRouteParamRequiredArgument_WithRouteConstraint_ProducesDiagnostics()
    {
        // Console.WriteLine($"Waiting for debugger to attach for {System.Environment.ProcessId}");
        // while (!System.Diagnostics.Debugger.IsAttached)
        // {
        //     Thread.Sleep(100);
        // }
        // Console.WriteLine("Debugger attached");
        // Arrange
        var source = TestSource.Read(@"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
var app = WebApplication.Create();
app.MapGet(""/hello/{age:int?}"", /*MM*/(int age) => $""Age: {age}"");
");
        // Act
        var diagnostics = await Runner.GetDiagnosticsAsync(source.Source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Same(DiagnosticDescriptors.DetectMismatchedParameterOptionality, diagnostic.Descriptor);
        AnalyzerAssert.DiagnosticLocation(source.DefaultMarkerLocation, diagnostic.Location);
        Assert.Equal("'age' argument should be annotated as optional to match route parameter", diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task OptionalRouteParamRequiredArgument_WithFromRoute_ProducesDiagnostics()
    {
        // Console.WriteLine($"Waiting for debugger to attach for {System.Environment.ProcessId}");
        // while (!System.Diagnostics.Debugger.IsAttached)
        // {
        //     Thread.Sleep(100);
        // }
        // Console.WriteLine("Debugger attached");
        // Arrange
        var source = TestSource.Read(@"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
var app = WebApplication.Create();
app.MapGet(""/hello/{Age?}"", /*MM*/([FromRoute] int age) => $""Age: {age}"");
");
        // Act
        var diagnostics = await Runner.GetDiagnosticsAsync(source.Source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Same(DiagnosticDescriptors.DetectMismatchedParameterOptionality, diagnostic.Descriptor);
        AnalyzerAssert.DiagnosticLocation(source.DefaultMarkerLocation, diagnostic.Location);
        Assert.Equal("'age' argument should be annotated as optional to match route parameter", diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task RequiredRouteParamOptionalArgument_DoesNotProduceDiagnostics()
    {
        // Arrange
        var source = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
var app = WebApplication.Create();
app.MapGet(""/hello/{name}"", (string? name) => $""Hello {name}"");
";
        // Act
        var diagnostics = await Runner.GetDiagnosticsAsync(source);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task MatchingOptionality_DoesNotProduceDiagnostics()
    {
        // Console.WriteLine($"Waiting for debugger to attach for {System.Environment.ProcessId}");
        // while (!System.Diagnostics.Debugger.IsAttached)
        // {
        //     Thread.Sleep(100);
        // }
        // Console.WriteLine("Debugger attached");
        // Arrange
        var source = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
var app = WebApplication.Create();
app.MapGet(""/hello/{name?}"", (string? name) => $""Hello {name}"");
";
        // Act
        var diagnostics = await Runner.GetDiagnosticsAsync(source);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task OptionalRouteParamRequiredArgument_MethodGroup_ProducesDiagnostics()
    {
        // Console.WriteLine($"Waiting for debugger to attach for {System.Environment.ProcessId}");
        // while (!System.Diagnostics.Debugger.IsAttached)
        // {
        //     Thread.Sleep(100);
        // }
        // Console.WriteLine("Debugger attached");
        // Arrange
        var source = TestSource.Read(@"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
var app = WebApplication.Create();
string SayHello(string name) => $""Hello {name}"";
app.MapGet(""/hello/{name?}"", /*MM*/SayHello);
");
        // Act
        var diagnostics = await Runner.GetDiagnosticsAsync(source.Source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Same(DiagnosticDescriptors.DetectMismatchedParameterOptionality, diagnostic.Descriptor);
        AnalyzerAssert.DiagnosticLocation(source.DefaultMarkerLocation, diagnostic.Location);
        // Assert.Equal("AuthorizeAttribute should be placed on the delegate instead of Hello", diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }
}
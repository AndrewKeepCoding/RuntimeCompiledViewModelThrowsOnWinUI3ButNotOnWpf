using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ViewModelCreatorLib;

public class BasicViewModel : System.ComponentModel.INotifyPropertyChanged
{
    private string _someText = string.Empty;

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    public string SomeText
    {
        get => _someText;
        set
        {
            if (_someText != value)
            {
                _someText = value;
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(SomeText)));
            }
        }
    }
}

public static class ViewModelCreator
{
    // Creates a ViewModel with the 'new' keyword.
    public static object? CreateWithNewKeyword() => new BasicViewModel();

    // Creates a ViewModel with reflection.
    public static object? CreateWithReflection()
    {
        var viewModelType = typeof(BasicViewModel);
        var viewModel = Activator.CreateInstance(viewModelType);
        return viewModel;
    }

    // Compiles at runtime and creates a ViewModel.
    // Exception thrown:
    // 'System.ArgumentNullException' in System.Private.CoreLib.dll
    public static object? CreateAtRuntime()
    {
        string viewModelCSharpCode = GetViewModelCSharpCode();
        Assembly viewModelAssemblly = CreateViewModelAssembly(viewModelCSharpCode);
        return viewModelAssemblly.CreateInstance("DynamicViewModel");
    }

    private static string GetViewModelCSharpCode()
    {
        return
            """
            public class DynamicViewModel : System.ComponentModel.INotifyPropertyChanged
            {
                private string _someText = string.Empty;

                public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

                public string SomeText
                {
                    get => _someText;
                    set
                    {
                        if (_someText != value)
                        {
                            _someText = value;
                            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(SomeText)));
                        }
                    }
                }
            }
            """;
    }

    private static Assembly CreateViewModelAssembly(string viewModelCode)
    {
        CSharpCompilationOptions cSharpCompilationOptions = new(
            outputKind: OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: NullableContextOptions.Enable);
        SyntaxTree viewModelClassSyntaxTree = CSharpSyntaxTree.ParseText(viewModelCode);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                assemblyName: "SomeAssembly",
                options: cSharpCompilationOptions,
                references: CreateReferences())
                .AddSyntaxTrees(viewModelClassSyntaxTree);

        using var stream = new MemoryStream();
        EmitResult emitResult = compilation.Emit(stream);

        if (emitResult.Success is false)
        {
            throw new Exception("Failed to compile the view model");
        }

        _ = stream.Seek(0, SeekOrigin.Begin);

        return Assembly.Load(stream.ToArray());
    }

    private static List<PortableExecutableReference> CreateReferences()
    {
        string runtimePath = RuntimeEnvironment.GetRuntimeDirectory();

        List<PortableExecutableReference> references = new()
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(runtimePath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.INotifyPropertyChanged).GetTypeInfo().Assembly.Location),
            };

        return references;
    }
}

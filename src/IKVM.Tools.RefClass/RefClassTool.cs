using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using IKVM.ByteCode;
using IKVM.ByteCode.Buffers;
using IKVM.ByteCode.Decoding;
using IKVM.ByteCode.Encoding;
using IKVM.Tools.Core.Diagnostics;

using Microsoft.Extensions.DependencyInjection;

namespace IKVM.Tools.RefClass
{

    /// <summary>
    /// Implements the 'refcls' program entry point.
    /// </summary>
    public class RefClassTool : RootCommand
    {

        readonly static BlobBuilder methodBody;
        readonly static CodeBuilder methodCode;

        /// <summary>
        /// Initializes the static instance.
        /// </summary>
        static RefClassTool()
        {
            methodBody = new BlobBuilder();
            methodCode = new CodeBuilder(methodBody);
            methodCode.AconstNull();
            methodCode.Athrow();
        }

        /// <summary>
        /// Main application entry point.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<int> MainAsync(string[] args, CancellationToken cancellationToken)
        {
            return new RefClassTool().InvokeAsync(args, cancellationToken);
        }

        /// <summary>
        /// Output directory path.
        /// </summary>
        readonly Option<string> OutputDir = new Option<string>("-d")
        {
            Description = "Where to place rewritten class files",
            DefaultValueFactory = r => Environment.CurrentDirectory,
        };

        /// <summary>
        /// Set of input classes to process.
        /// </summary>
        readonly Argument<string[]> Classes = new("classes")
        {
            Arity = ArgumentArity.OneOrMore
        };

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        RefClassTool() : base("IKVM Reference Class Tool")
        {
            Options.Add(OutputDir);
            Arguments.Add(Classes);
            SetAction(RunAsync);
        }

        /// <summary>
        /// Invokes the program with the specified arguments.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> InvokeAsync(string[] args, CancellationToken cancellationToken)
        {
            return await Parse(args).InvokeAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Invoked when the program is running.
        /// </summary>
        /// <param name="result"></param>
        async Task<int> RunAsync(ParseResult result, CancellationToken cancellationToken)
        {
            var services = new ServiceCollection();
            services.AddToolsDiagnostics();
            using var provider = services.BuildServiceProvider();

            var classes = result.GetRequiredValue(Classes);
            if (classes.Length <= 0)
                throw new InvalidOperationException();

            var outputDir = result.GetValue(OutputDir);
            if (string.IsNullOrWhiteSpace(outputDir))
                throw new InvalidOperationException();

            // process ecah class asynchronously
            foreach (var t in classes.Select(c => Task.Run(() => ProcessAsync(outputDir, c, cancellationToken))).ToList())
                if (await t == false)
                    return 1;

            return 0;
        }

        /// <summary>
        /// Processes each individual class file.
        /// </summary>
        /// <param name="clazz"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<bool> ProcessAsync(string outputDir, string clazz, CancellationToken cancellationToken)
        {
            if (ClassFile.TryRead(clazz, out var cf) == false)
                return false;

            Console.WriteLine(clazz);

            if (cf is null)
                return false;

            try
            {
                if (cf.Constants.Get(cf.This).Name is not string name)
                    throw new InvalidOperationException();

                string? super = null;
                if (cf.Super.IsNotNil)
                    super = cf.Constants.Get(cf.Super).Name;

                var cb = new ClassFileBuilder(cf.Version, cf.AccessFlags, name, super);

                foreach (var iface in cf.Interfaces)
                    Translate(cf, cb, iface);

                foreach (var field in cf.Fields)
                    Translate(cf, cb, field);

                foreach (var method in cf.Methods)
                    Translate(cf, cb, method);

                var ab = new AttributeTableBuilder(cb.Constants);
                foreach (var attribute in cf.Attributes)
                    Translate(cf, cb, ab, attribute, null);

                // serialize class to blob
                var b = new BlobBuilder();
                cb.Serialize(b);

                // get output directory
                var d = Path.Combine([outputDir, .. name.Split('/')[..^1]]);
                Directory.CreateDirectory(d);

                // calculate file path
                var n = Path.ChangeExtension(name.Split('/')[^1], ".class");
                var p = Path.Combine(d, n);

                // open the output file and write the contents
                using (var cs = File.Open(p, FileMode.Create, FileAccess.Write, FileShare.None))
                    b.WriteContentTo(cs);

                return true;
            }
            finally
            {
                cf?.Dispose();
            }
        }

        void Translate(ClassFile cf, ClassFileBuilder cb, Interface iface)
        {
            var clazz = cf.Constants.Get(iface.Class);
            if (clazz.Name is string name)
                cb.AddInterface(name);
        }

        void Translate(ClassFile cf, ClassFileBuilder cb, Field field)
        {
            var ab = new AttributeTableBuilder(cb.Constants);
            foreach (var attribute in field.Attributes)
                Translate(cf, cb, ab, attribute, null);

            cb.AddField(field.AccessFlags, cf.Constants.Get(field.Name).Value, cf.Constants.Get(field.Descriptor).Value, ab);
        }

        void Translate(ClassFile cf, ClassFileBuilder cb, Method method)
        {
            var ab = new AttributeTableBuilder(cb.Constants);
            foreach (var attribute in method.Attributes)
                Translate(cf, cb, ab, attribute, method);

            cb.AddMethod(method.AccessFlags, cf.Constants.Get(method.Name).Value, cf.Constants.Get(method.Descriptor).Value, ab);
        }

        void Translate(ClassFile cf, ClassFileBuilder cb, AttributeTableBuilder ab, IKVM.ByteCode.Decoding.Attribute attribute, Method? method)
        {
            switch (cf.Constants.Get(attribute.Name).Value)
            {
                case AttributeName.Code when method is not null:
                    Translate(cf, cb, ab, attribute.AsCode(), method.Value);
                    break;
                case AttributeName.Code:
                    throw new InvalidOperationException("Code attribute without method.");
                case AttributeName.LineNumberTable:
                case AttributeName.StackMapTable:
                case AttributeName.LocalVariableTable:
                case AttributeName.LocalVariableTypeTable:
                    break;
                default:
                    attribute.CopyTo(cf.Constants, cb.Constants, ref ab.Encoder);
                    break;
            }
        }

        void Translate(ClassFile cf, ClassFileBuilder cb, AttributeTableBuilder ab, CodeAttribute attribute, Method method)
        {
            var ab2 = new AttributeTableBuilder(cb.Constants);
            foreach (var attribute2 in attribute.Attributes)
                Translate(cf, cb, ab2, attribute2, null);

            switch (cf.Constants.Get(method.Name).Value)
            {
                case "<init>" when cf.Constants.Get(cf.Super).Name == "cli/System/MulticastDelegate":
                    // init methods of delegates should continue to call base init
                    {
                        var b = new BlobBuilder();
                        var c = new CodeBuilder(b);
                        c.Aload0();
                        c.InvokeSpecial(ConstantHandle.Nil);
                        c.Return();

                        ab.Code(4, 255, b, e => c.WriteExceptionsTo(ref e), ab2);
                        break;
                    }

                default:
                    {
                        var b = new BlobBuilder();
                        methodBody.WriteContentTo(b);
                        ab.Code(4, 255, b, e => methodCode.WriteExceptionsTo(ref e), ab2);
                        break;
                    }
            }
        }

    }

}

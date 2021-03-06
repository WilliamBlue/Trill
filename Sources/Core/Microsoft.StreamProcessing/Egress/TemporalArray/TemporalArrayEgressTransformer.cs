﻿// *********************************************************************
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License
// *********************************************************************
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.StreamProcessing
{
    internal partial class TemporalArrayEgressTemplate
    {
        private static int TemporalArrayEgressSequenceNumber = 0;

        private string className;
        private string staticCtor;

        private string TKey;
        private string TPayload;
        private string TResult;

        private Func<string, string, string> startEdgeFunction;
        private Func<string, string, string, string> intervalFunction;

        private string partitionString;
        private string ingressType;

        private string genericArguments;
        private string egress;
        private string inputKey;
        private string partitionKeyArgument;

        private string BatchGeneratedFrom_TKey_TPayload;
        private string TKeyTPayloadGenericParameters;
        private IEnumerable<MyFieldInfo> fields;
        private ColumnarRepresentation payloadRepresentation;
        private bool isColumnar;

        private TemporalArrayEgressTemplate(Type tKey, Type tPayload, Type tResult, string partitionString, string ingressType, bool isColumnar)
        {
            var tm = new TypeMapper(tKey, tPayload, tResult);

            this.className = string.Format("GeneratedTemporalArrayEgress_{0}", TemporalArrayEgressSequenceNumber++);

            this.payloadRepresentation = new ColumnarRepresentation(tPayload);

            this.BatchGeneratedFrom_TKey_TPayload = Transformer.GetBatchClassName(
                tKey == typeof(Empty)
                ? tKey
                : typeof(PartitionKey<>).MakeGenericType(tKey), tPayload);
            this.TKeyTPayloadGenericParameters = tm.GenericTypeVariables(tKey, tPayload).BracketedCommaSeparatedString();

            this.fields = this.payloadRepresentation.AllFields;

            this.staticCtor = Transformer.StaticCtor(this.className);

            this.TKey = tm.CSharpNameFor(tKey);
            this.TPayload = tm.CSharpNameFor(tPayload);
            this.TResult = tm.CSharpNameFor(tResult);

            this.partitionString = partitionString;
            this.ingressType = ingressType;

            this.partitionKeyArgument = !string.IsNullOrEmpty(partitionString) ? "colkey[i].Key, " : string.Empty;
            this.genericArguments = string.IsNullOrEmpty(partitionString) ? this.TPayload : this.TKey + ", " + this.TPayload;
            this.egress = (ingressType != "StreamEvent")
                ? this.TResult
                : partitionString + "StreamEvent<" + this.genericArguments + ">";
            this.inputKey = string.IsNullOrEmpty(partitionString) ? this.TKey : "PartitionKey<" + this.TKey + ">";
            this.isColumnar = isColumnar;
        }

        internal static Tuple<Type, string> Generate<TPayload>(StreamEventArrayObservable<TPayload> streamEventObservable)
        {
#if CODEGEN_TIMING
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            string errorMessages = null;
            try
            {
                var template = new TemporalArrayEgressTemplate(typeof(Empty), typeof(TPayload), typeof(TPayload), string.Empty, "StreamEvent", streamEventObservable.source.Properties.IsColumnar);
                var keyType = typeof(Empty);
                var expandedCode = template.TransformText();

                var assemblyReferences = Transformer.AssemblyReferencesNeededFor(keyType, typeof(TPayload));
                assemblyReferences.Add(typeof(IStreamable<,>).GetTypeInfo().Assembly);
                assemblyReferences.Add(Transformer.GeneratedStreamMessageAssembly<Empty, TPayload>());

                var a = Transformer.CompileSourceCode(expandedCode, assemblyReferences, out errorMessages);
                var t = a.GetType(template.className);
#if CODEGEN_TIMING
              sw.Stop();
              Console.WriteLine("Time to generate and instantiate a IOOEJ operator: {0}ms", sw.ElapsedMilliseconds);
#endif
                return Tuple.Create(t, errorMessages);
            }
            catch
            {
#if CODEGEN_TIMING
              sw.Stop();
#endif
                if (Config.CodegenOptions.DontFallBackToRowBasedExecution)
                {
                    throw new InvalidOperationException("Code Generation failed when it wasn't supposed to!");
                }
                return Tuple.Create((Type)null, errorMessages);
            }
        }

        internal static Tuple<Type, string> Generate<TPayload, TResult>(StartEdgeArrayObservable<TPayload, TResult> startEdgeObservable)
        {
#if CODEGEN_TIMING
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            string errorMessages = null;
            try
            {
                var template = new TemporalArrayEgressTemplate(typeof(Empty), typeof(TPayload), typeof(TResult), string.Empty, "StartEdge", startEdgeObservable.source.Properties.IsColumnar);
                if (startEdgeObservable.constructor != null)
                    template.startEdgeFunction = ((x, y) => startEdgeObservable.constructor.Body.ExpressionToCSharpStringWithParameterSubstitution(
                                                new Dictionary<ParameterExpression, string>
                                                        {
                                                            { startEdgeObservable.constructor.Parameters[0], x },
                                                            { startEdgeObservable.constructor.Parameters[1], y },
                                                        }));
                var keyType = typeof(Empty);
                var expandedCode = template.TransformText();

                var assemblyReferences = Transformer.AssemblyReferencesNeededFor(keyType, typeof(TPayload), typeof(TResult));
                assemblyReferences.Add(typeof(IStreamable<,>).GetTypeInfo().Assembly);
                assemblyReferences.Add(Transformer.GeneratedStreamMessageAssembly<Empty, TPayload>());

                var a = Transformer.CompileSourceCode(expandedCode, assemblyReferences, out errorMessages);
                var t = a.GetType(template.className);
#if CODEGEN_TIMING
              sw.Stop();
              Console.WriteLine("Time to generate and instantiate a IOOEJ operator: {0}ms", sw.ElapsedMilliseconds);
#endif
                return Tuple.Create(t, errorMessages);
            }
            catch
            {
#if CODEGEN_TIMING
              sw.Stop();
#endif
                if (Config.CodegenOptions.DontFallBackToRowBasedExecution)
                {
                    throw new InvalidOperationException("Code Generation failed when it wasn't supposed to!");
                }
                return Tuple.Create((Type)null, errorMessages);
            }
        }

        internal static Tuple<Type, string> Generate<TPayload, TResult>(IntervalArrayObservable<TPayload, TResult> intervalObservable)
        {
#if CODEGEN_TIMING
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            string errorMessages = null;
            try
            {
                var template = new TemporalArrayEgressTemplate(typeof(Empty), typeof(TPayload), typeof(TResult), string.Empty, "Interval", intervalObservable.source.Properties.IsColumnar);
                if (intervalObservable.constructor != null)
                    template.intervalFunction = ((x, y, z) => intervalObservable.constructor.Body.ExpressionToCSharpStringWithParameterSubstitution(
                                                new Dictionary<ParameterExpression, string>
                                                        {
                                                            { intervalObservable.constructor.Parameters[0], x },
                                                            { intervalObservable.constructor.Parameters[1], y },
                                                            { intervalObservable.constructor.Parameters[2], z },
                                                        }));
                var keyType = typeof(Empty);
                var expandedCode = template.TransformText();

                var assemblyReferences = Transformer.AssemblyReferencesNeededFor(keyType, typeof(TPayload), typeof(TResult));
                assemblyReferences.Add(typeof(IStreamable<,>).GetTypeInfo().Assembly);
                assemblyReferences.Add(Transformer.GeneratedStreamMessageAssembly<Empty, TPayload>());

                var a = Transformer.CompileSourceCode(expandedCode, assemblyReferences, out errorMessages);
                var t = a.GetType(template.className);
#if CODEGEN_TIMING
              sw.Stop();
              Console.WriteLine("Time to generate and instantiate a IOOEJ operator: {0}ms", sw.ElapsedMilliseconds);
#endif
                return Tuple.Create(t, errorMessages);
            }
            catch
            {
#if CODEGEN_TIMING
              sw.Stop();
#endif
                if (Config.CodegenOptions.DontFallBackToRowBasedExecution)
                {
                    throw new InvalidOperationException("Code Generation failed when it wasn't supposed to!");
                }
                return Tuple.Create((Type)null, errorMessages);
            }
        }

        internal static Tuple<Type, string> Generate<TKey, TPayload>(PartitionedStreamEventArrayObservable<TKey, TPayload> partitionedStreamEventObservable)
        {
#if CODEGEN_TIMING
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            string errorMessages = null;
            try
            {
                var template = new TemporalArrayEgressTemplate(typeof(TKey), typeof(TPayload), typeof(TPayload), "Partitioned", "StreamEvent", partitionedStreamEventObservable.source.Properties.IsColumnar);

                var keyType = typeof(PartitionKey<>).MakeGenericType(typeof(TKey));
                var expandedCode = template.TransformText();

                var assemblyReferences = Transformer.AssemblyReferencesNeededFor(keyType, typeof(TPayload));
                assemblyReferences.Add(typeof(IStreamable<,>).GetTypeInfo().Assembly);
                assemblyReferences.Add(Transformer.GeneratedStreamMessageAssembly<PartitionKey<TKey>, TPayload>());

                var a = Transformer.CompileSourceCode(expandedCode, assemblyReferences, out errorMessages);
                var t = a.GetType(template.className);
#if CODEGEN_TIMING
              sw.Stop();
              Console.WriteLine("Time to generate and instantiate a IOOEJ operator: {0}ms", sw.ElapsedMilliseconds);
#endif
                return Tuple.Create(t, errorMessages);
            }
            catch
            {
#if CODEGEN_TIMING
              sw.Stop();
#endif
                if (Config.CodegenOptions.DontFallBackToRowBasedExecution)
                {
                    throw new InvalidOperationException("Code Generation failed when it wasn't supposed to!");
                }
                return Tuple.Create((Type)null, errorMessages);
            }
        }

        internal static Tuple<Type, string> Generate<TKey, TPayload, TResult>(PartitionedStartEdgeArrayObservable<TKey, TPayload, TResult> partitionedStartEdgeObservable)
        {
#if CODEGEN_TIMING
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            string errorMessages = null;
            try
            {
                var template = new TemporalArrayEgressTemplate(typeof(TKey), typeof(TPayload), typeof(TResult), "Partitioned", "StartEdge", partitionedStartEdgeObservable.source.Properties.IsColumnar);
                if (partitionedStartEdgeObservable.constructor != null)
                    template.startEdgeFunction = ((x, y) => partitionedStartEdgeObservable.constructor.Body.ExpressionToCSharpStringWithParameterSubstitution(
                                                new Dictionary<ParameterExpression, string>
                                                        {
                                                            { partitionedStartEdgeObservable.constructor.Parameters[0], "colkey[i].Key" },
                                                            { partitionedStartEdgeObservable.constructor.Parameters[0], x },
                                                            { partitionedStartEdgeObservable.constructor.Parameters[1], y },
                                                        }));
                var keyType = typeof(PartitionKey<>).MakeGenericType(typeof(TKey));
                var expandedCode = template.TransformText();

                var assemblyReferences = Transformer.AssemblyReferencesNeededFor(keyType, typeof(TPayload), typeof(TResult));
                assemblyReferences.Add(typeof(IStreamable<,>).GetTypeInfo().Assembly);
                assemblyReferences.Add(Transformer.GeneratedStreamMessageAssembly<PartitionKey<TKey>, TPayload>());

                var a = Transformer.CompileSourceCode(expandedCode, assemblyReferences, out errorMessages);
                var t = a.GetType(template.className);
#if CODEGEN_TIMING
              sw.Stop();
              Console.WriteLine("Time to generate and instantiate a IOOEJ operator: {0}ms", sw.ElapsedMilliseconds);
#endif
                return Tuple.Create(t, errorMessages);
            }
            catch
            {
#if CODEGEN_TIMING
              sw.Stop();
#endif
                if (Config.CodegenOptions.DontFallBackToRowBasedExecution)
                {
                    throw new InvalidOperationException("Code Generation failed when it wasn't supposed to!");
                }
                return Tuple.Create((Type)null, errorMessages);
            }
        }

        internal static Tuple<Type, string> Generate<TKey, TPayload, TResult>(PartitionedIntervalArrayObservable<TKey, TPayload, TResult> partitionedIntervalObservable)
        {
#if CODEGEN_TIMING
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            string errorMessages = null;
            try
            {
                var template = new TemporalArrayEgressTemplate(typeof(TKey), typeof(TPayload), typeof(TResult), "Partitioned", "Interval", partitionedIntervalObservable.source.Properties.IsColumnar);
                if (partitionedIntervalObservable.constructor != null)
                    template.intervalFunction = ((x, y, z) => partitionedIntervalObservable.constructor.Body.ExpressionToCSharpStringWithParameterSubstitution(
                                                new Dictionary<ParameterExpression, string>
                                                        {
                                                            { partitionedIntervalObservable.constructor.Parameters[0], "colkey[i].Key" },
                                                            { partitionedIntervalObservable.constructor.Parameters[0], x },
                                                            { partitionedIntervalObservable.constructor.Parameters[1], y },
                                                            { partitionedIntervalObservable.constructor.Parameters[2], z },
                                                        }));
                var keyType = typeof(PartitionKey<>).MakeGenericType(typeof(TKey));
                var expandedCode = template.TransformText();

                var assemblyReferences = Transformer.AssemblyReferencesNeededFor(keyType, typeof(TPayload), typeof(TResult));
                assemblyReferences.Add(typeof(IStreamable<,>).GetTypeInfo().Assembly);
                assemblyReferences.Add(Transformer.GeneratedStreamMessageAssembly<PartitionKey<TKey>, TPayload>());

                var a = Transformer.CompileSourceCode(expandedCode, assemblyReferences, out errorMessages);
                var t = a.GetType(template.className);
#if CODEGEN_TIMING
              sw.Stop();
              Console.WriteLine("Time to generate and instantiate a IOOEJ operator: {0}ms", sw.ElapsedMilliseconds);
#endif
                return Tuple.Create(t, errorMessages);
            }
            catch
            {
#if CODEGEN_TIMING
              sw.Stop();
#endif
                if (Config.CodegenOptions.DontFallBackToRowBasedExecution)
                {
                    throw new InvalidOperationException("Code Generation failed when it wasn't supposed to!");
                }
                return Tuple.Create((Type)null, errorMessages);
            }
        }
    }
}

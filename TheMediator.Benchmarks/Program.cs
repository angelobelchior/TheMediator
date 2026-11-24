using BenchmarkDotNet.Running;
using TheMediator.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(MediatorBenchmarks).Assembly).Run(args);

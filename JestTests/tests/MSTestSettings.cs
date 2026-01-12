// Enforce single-threaded test execution for the entire test assembly.
[assembly: Parallelize(Workers = 1, Scope = ExecutionScope.MethodLevel)]

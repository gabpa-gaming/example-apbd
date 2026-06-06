namespace EFCore_CodeFirst_Test_Example.Exceptions;

public class ConflictException(string msg) : Exception(msg);
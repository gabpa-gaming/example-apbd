namespace EFCore_CodeFirst_Test_Example.Exceptions;

public class NotFoundException(string msg) : Exception(msg);
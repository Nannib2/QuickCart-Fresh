namespace Stage0;
 partial class Program
{
    private static void Main(string[] args)
    {
        Welcome2172();
        Welcome5267();
        Console.ReadKey();
    }
     static partial void Welcome5267();
    private static void Welcome2172()
    {
        Console.WriteLine("enter your name");
        string userName = Console.ReadLine()!;
        Console.WriteLine("{0},welcome to my first console application", userName);
    }
}

using MyApp;
using System.IO;
class Program
{
    static async Task Main(string[] args)
    {
        //var p = "D:\\c#\\Practicum7sem\\prac1\\hobbit.txt";
        if (args.Length == 0)
        {
            Console.WriteLine("enter the file path on the command line");
            return;
        }
        var path = args[0];
        string text;
        using (StreamReader reader = new StreamReader(path))
        {
            text = await reader.ReadToEndAsync();
        }
        var token = new CancellationTokenSource();
        var answer = new Library(token.Token);
        await answer.Init_model();
        var task_list = new List<Task>();
        while (token.Token.IsCancellationRequested == false)
        {
            Console.WriteLine("Enter question");
            var question = Console.ReadLine();
            if(String.IsNullOrEmpty(question))
            {
                Console.WriteLine("Incorrect question");
                token.Cancel();
                return;
            }
            var task = answer.Dialog(text, question).ContinueWith(task => { Console.WriteLine(question + "' : Answer: " + task.Result); });
            task_list.Add(task);
        }
        await Task.WhenAll(task_list);
    }
}
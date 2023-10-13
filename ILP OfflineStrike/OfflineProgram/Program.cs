using OfflineProgram;

Console.WriteLine("Which file do you want?");
var file = Console.ReadLine();
KnapsackILP.Run(file);

Console.ReadLine();

InitialILP.Run(file);

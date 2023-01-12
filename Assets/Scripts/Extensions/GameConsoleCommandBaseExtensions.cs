namespace Assets.Scripts.Extensions
{
    public static class GameConsoleCommandBaseExtensions
    {
        public static int GetParametersAmount(this GameConsoleCommandBase command)
        {
            if (command is GameConsoleCommand)
            {
                return 0;
            }
            else if (command is GameConsoleCommand<string>)
            {
                return 1;
            }
            else if (command is GameConsoleCommand<string, string>)
            {
                return 2;
            }
            else if (command is GameConsoleCommand<string, string, string>)
            {
                return 3;
            }
            //else if (command is GameConsoleCommand<string, string, string, string>)
            //{
            //    return 4;
            //}
            //else if (command is GameConsoleCommand<string, string, string, string, string>)
            //{
            //    return 5;
            //}
            //else if (command is GameConsoleCommand<string, string, string, string, string, string>)
            //{
            //    return 6;
            //}
            //else if (command is GameConsoleCommand<string, string, string, string, string, string, string>)
            //{
            //    return 7;
            //}
            //else if (command is GameConsoleCommand<string, string, string, string, string, string, string, string>)
            //{
            //    return 8;
            //}
            //else if (command is GameConsoleCommand<string, string, string, string, string, string, string, string, string>)
            //{
            //    return 9;
            //}
            //else if (command is GameConsoleCommand<string, string, string, string, string, string, string, string, string, string>)
            //{
            //    return 10;
            //}
            else
            {
                return -1;
            }
        }
    }
}

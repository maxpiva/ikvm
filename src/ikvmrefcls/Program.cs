using System.Threading;
using System.Threading.Tasks;

namespace ikvmrefclsr
{

    public static class Program
    {

        /// <summary>
        /// Main application entry point.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Task<int> Main(string[] args)
        {
            return IKVM.Tools.RefClass.RefClassTool.MainAsync(args, CancellationToken.None);
        }

    }

}

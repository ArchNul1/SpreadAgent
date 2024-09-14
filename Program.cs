using System.Threading.Tasks;
using CoreLibrary;

class Program
{
    static async Task Main()
    {
        await Core.CopyFilesToDirectories();
    }
}

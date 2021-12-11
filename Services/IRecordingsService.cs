using System.Threading.Tasks;

namespace EEG_Project.Services
{
    public interface IRecordingsService
    {
        Task<double[,]> ReadRecordingFile(string path);
    }
}
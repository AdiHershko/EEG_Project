using System.Threading.Tasks;

namespace EEG_Project.Services
{
    public interface IHttpService
    {
        Task<(double[], double[])> Welch(double[,] data, int channel, int time, int hz);
        Task<(double[], double[])> Welch(double[] data, int time, int hz);

        Task Train(int numberOfParts);
        Task<string> Predict(double[] data);

    }
}